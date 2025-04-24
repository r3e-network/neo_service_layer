using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Utilities;
using NeoServiceLayer.Core.Extensions;
using NeoServiceLayer.Enclave.Enclave.Models;

namespace NeoServiceLayer.Enclave.Enclave.Services
{
    /// <summary>
    /// Enclave service for secrets operations
    /// </summary>
    public class EnclaveSecretsService
    {
        private readonly ILogger<EnclaveSecretsService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnclaveSecretsService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        public EnclaveSecretsService(ILogger<EnclaveSecretsService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Processes a secrets request
        /// </summary>
        /// <param name="request">Enclave request</param>
        /// <returns>Enclave response</returns>
        public async Task<EnclaveResponse> ProcessRequestAsync(EnclaveRequest request)
        {
            var requestId = request.RequestId ?? Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Operation"] = request.Operation,
                ["PayloadSize"] = request.Payload?.Length ?? 0
            };

            LoggingUtility.LogOperationStart(_logger, "ProcessSecretsRequest", requestId, additionalData);

            try
            {
                var result = await HandleRequestAsync(request.Operation, request.Payload);
                return new EnclaveResponse
                {
                    RequestId = requestId,
                    Success = true,
                    Payload = result
                };
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "ProcessSecretsRequest", requestId, ex, 0, additionalData);

                return new EnclaveResponse
                {
                    RequestId = requestId,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Handles a secrets request
        /// </summary>
        /// <param name="operation">The operation to perform</param>
        /// <param name="payload">The request payload</param>
        /// <returns>The result of the operation</returns>
        public async Task<byte[]> HandleRequestAsync(string operation, byte[] payload)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Operation"] = operation,
                ["PayloadSize"] = payload?.Length ?? 0
            };

            LoggingUtility.LogOperationStart(_logger, "HandleSecretsRequest", requestId, additionalData);

            try
            {
                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<EnclaveSecretsService, byte[]>(
                    _logger,
                    async () =>
                    {
                        switch (operation)
                        {
                            case Constants.SecretsOperations.CreateSecret:
                                return await CreateSecretAsync(payload);
                            case Constants.SecretsOperations.GetSecret:
                                return await GetSecretValueAsync(payload);
                            case Constants.SecretsOperations.UpdateSecret:
                                return await UpdateValueAsync(payload);
                            case Constants.SecretsOperations.DeleteSecret:
                                return await DeleteSecretAsync(payload);
                            case Constants.SecretsOperations.UpdateSecretValue:
                                return await UpdateValueAsync(payload);
                            default:
                                throw new InvalidOperationException($"Unknown operation: {operation}");
                        }
                    },
                    operation,
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new InvalidOperationException($"Failed to process secrets request: {operation}");
                }

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "HandleSecretsRequest", requestId, ex, 0, additionalData);
                throw;
            }
        }

        private async Task<byte[]> CreateSecretAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<CreateSecretRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateNotNullOrEmpty(request.Name, "Secret name");
            ValidationUtility.ValidateNotNullOrEmpty(request.Value, "Secret value");
            ValidationUtility.ValidateGuid(request.AccountId, "Account ID");

            // Generate a unique ID for the secret
            var secretId = Guid.NewGuid();

            // Generate a unique encryption key for this secret
            var encryptionKey = EncryptionUtility.GenerateRandomKey();

            // Encrypt the secret value
            var encryptedValue = EncryptionUtility.EncryptWithKey(request.Value, encryptionKey);

            // Create the secret object
            var secret = new Secret
            {
                Id = secretId,
                Name = request.Name,
                Description = request.Description,
                AccountId = request.AccountId,
                AllowedFunctionIds = request.AllowedFunctionIds ?? new List<Guid>(),
                Tags = request.Tags ?? new Dictionary<string, string>(),
                EncryptedValue = encryptedValue,
                Version = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastRotatedAt = DateTime.UtcNow,
                NextRotationAt = request.RotationPeriod.HasValue ? DateTime.UtcNow.AddDays(request.RotationPeriod.Value) : (DateTime?)null,
                RotationPeriod = request.RotationPeriod
            };

            // Store the secret in the secure storage
            await StoreSecretAsync(secret, encryptionKey);

            // Log the creation (without sensitive data)
            LoggingUtility.LogSecurityEvent(_logger, "SecretCreated", Guid.NewGuid().ToString(),
                request.AccountId.ToString(), "Secret", secretId.ToString(), "Create", "Success");

            // Create response (without sensitive data)
            var response = new
            {
                Id = secret.Id,
                Name = secret.Name,
                Description = secret.Description,
                AccountId = secret.AccountId,
                AllowedFunctionIds = secret.AllowedFunctionIds,
                Tags = secret.Tags,
                Version = secret.Version,
                CreatedAt = secret.CreatedAt,
                UpdatedAt = secret.UpdatedAt,
                LastRotatedAt = secret.LastRotatedAt,
                NextRotationAt = secret.NextRotationAt,
                RotationPeriod = secret.RotationPeriod
            };

            return JsonUtility.SerializeToUtf8Bytes(response);
        }

        // These methods are now replaced by EncryptionUtility

        private async Task StoreSecretAsync(Secret secret, byte[] encryptionKey)
        {
            // Store the secret in the secure storage
            // In a production environment, this would use a secure storage mechanism
            // such as a hardware security module (HSM) or a secure database

            // For now, we'll simulate storage
            await Task.Delay(10); // Placeholder for actual storage operation

            // In a real implementation, we would also securely store the encryption key
            // associated with this secret, possibly using a key management service
        }

        private async Task<byte[]> GetSecretValueAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<GetSecretValueRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.SecretId, "Secret ID");
            ValidationUtility.ValidateGuid(request.AccountId, "Account ID");

            // Check if the function has access to this secret
            if (request.FunctionId.HasValue)
            {
                ValidationUtility.ValidateGuid(request.FunctionId.Value, "Function ID");
                var hasAccess = await CheckSecretAccessAsync(request.SecretId, request.AccountId, request.FunctionId.Value);
                if (!hasAccess)
                {
                    LoggingUtility.LogSecurityEvent(_logger, "SecretAccessDenied", Guid.NewGuid().ToString(),
                        request.AccountId.ToString(), "Secret", request.SecretId.ToString(), "Access", "Denied",
                        new Dictionary<string, object> { ["FunctionId"] = request.FunctionId.Value });

                    throw new UnauthorizedAccessException("Function does not have access to this secret");
                }
            }

            // Retrieve the secret and its encryption key
            var (secret, encryptionKey) = await RetrieveSecretAsync(request.SecretId, request.AccountId);

            if (secret == null || encryptionKey == null)
            {
                throw new KeyNotFoundException("Secret not found");
            }

            // Decrypt the secret value
            var decryptedValue = EncryptionUtility.DecryptWithKey(secret.EncryptedValue, encryptionKey);

            // Create response with the decrypted value
            var response = new
            {
                Id = secret.Id,
                Name = secret.Name,
                Value = decryptedValue,
                Version = secret.Version
            };

            // Log access to the secret (but not the value itself)
            LoggingUtility.LogSecurityEvent(_logger, "SecretAccessed", Guid.NewGuid().ToString(),
                request.AccountId.ToString(), "Secret", secret.Id.ToString(), "Access", "Success",
                new Dictionary<string, object>
                {
                    ["Name"] = secret.Name,
                    ["Version"] = secret.Version,
                    ["FunctionId"] = request.FunctionId
                });

            return JsonUtility.SerializeToUtf8Bytes(response);
        }

        // This method is now replaced by EncryptionUtility

        private async Task<(Secret, byte[])> RetrieveSecretAsync(Guid secretId, Guid accountId)
        {
            // In a production environment, this would retrieve the secret and its encryption key
            // from a secure storage mechanism such as a hardware security module (HSM) or a secure database

            // For now, we'll simulate retrieval with a placeholder secret
            await Task.Delay(10); // Placeholder for actual retrieval operation

            // Create a placeholder secret and encryption key
            // In a real implementation, these would be retrieved from secure storage
            var secret = new Secret
            {
                Id = secretId,
                Name = "api-key",
                Description = "API key for external service",
                AccountId = accountId,
                AllowedFunctionIds = new List<Guid>(),
                Tags = new Dictionary<string, string>(),
                EncryptedValue = "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8gISIjJCUmJygpKissLS4vMDEyMzQ1Njc4OTo7PD0+P0BBQkNERUZHSElKS0xNTk9QUVJTVFVWV1hZWltcXV5fYGFiY2RlZmdoaWprbG1ub3BxcnN0dXZ3eHl6e3x9fn8=", // Placeholder encrypted value
                Version = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30),
                LastRotatedAt = DateTime.UtcNow.AddDays(-30),
                NextRotationAt = DateTime.UtcNow.AddDays(60),
                RotationPeriod = 90
            };

            // Generate a placeholder encryption key
            // In a real implementation, this would be retrieved from a key management service
            var encryptionKey = new byte[32];
            for (int i = 0; i < encryptionKey.Length; i++)
            {
                encryptionKey[i] = (byte)i; // Placeholder key for demonstration
            }

            return (secret, encryptionKey);
        }

        private async Task<bool> CheckSecretAccessAsync(Guid secretId, Guid accountId, Guid functionId)
        {
            // In a production environment, this would check if the function has access to the secret
            // by retrieving the secret's allowed function IDs and checking if the function ID is in the list

            // For now, we'll simulate access checking
            await Task.Delay(10); // Placeholder for actual access check

            // Always return true for demonstration purposes
            // In a real implementation, this would check the secret's AllowedFunctionIds list
            return true;
        }

        private async Task<byte[]> UpdateValueAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<UpdateSecretValueRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.SecretId, "Secret ID");
            ValidationUtility.ValidateGuid(request.AccountId, "Account ID");
            ValidationUtility.ValidateNotNullOrEmpty(request.Value, "Secret value");

            // Retrieve the secret and its encryption key
            var (secret, encryptionKey) = await RetrieveSecretAsync(request.SecretId, request.AccountId);

            if (secret == null || encryptionKey == null)
            {
                throw new KeyNotFoundException("Secret not found");
            }

            // Generate a new encryption key for the updated value
            var newEncryptionKey = EncryptionUtility.GenerateRandomKey();

            // Encrypt the new value
            var encryptedValue = EncryptionUtility.EncryptWithKey(request.Value, newEncryptionKey);

            // Update the secret
            secret.EncryptedValue = encryptedValue;
            secret.Version += 1;
            secret.UpdatedAt = DateTime.UtcNow;

            if (request.Description != null)
            {
                secret.Description = request.Description;
            }

            if (request.AllowedFunctionIds != null)
            {
                secret.AllowedFunctionIds = request.AllowedFunctionIds;
            }

            if (request.Tags != null)
            {
                secret.Tags = request.Tags;
            }

            if (request.RotationPeriod.HasValue)
            {
                secret.RotationPeriod = request.RotationPeriod;
                secret.NextRotationAt = DateTime.UtcNow.AddDays(request.RotationPeriod.Value);
            }

            // Store the updated secret
            await StoreSecretAsync(secret, newEncryptionKey);

            // Create response (without sensitive data)
            var response = new
            {
                Id = secret.Id,
                Name = secret.Name,
                Description = secret.Description,
                AccountId = secret.AccountId,
                AllowedFunctionIds = secret.AllowedFunctionIds,
                Tags = secret.Tags,
                Version = secret.Version,
                UpdatedAt = secret.UpdatedAt,
                LastRotatedAt = secret.LastRotatedAt,
                NextRotationAt = secret.NextRotationAt,
                RotationPeriod = secret.RotationPeriod
            };

            // Log the update (but not the value itself)
            LoggingUtility.LogSecurityEvent(_logger, "SecretUpdated", Guid.NewGuid().ToString(),
                request.AccountId.ToString(), "Secret", secret.Id.ToString(), "Update", "Success",
                new Dictionary<string, object>
                {
                    ["Name"] = secret.Name,
                    ["Version"] = secret.Version
                });

            return JsonUtility.SerializeToUtf8Bytes(response);
        }

        private async Task<byte[]> RotateSecretAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<RotateSecretRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.SecretId, "Secret ID");
            ValidationUtility.ValidateGuid(request.AccountId, "Account ID");

            // Retrieve the secret and its encryption key
            var (secret, encryptionKey) = await RetrieveSecretAsync(request.SecretId, request.AccountId);

            if (secret == null || encryptionKey == null)
            {
                throw new KeyNotFoundException("Secret not found");
            }

            // Decrypt the current value
            var currentValue = EncryptionUtility.DecryptWithKey(secret.EncryptedValue, encryptionKey);

            // Generate a new value based on the secret type
            string newValue;
            if (request.NewValue != null)
            {
                // Use the provided new value
                newValue = request.NewValue;
            }
            else
            {
                // Generate a new value based on the secret type
                newValue = await GenerateNewSecretValueAsync(secret, currentValue);
            }

            // Generate a new encryption key
            var newEncryptionKey = EncryptionUtility.GenerateRandomKey();

            // Encrypt the new value
            var encryptedValue = EncryptionUtility.EncryptWithKey(newValue, newEncryptionKey);

            // Update the secret
            secret.EncryptedValue = encryptedValue;
            secret.Version += 1;
            secret.UpdatedAt = DateTime.UtcNow;
            secret.LastRotatedAt = DateTime.UtcNow;

            if (secret.RotationPeriod.HasValue)
            {
                secret.NextRotationAt = DateTime.UtcNow.AddDays(secret.RotationPeriod.Value);
            }

            // Store the updated secret
            await StoreSecretAsync(secret, newEncryptionKey);

            // Create response (without sensitive data)
            var response = new
            {
                Id = secret.Id,
                Name = secret.Name,
                Description = secret.Description,
                AccountId = secret.AccountId,
                AllowedFunctionIds = secret.AllowedFunctionIds,
                Tags = secret.Tags,
                Version = secret.Version,
                UpdatedAt = secret.UpdatedAt,
                LastRotatedAt = secret.LastRotatedAt,
                NextRotationAt = secret.NextRotationAt,
                RotationPeriod = secret.RotationPeriod
            };

            // Log the rotation (but not the value itself)
            LoggingUtility.LogSecurityEvent(_logger, "SecretRotated", Guid.NewGuid().ToString(),
                request.AccountId.ToString(), "Secret", secret.Id.ToString(), "Rotate", "Success",
                new Dictionary<string, object>
                {
                    ["Name"] = secret.Name,
                    ["Version"] = secret.Version
                });

            return JsonUtility.SerializeToUtf8Bytes(response);
        }

        private async Task<byte[]> DeleteSecretAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<DeleteSecretRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.SecretId, "Secret ID");
            ValidationUtility.ValidateGuid(request.AccountId, "Account ID");

            // Retrieve the secret
            var (secret, _) = await RetrieveSecretAsync(request.SecretId, request.AccountId);

            if (secret == null)
            {
                throw new KeyNotFoundException("Secret not found");
            }

            // Delete the secret
            await DeleteSecretFromStorageAsync(secret.Id, secret.AccountId);

            // Create response
            var response = new
            {
                Id = secret.Id,
                Name = secret.Name,
                Deleted = true
            };

            // Log the deletion
            LoggingUtility.LogSecurityEvent(_logger, "SecretDeleted", Guid.NewGuid().ToString(),
                request.AccountId.ToString(), "Secret", secret.Id.ToString(), "Delete", "Success",
                new Dictionary<string, object>
                {
                    ["Name"] = secret.Name
                });

            return JsonUtility.SerializeToUtf8Bytes(response);
        }

        private async Task DeleteSecretFromStorageAsync(Guid secretId, Guid accountId)
        {
            // In a production environment, this would delete the secret from a secure storage mechanism
            // For now, we'll simulate deletion
            await Task.Delay(10); // Placeholder for actual deletion operation
        }

        private async Task<string> GenerateNewSecretValueAsync(Secret secret, string currentValue)
        {
            // Generate a new value based on the secret type
            // This is a simplified implementation that generates random values
            // In a real implementation, this would be more sophisticated and handle different types of secrets

            // Check if the secret has a type tag
            if (secret.Tags != null && secret.Tags.TryGetValue("type", out var secretType))
            {
                switch (secretType.ToLower())
                {
                    case "api-key":
                        return Convert.ToBase64String(EncryptionUtility.GenerateRandomKey());
                    case "password":
                        return EncryptionUtility.GenerateRandomPassword(16);
                    case "connection-string":
                        return GenerateConnectionString(currentValue);
                    default:
                        return GenerateRandomString(32);
                }
            }

            // Default to generating a random string
            return GenerateRandomString(32);
        }

        private string GenerateConnectionString(string currentValue)
        {
            // Parse the current connection string and update the password
            // This is a simplified implementation that assumes the connection string has a password parameter
            // In a real implementation, this would be more sophisticated and handle different types of connection strings

            // Generate a new password
            var newPassword = EncryptionUtility.GenerateRandomPassword(16);

            // Replace the password in the connection string
            // This is a very simplified approach and would need to be more robust in a real implementation
            if (currentValue.Contains("Password="))
            {
                var parts = currentValue.Split(';');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                    {
                        parts[i] = $"Password={newPassword}";
                    }
                }

                return string.Join(";", parts);
            }

            // If we can't find a password parameter, just return the current value
            return currentValue;
        }

        private string GenerateRandomString(int length)
        {
            // Generate a random string of the specified length
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new char[length];
            var randomBytes = new byte[length];

            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = chars[randomBytes[i] % chars.Length];
            }

            return new string(result);
        }

        private async Task<byte[]> HasAccessAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<CheckSecretAccessRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.SecretId, "Secret ID");
            ValidationUtility.ValidateGuid(request.AccountId, "Account ID");
            ValidationUtility.ValidateGuid(request.FunctionId, "Function ID");

            // Check if the function has access to the secret
            var hasAccess = await CheckSecretAccessAsync(request.SecretId, request.AccountId, request.FunctionId);

            // Create response
            var response = new
            {
                HasAccess = hasAccess,
                SecretId = request.SecretId,
                FunctionId = request.FunctionId
            };

            // Log the access check
            LoggingUtility.LogSecurityEvent(_logger, "SecretAccessCheck", Guid.NewGuid().ToString(),
                request.AccountId.ToString(), "Secret", request.SecretId.ToString(), "AccessCheck", hasAccess ? "Granted" : "Denied",
                new Dictionary<string, object>
                {
                    ["FunctionId"] = request.FunctionId
                });

            return JsonUtility.SerializeToUtf8Bytes(response);
        }

        /// <summary>
        /// Request model for creating a secret
        /// </summary>
        private class CreateSecretRequest
        {
            /// <summary>
            /// Gets or sets the name of the secret
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the description of the secret
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the value of the secret
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the list of function IDs that are allowed to access this secret
            /// </summary>
            public List<Guid> AllowedFunctionIds { get; set; }

            /// <summary>
            /// Gets or sets the tags for the secret
            /// </summary>
            public Dictionary<string, string> Tags { get; set; }

            /// <summary>
            /// Gets or sets the rotation period in days
            /// </summary>
            public int? RotationPeriod { get; set; }
        }

        /// <summary>
        /// Request model for getting a secret value
        /// </summary>
        private class GetSecretValueRequest
        {
            /// <summary>
            /// Gets or sets the secret ID
            /// </summary>
            public Guid SecretId { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the function ID
            /// </summary>
            public Guid? FunctionId { get; set; }
        }

        /// <summary>
        /// Request model for updating a secret value
        /// </summary>
        private class UpdateSecretValueRequest
        {
            /// <summary>
            /// Gets or sets the secret ID
            /// </summary>
            public Guid SecretId { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the value of the secret
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the description of the secret
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the list of function IDs that are allowed to access this secret
            /// </summary>
            public List<Guid> AllowedFunctionIds { get; set; }

            /// <summary>
            /// Gets or sets the tags for the secret
            /// </summary>
            public Dictionary<string, string> Tags { get; set; }

            /// <summary>
            /// Gets or sets the rotation period in days
            /// </summary>
            public int? RotationPeriod { get; set; }
        }

        /// <summary>
        /// Request model for rotating a secret
        /// </summary>
        private class RotateSecretRequest
        {
            /// <summary>
            /// Gets or sets the secret ID
            /// </summary>
            public Guid SecretId { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the new value of the secret (optional)
            /// </summary>
            public string NewValue { get; set; }
        }

        /// <summary>
        /// Request model for checking secret access
        /// </summary>
        private class CheckSecretAccessRequest
        {
            /// <summary>
            /// Gets or sets the secret ID
            /// </summary>
            public Guid SecretId { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the function ID
            /// </summary>
            public Guid FunctionId { get; set; }
        }
    }
}
