using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Secrets.Repositories;

namespace NeoServiceLayer.Services.Secrets
{
    /// <summary>
    /// Implementation of the secrets service
    /// </summary>
    public class SecretsService : ISecretsService
    {
        private readonly ILogger<SecretsService> _logger;
        private readonly ISecretsRepository _secretsRepository;
        private readonly IEnclaveService _enclaveService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretsService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="secretsRepository">Secrets repository</param>
        /// <param name="enclaveService">Enclave service</param>
        public SecretsService(ILogger<SecretsService> logger, ISecretsRepository secretsRepository, IEnclaveService enclaveService)
        {
            _logger = logger;
            _secretsRepository = secretsRepository;
            _enclaveService = enclaveService;
        }

        /// <inheritdoc/>
        public async Task<Secret> CreateSecretAsync(string name, string value, string description, Guid accountId, List<Guid> allowedFunctionIds, DateTime? expiresAt = null)
        {
            _logger.LogInformation("Creating secret: {Name}, AccountId: {AccountId}", name, accountId);

            // Check if secret with same name already exists for this account
            if (await _secretsRepository.GetByNameAsync(name, accountId) != null)
            {
                throw new SecretsException("Secret with this name already exists");
            }

            try
            {
                // Send secret creation request to enclave
                var secretRequest = new
                {
                    Name = name,
                    Value = value,
                    Description = description,
                    AccountId = accountId,
                    AllowedFunctionIds = allowedFunctionIds,
                    ExpiresAt = expiresAt
                };

                var secret = await _enclaveService.SendRequestAsync<object, Secret>(
                    Constants.EnclaveServiceTypes.Secrets,
                    Constants.SecretsOperations.CreateSecret,
                    secretRequest);

                // Save secret to repository
                await _secretsRepository.CreateAsync(secret);

                _logger.LogInformation("Secret created successfully: {Id}, Name: {Name}", secret.Id, secret.Name);
                return secret;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating secret: {Name}", name);
                throw new SecretsException("Error creating secret", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Secret> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting secret by ID: {Id}", id);
            return await _secretsRepository.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<Secret> GetByNameAsync(string name, Guid accountId)
        {
            _logger.LogInformation("Getting secret by name: {Name}, AccountId: {AccountId}", name, accountId);
            return await _secretsRepository.GetByNameAsync(name, accountId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Secret>> GetByAccountIdAsync(Guid accountId)
        {
            _logger.LogInformation("Getting secrets by account ID: {AccountId}", accountId);
            return await _secretsRepository.GetByAccountIdAsync(accountId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Secret>> GetByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Getting secrets by function ID: {FunctionId}", functionId);
            return await _secretsRepository.GetByFunctionIdAsync(functionId);
        }

        /// <inheritdoc/>
        public async Task<Secret> UpdateAsync(Secret secret)
        {
            _logger.LogInformation("Updating secret: {Id}", secret.Id);
            return await _secretsRepository.UpdateAsync(secret);
        }

        /// <inheritdoc/>
        public async Task<Secret> UpdateValueAsync(Guid id, string value)
        {
            _logger.LogInformation("Updating value for secret: {Id}", id);

            var secret = await _secretsRepository.GetByIdAsync(id);
            if (secret == null)
            {
                throw new SecretsException("Secret not found");
            }

            try
            {
                // Send value update request to enclave
                var updateRequest = new
                {
                    Id = id,
                    Value = value
                };

                var updatedSecret = await _enclaveService.SendRequestAsync<object, Secret>(
                    Constants.EnclaveServiceTypes.Secrets,
                    Constants.SecretsOperations.UpdateValue,
                    updateRequest);

                // Update secret in repository
                secret.Version = updatedSecret.Version;
                secret.EncryptedValue = updatedSecret.EncryptedValue;
                secret.UpdatedAt = DateTime.UtcNow;

                await _secretsRepository.UpdateAsync(secret);

                _logger.LogInformation("Secret value updated successfully: {Id}, Version: {Version}", secret.Id, secret.Version);
                return secret;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating secret value: {Id}", id);
                throw new SecretsException("Error updating secret value", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Secret> UpdateAllowedFunctionsAsync(Guid id, List<Guid> allowedFunctionIds)
        {
            _logger.LogInformation("Updating allowed functions for secret: {Id}", id);

            var secret = await _secretsRepository.GetByIdAsync(id);
            if (secret == null)
            {
                throw new SecretsException("Secret not found");
            }

            secret.AllowedFunctionIds = allowedFunctionIds;
            secret.UpdatedAt = DateTime.UtcNow;

            await _secretsRepository.UpdateAsync(secret);

            _logger.LogInformation("Secret allowed functions updated successfully: {Id}", secret.Id);
            return secret;
        }

        /// <inheritdoc/>
        public async Task<string> GetSecretValueAsync(Guid id, Guid functionId)
        {
            _logger.LogInformation("Getting value for secret: {Id}, FunctionId: {FunctionId}", id, functionId);

            // Check if function has access to the secret
            if (!await HasAccessAsync(id, functionId))
            {
                throw new SecretsException("Function does not have access to this secret");
            }

            try
            {
                // Send get value request to enclave
                var getValueRequest = new
                {
                    Id = id,
                    FunctionId = functionId
                };

                var result = await _enclaveService.SendRequestAsync<object, object>(
                    Constants.EnclaveServiceTypes.Secrets,
                    Constants.SecretsOperations.GetSecretValue,
                    getValueRequest);

                // Extract value from result
                var value = result.GetType().GetProperty("Value")?.GetValue(result)?.ToString();

                _logger.LogInformation("Secret value retrieved successfully: {Id}", id);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting secret value: {Id}", id);
                throw new SecretsException("Error getting secret value", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Secret> RotateSecretAsync(Guid id, string newValue)
        {
            _logger.LogInformation("Rotating secret: {Id}", id);

            var secret = await _secretsRepository.GetByIdAsync(id);
            if (secret == null)
            {
                throw new SecretsException("Secret not found");
            }

            try
            {
                // Send rotation request to enclave
                var rotationRequest = new
                {
                    Id = id,
                    NewValue = newValue
                };

                var rotatedSecret = await _enclaveService.SendRequestAsync<object, Secret>(
                    Constants.EnclaveServiceTypes.Secrets,
                    Constants.SecretsOperations.RotateSecret,
                    rotationRequest);

                // Update secret in repository
                secret.Version = rotatedSecret.Version;
                secret.EncryptedValue = rotatedSecret.EncryptedValue;
                secret.UpdatedAt = DateTime.UtcNow;

                await _secretsRepository.UpdateAsync(secret);

                _logger.LogInformation("Secret rotated successfully: {Id}, Version: {Version}", secret.Id, secret.Version);
                return secret;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rotating secret: {Id}", id);
                throw new SecretsException("Error rotating secret", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting secret: {Id}", id);
            return await _secretsRepository.DeleteAsync(id);
        }

        /// <inheritdoc/>
        public async Task<bool> HasAccessAsync(Guid secretId, Guid functionId)
        {
            _logger.LogInformation("Checking if function has access to secret: {SecretId}, FunctionId: {FunctionId}", secretId, functionId);

            var secret = await _secretsRepository.GetByIdAsync(secretId);
            if (secret == null)
            {
                return false;
            }

            // Check if the secret has expired
            if (secret.ExpiresAt.HasValue && secret.ExpiresAt.Value < DateTime.UtcNow)
            {
                return false;
            }

            // Check if the function is in the allowed functions list
            var hasAccess = secret.AllowedFunctionIds.Contains(functionId);

            // If the function is not in the allowed list, return false immediately
            if (!hasAccess)
            {
                return false;
            }

            try
            {
                // Send access check request to enclave for additional validation
                var accessRequest = new
                {
                    SecretId = secretId,
                    FunctionId = functionId
                };

                var result = await _enclaveService.SendRequestAsync<object, object>(
                    Constants.EnclaveServiceTypes.Secrets,
                    Constants.SecretsOperations.HasAccess,
                    accessRequest);

                // Extract access result from result
                var enclaveHasAccess = (bool)result.GetType().GetProperty("HasAccess")?.GetValue(result);

                // Both repository and enclave must grant access
                return enclaveHasAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking secret access: {SecretId}, FunctionId: {FunctionId}", secretId, functionId);
                return false;
            }
        }
    }
}
