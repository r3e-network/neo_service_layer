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
    /// Enclave service for wallet operations
    /// </summary>
    public class EnclaveWalletService
    {
        private readonly ILogger<EnclaveWalletService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnclaveWalletService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        public EnclaveWalletService(ILogger<EnclaveWalletService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Processes a wallet request
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

            LoggingUtility.LogOperationStart(_logger, "ProcessWalletRequest", requestId, additionalData);

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
                LoggingUtility.LogOperationFailure(_logger, "ProcessWalletRequest", requestId, ex, 0, additionalData);

                return new EnclaveResponse
                {
                    RequestId = requestId,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Handles a wallet request
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

            LoggingUtility.LogOperationStart(_logger, "HandleWalletRequest", requestId, additionalData);

            try
            {
                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<EnclaveWalletService, byte[]>(
                    _logger,
                    async () =>
                    {
                        switch (operation)
                        {
                            case Constants.WalletOperations.CreateWallet:
                                return await CreateWalletAsync(payload);
                            case "importFromWIF":
                                return await ImportFromWIFAsync(payload);
                            case Constants.WalletOperations.SignData:
                                return await SignDataInternalAsync(payload);
                            case Constants.WalletOperations.TransferNeo:
                                return await TransferNeoAsync(payload);
                            case Constants.WalletOperations.TransferGas:
                                return await TransferGasAsync(payload);
                            case Constants.WalletOperations.TransferToken:
                                return await TransferTokenAsync(payload);
                            default:
                                throw new InvalidOperationException($"Unknown operation: {operation}");
                        }
                    },
                    operation,
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new InvalidOperationException($"Failed to process wallet request: {operation}");
                }

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "HandleWalletRequest", requestId, ex, 0, additionalData);
                throw;
            }
        }

        private async Task<byte[]> CreateWalletAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<CreateWalletRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateNotNullOrEmpty(request.Name, "Wallet name");
            ValidationUtility.ValidateGuid(request.AccountId, "Account ID");
            ValidationUtility.ValidateNotNullOrEmpty(request.Password, "Password");

            // Generate a new Neo N3 wallet
            var (privateKey, publicKey, address, scriptHash) = GenerateNeoWallet();

            // Create the wallet object
            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                AccountId = request.AccountId,
                Address = address,
                ScriptHash = scriptHash,
                PublicKey = publicKey,
                EncryptedPrivateKey = EncryptionUtility.EncryptWithPassword(privateKey, request.Password),
                Tags = request.Tags ?? new Dictionary<string, string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Store the wallet securely
            await StoreWalletAsync(wallet);

            // Create response (without sensitive data)
            var response = new
            {
                Id = wallet.Id,
                Name = wallet.Name,
                AccountId = wallet.AccountId,
                Address = wallet.Address,
                ScriptHash = wallet.ScriptHash,
                PublicKey = wallet.PublicKey,
                Tags = wallet.Tags,
                CreatedAt = wallet.CreatedAt
            };

            // Log wallet creation (without sensitive data)
            LoggingUtility.LogSecurityEvent(_logger, "WalletCreated", Guid.NewGuid().ToString(),
                request.AccountId.ToString(), "Wallet", wallet.Id.ToString(), "Create", "Success",
                new Dictionary<string, object>
                {
                    ["Name"] = wallet.Name,
                    ["Address"] = wallet.Address
                });

            return JsonUtility.SerializeToUtf8Bytes(response);
        }

        private (string privateKey, string publicKey, string address, string scriptHash) GenerateNeoWallet()
        {
            // In a production environment, this would use the Neo SDK to generate a wallet
            // For now, we'll simulate wallet generation with placeholder values

            // Generate a random private key
            var privateKeyBytes = EncryptionUtility.GenerateRandomKey();
            var privateKey = Convert.ToBase64String(privateKeyBytes);

            // In a real implementation, we would derive the public key, address, and script hash from the private key
            // using the Neo SDK. For now, we'll use placeholder values.
            var publicKey = "02" + Convert.ToBase64String(privateKeyBytes).Substring(0, 64);
            var address = "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA";
            var scriptHash = "0x" + BitConverter.ToString(privateKeyBytes.Take(20).ToArray()).Replace("-", "").ToLower();

            return (privateKey, publicKey, address, scriptHash);
        }

        private string EncryptPrivateKey(string privateKey, string password)
        {
            // This method is now replaced by EncryptionUtility.EncryptWithPassword
            // but we keep it for backward compatibility
            return EncryptionUtility.EncryptWithPassword(privateKey, password);
        }

        private async Task StoreWalletAsync(Wallet wallet)
        {
            // In a production environment, this would store the wallet in a secure storage mechanism
            // such as a hardware security module (HSM) or a secure database

            // For now, we'll simulate storage
            await Task.Delay(10); // Placeholder for actual storage operation
        }

        private async Task<byte[]> ImportFromWIFAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<ImportWalletRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateNotNullOrEmpty(request.Name, "Wallet name");
            ValidationUtility.ValidateNotNullOrEmpty(request.WIF, "WIF");
            ValidationUtility.ValidateGuid(request.AccountId, "Account ID");
            ValidationUtility.ValidateNotNullOrEmpty(request.Password, "Password");

            // Validate WIF format
            if (!request.WIF.IsValidWif())
            {
                throw new ArgumentException("Invalid WIF format");
            }

            // Import the Neo N3 wallet from WIF
            var (privateKey, publicKey, address, scriptHash) = ImportNeoWalletFromWIF(request.WIF);

            // Create the wallet object
            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                AccountId = request.AccountId,
                Address = address,
                ScriptHash = scriptHash,
                PublicKey = publicKey,
                EncryptedPrivateKey = EncryptionUtility.EncryptWithPassword(privateKey, request.Password),
                Tags = request.Tags ?? new Dictionary<string, string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Store the wallet securely
            await StoreWalletAsync(wallet);

            // Create response (without sensitive data)
            var response = new
            {
                Id = wallet.Id,
                Name = wallet.Name,
                AccountId = wallet.AccountId,
                Address = wallet.Address,
                ScriptHash = wallet.ScriptHash,
                PublicKey = wallet.PublicKey,
                Tags = wallet.Tags,
                CreatedAt = wallet.CreatedAt
            };

            // Log wallet import (without sensitive data)
            LoggingUtility.LogSecurityEvent(_logger, "WalletImported", Guid.NewGuid().ToString(),
                request.AccountId.ToString(), "Wallet", wallet.Id.ToString(), "Import", "Success",
                new Dictionary<string, object>
                {
                    ["Name"] = wallet.Name,
                    ["Address"] = wallet.Address
                });

            return JsonUtility.SerializeToUtf8Bytes(response);
        }

        private (string privateKey, string publicKey, string address, string scriptHash) ImportNeoWalletFromWIF(string wif)
        {
            // In a production environment, this would use the Neo SDK to import a wallet from WIF
            // For now, we'll simulate wallet import with placeholder values

            // Validate WIF format
            if (!IsValidWIF(wif))
            {
                throw new ArgumentException("Invalid WIF format");
            }

            // In a real implementation, we would derive the private key, public key, address, and script hash from the WIF
            // using the Neo SDK. For now, we'll use placeholder values.
            var privateKey = "base64encodedprivatekeyderivedfromwif";
            var publicKey = "02a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2";
            var address = "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA";
            var scriptHash = "0x1234567890abcdef1234567890abcdef12345678";

            return (privateKey, publicKey, address, scriptHash);
        }

        private bool IsValidWIF(string wif)
        {
            // This method is now replaced by StringExtensions.IsValidWif
            // but we keep it for backward compatibility
            return wif.IsValidWif();
        }

        public async Task<string> SignDataAsync(byte[] data)
        {
            // Create a sign data request
            var request = new SignDataRequest
            {
                // Use a default wallet for signing price data
                WalletId = Guid.NewGuid(), // This would be a configured wallet ID in production
                AccountId = Guid.NewGuid(), // This would be a configured account ID in production
                Data = data,
                Password = "password" // This would be a securely stored password in production
            };

            // Serialize the request to JSON
            var payload = JsonUtility.SerializeToUtf8Bytes(request);

            // Call the protected SignDataAsync method
            var responseBytes = await SignDataInternalAsync(payload);

            // Deserialize the response
            var response = JsonUtility.Deserialize<dynamic>(responseBytes);

            // Return the signature
            return response.Signature;
        }

        protected async Task<byte[]> SignDataInternalAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<SignDataRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.WalletId, "Wallet ID");
            ValidationUtility.ValidateGuid(request.AccountId, "Account ID");
            ValidationUtility.ValidateNotNull(request.Data, "Data to sign");

            if (request.Data.Length == 0)
            {
                throw new ArgumentException("Data to sign cannot be empty");
            }

            // Retrieve the wallet
            var wallet = await RetrieveWalletAsync(request.WalletId, request.AccountId);

            if (wallet == null)
            {
                throw new KeyNotFoundException("Wallet not found");
            }

            // Decrypt the private key
            var privateKey = EncryptionUtility.DecryptWithPassword(wallet.EncryptedPrivateKey, request.Password);

            // Sign the data
            var signature = SignDataWithPrivateKey(request.Data, privateKey);

            // Create response
            var response = new
            {
                WalletId = wallet.Id,
                Address = wallet.Address,
                Signature = signature
            };

            // Log data signing (without sensitive data)
            LoggingUtility.LogSecurityEvent(_logger, "DataSigned", Guid.NewGuid().ToString(),
                request.AccountId.ToString(), "Wallet", wallet.Id.ToString(), "Sign", "Success",
                new Dictionary<string, object>
                {
                    ["Address"] = wallet.Address,
                    ["DataSize"] = request.Data.Length
                });

            return JsonUtility.SerializeToUtf8Bytes(response);
        }

        private async Task<Wallet> RetrieveWalletAsync(Guid walletId, Guid accountId)
        {
            // In a production environment, this would retrieve the wallet from a secure storage mechanism
            // For now, we'll simulate retrieval with a placeholder wallet
            await Task.Delay(10); // Placeholder for actual retrieval operation

            // Create a placeholder wallet
            // In a real implementation, this would be retrieved from secure storage
            var wallet = new Wallet
            {
                Id = walletId,
                Name = "My Wallet",
                AccountId = accountId,
                Address = "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
                ScriptHash = "0x1234567890abcdef1234567890abcdef12345678",
                PublicKey = "02a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2",
                EncryptedPrivateKey = "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8gISIjJCUmJygpKissLS4vMDEyMzQ1Njc4OTo7PD0+P0BBQkNERUZHSElKS0xNTk9QUVJTVFVWV1hZWltcXV5fYGFiY2RlZmdoaWprbG1ub3BxcnN0dXZ3eHl6e3x9fn8=", // Placeholder encrypted private key
                Tags = new Dictionary<string, string>(),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            };

            return wallet;
        }

        private string DecryptPrivateKey(string encryptedPrivateKey, string password)
        {
            // This method is now replaced by EncryptionUtility.DecryptWithPassword
            // but we keep it for backward compatibility
            return EncryptionUtility.DecryptWithPassword(encryptedPrivateKey, password);
        }

        private string SignDataWithPrivateKey(byte[] data, string privateKey)
        {
            // In a production environment, this would use the Neo SDK to sign data with the private key
            // For now, we'll simulate signing with a placeholder signature

            // Generate a deterministic signature based on the data and private key
            using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(privateKey)))
            {
                var signatureBytes = hmac.ComputeHash(data);
                return "0x" + BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();
            }
        }

        private async Task<byte[]> TransferNeoAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<TransferNeoRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.WalletId, "Wallet ID");
            ValidationUtility.ValidateGuid(request.AccountId, "Account ID");
            ValidationUtility.ValidateNotNullOrEmpty(request.ToAddress, "Recipient address");
            ValidationUtility.ValidateGreaterThanZero(request.Amount, "Amount");
            ValidationUtility.ValidateNotNullOrEmpty(request.Password, "Password");

            // Validate Neo address format
            if (!request.ToAddress.IsValidNeoAddress())
            {
                throw new ArgumentException("Invalid Neo address format");
            }

            // Retrieve the wallet
            var wallet = await RetrieveWalletAsync(request.WalletId, request.AccountId);

            if (wallet == null)
            {
                throw new KeyNotFoundException("Wallet not found");
            }

            // Decrypt the private key
            var privateKey = EncryptionUtility.DecryptWithPassword(wallet.EncryptedPrivateKey, request.Password);

            // Create and sign the transaction
            var transactionHash = await CreateAndSignNeoTransactionAsync(wallet, privateKey, request.ToAddress, request.Amount, request.Network);

            // Create response
            var response = new
            {
                WalletId = wallet.Id,
                FromAddress = wallet.Address,
                ToAddress = request.ToAddress,
                Amount = request.Amount,
                Asset = "NEO",
                TransactionHash = transactionHash,
                Network = request.Network
            };

            // Log the transaction (without sensitive data)
            LoggingUtility.LogSecurityEvent(_logger, "NeoTransfer", Guid.NewGuid().ToString(),
                request.AccountId.ToString(), "Wallet", wallet.Id.ToString(), "Transfer", "Success",
                new Dictionary<string, object>
                {
                    ["FromAddress"] = wallet.Address,
                    ["ToAddress"] = request.ToAddress,
                    ["Amount"] = request.Amount,
                    ["Asset"] = "NEO",
                    ["TransactionHash"] = transactionHash,
                    ["Network"] = request.Network
                });

            return JsonUtility.SerializeToUtf8Bytes(response);
        }

        private async Task<string> CreateAndSignNeoTransactionAsync(Wallet wallet, string privateKey, string toAddress, decimal amount, string network)
        {
            // In a production environment, this would use the Neo SDK to create and sign a transaction
            // For now, we'll simulate transaction creation and signing

            // Validate the recipient address
            if (!IsValidNeoAddress(toAddress))
            {
                throw new ArgumentException("Invalid recipient address");
            }

            // Simulate network delay for transaction creation and signing
            await Task.Delay(100);

            // Generate a transaction hash
            var transactionHash = "0x" + Guid.NewGuid().ToString("N");

            return transactionHash;
        }

        private bool IsValidNeoAddress(string address)
        {
            // This method is now replaced by StringExtensions.IsValidNeoAddress
            // but we keep it for backward compatibility
            return address.IsValidNeoAddress();
        }

        private async Task<byte[]> TransferGasAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<TransferGasRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.WalletId, "Wallet ID");
            ValidationUtility.ValidateGuid(request.AccountId, "Account ID");
            ValidationUtility.ValidateNotNullOrEmpty(request.ToAddress, "Recipient address");
            ValidationUtility.ValidateGreaterThanZero(request.Amount, "Amount");
            ValidationUtility.ValidateNotNullOrEmpty(request.Password, "Password");

            // Validate Neo address format
            if (!request.ToAddress.IsValidNeoAddress())
            {
                throw new ArgumentException("Invalid Neo address format");
            }

            // Retrieve the wallet
            var wallet = await RetrieveWalletAsync(request.WalletId, request.AccountId);

            if (wallet == null)
            {
                throw new KeyNotFoundException("Wallet not found");
            }

            // Decrypt the private key
            var privateKey = EncryptionUtility.DecryptWithPassword(wallet.EncryptedPrivateKey, request.Password);

            // Create and sign the transaction
            var transactionHash = await CreateAndSignGasTransactionAsync(wallet, privateKey, request.ToAddress, request.Amount, request.Network);

            // Create response
            var response = new
            {
                WalletId = wallet.Id,
                FromAddress = wallet.Address,
                ToAddress = request.ToAddress,
                Amount = request.Amount,
                Asset = "GAS",
                TransactionHash = transactionHash,
                Network = request.Network
            };

            // Log the transaction (without sensitive data)
            LoggingUtility.LogSecurityEvent(_logger, "GasTransfer", Guid.NewGuid().ToString(),
                request.AccountId.ToString(), "Wallet", wallet.Id.ToString(), "Transfer", "Success",
                new Dictionary<string, object>
                {
                    ["FromAddress"] = wallet.Address,
                    ["ToAddress"] = request.ToAddress,
                    ["Amount"] = request.Amount,
                    ["Asset"] = "GAS",
                    ["TransactionHash"] = transactionHash,
                    ["Network"] = request.Network
                });

            return JsonUtility.SerializeToUtf8Bytes(response);
        }

        private async Task<string> CreateAndSignGasTransactionAsync(Wallet wallet, string privateKey, string toAddress, decimal amount, string network)
        {
            // In a production environment, this would use the Neo SDK to create and sign a transaction
            // For now, we'll simulate transaction creation and signing

            // Validate the recipient address
            if (!IsValidNeoAddress(toAddress))
            {
                throw new ArgumentException("Invalid recipient address");
            }

            // Simulate network delay for transaction creation and signing
            await Task.Delay(100);

            // Generate a transaction hash
            var transactionHash = "0x" + Guid.NewGuid().ToString("N");

            return transactionHash;
        }

        private async Task<byte[]> TransferTokenAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<TransferTokenRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.WalletId, "Wallet ID");
            ValidationUtility.ValidateGuid(request.AccountId, "Account ID");
            ValidationUtility.ValidateNotNullOrEmpty(request.ToAddress, "Recipient address");
            ValidationUtility.ValidateNotNullOrEmpty(request.TokenScriptHash, "Token script hash");
            ValidationUtility.ValidateGreaterThanZero(request.Amount, "Amount");
            ValidationUtility.ValidateNotNullOrEmpty(request.Password, "Password");

            // Validate Neo address format
            if (!request.ToAddress.IsValidNeoAddress())
            {
                throw new ArgumentException("Invalid Neo address format");
            }

            // Validate script hash format
            if (!request.TokenScriptHash.IsValidScriptHash())
            {
                throw new ArgumentException("Invalid token script hash format");
            }

            // Retrieve the wallet
            var wallet = await RetrieveWalletAsync(request.WalletId, request.AccountId);

            if (wallet == null)
            {
                throw new KeyNotFoundException("Wallet not found");
            }

            // Decrypt the private key
            var privateKey = EncryptionUtility.DecryptWithPassword(wallet.EncryptedPrivateKey, request.Password);

            // Create and sign the transaction
            var transactionHash = await CreateAndSignTokenTransactionAsync(wallet, privateKey, request.ToAddress, request.TokenScriptHash, request.Amount, request.Network);

            // Create response
            var response = new
            {
                WalletId = wallet.Id,
                FromAddress = wallet.Address,
                ToAddress = request.ToAddress,
                TokenScriptHash = request.TokenScriptHash,
                Amount = request.Amount,
                TransactionHash = transactionHash,
                Network = request.Network
            };

            // Log the transaction (without sensitive data)
            LoggingUtility.LogSecurityEvent(_logger, "TokenTransfer", Guid.NewGuid().ToString(),
                request.AccountId.ToString(), "Wallet", wallet.Id.ToString(), "Transfer", "Success",
                new Dictionary<string, object>
                {
                    ["FromAddress"] = wallet.Address,
                    ["ToAddress"] = request.ToAddress,
                    ["TokenScriptHash"] = request.TokenScriptHash,
                    ["Amount"] = request.Amount,
                    ["TransactionHash"] = transactionHash,
                    ["Network"] = request.Network
                });

            return JsonUtility.SerializeToUtf8Bytes(response);
        }

        private async Task<string> CreateAndSignTokenTransactionAsync(Wallet wallet, string privateKey, string toAddress, string tokenScriptHash, decimal amount, string network)
        {
            // In a production environment, this would use the Neo SDK to create and sign a transaction
            // For now, we'll simulate transaction creation and signing

            // Validate the recipient address
            if (!IsValidNeoAddress(toAddress))
            {
                throw new ArgumentException("Invalid recipient address");
            }

            // Validate the token script hash
            if (!IsValidScriptHash(tokenScriptHash))
            {
                throw new ArgumentException("Invalid token script hash");
            }

            // Simulate network delay for transaction creation and signing
            await Task.Delay(100);

            // Generate a transaction hash
            var transactionHash = "0x" + Guid.NewGuid().ToString("N");

            return transactionHash;
        }

        private bool IsValidScriptHash(string scriptHash)
        {
            // This method is now replaced by StringExtensions.IsValidScriptHash
            // but we keep it for backward compatibility
            return scriptHash.IsValidScriptHash();
        }

        /// <summary>
        /// Request model for creating a wallet
        /// </summary>
        private class CreateWalletRequest
        {
            /// <summary>
            /// Gets or sets the name of the wallet
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the password for encrypting the private key
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets the tags for the wallet
            /// </summary>
            public Dictionary<string, string> Tags { get; set; }
        }

        /// <summary>
        /// Request model for importing a wallet from WIF
        /// </summary>
        private class ImportWalletRequest
        {
            /// <summary>
            /// Gets or sets the name of the wallet
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the WIF (Wallet Import Format) string
            /// </summary>
            public string WIF { get; set; }

            /// <summary>
            /// Gets or sets the password for encrypting the private key
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets the tags for the wallet
            /// </summary>
            public Dictionary<string, string> Tags { get; set; }
        }

        /// <summary>
        /// Request model for signing data
        /// </summary>
        private class SignDataRequest
        {
            /// <summary>
            /// Gets or sets the wallet ID
            /// </summary>
            public Guid WalletId { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the data to sign
            /// </summary>
            public byte[] Data { get; set; }

            /// <summary>
            /// Gets or sets the password for decrypting the private key
            /// </summary>
            public string Password { get; set; }
        }

        /// <summary>
        /// Request model for transferring NEO
        /// </summary>
        private class TransferNeoRequest
        {
            /// <summary>
            /// Gets or sets the wallet ID
            /// </summary>
            public Guid WalletId { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the recipient address
            /// </summary>
            public string ToAddress { get; set; }

            /// <summary>
            /// Gets or sets the amount to transfer
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// Gets or sets the password for decrypting the private key
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets the network (MainNet or TestNet)
            /// </summary>
            public string Network { get; set; }
        }

        /// <summary>
        /// Request model for transferring GAS
        /// </summary>
        private class TransferGasRequest
        {
            /// <summary>
            /// Gets or sets the wallet ID
            /// </summary>
            public Guid WalletId { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the recipient address
            /// </summary>
            public string ToAddress { get; set; }

            /// <summary>
            /// Gets or sets the amount to transfer
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// Gets or sets the password for decrypting the private key
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets the network (MainNet or TestNet)
            /// </summary>
            public string Network { get; set; }
        }

        /// <summary>
        /// Request model for transferring tokens
        /// </summary>
        private class TransferTokenRequest
        {
            /// <summary>
            /// Gets or sets the wallet ID
            /// </summary>
            public Guid WalletId { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the recipient address
            /// </summary>
            public string ToAddress { get; set; }

            /// <summary>
            /// Gets or sets the token script hash
            /// </summary>
            public string TokenScriptHash { get; set; }

            /// <summary>
            /// Gets or sets the amount to transfer
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// Gets or sets the password for decrypting the private key
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets the network (MainNet or TestNet)
            /// </summary>
            public string Network { get; set; }
        }

        /// <summary>
        /// Wallet model
        /// </summary>
        private class Wallet
        {
            /// <summary>
            /// Gets or sets the wallet ID
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// Gets or sets the name of the wallet
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the Neo address
            /// </summary>
            public string Address { get; set; }

            /// <summary>
            /// Gets or sets the script hash
            /// </summary>
            public string ScriptHash { get; set; }

            /// <summary>
            /// Gets or sets the public key
            /// </summary>
            public string PublicKey { get; set; }

            /// <summary>
            /// Gets or sets the encrypted private key
            /// </summary>
            public string EncryptedPrivateKey { get; set; }

            /// <summary>
            /// Gets or sets the tags for the wallet
            /// </summary>
            public Dictionary<string, string> Tags { get; set; }

            /// <summary>
            /// Gets or sets the creation timestamp
            /// </summary>
            public DateTime CreatedAt { get; set; }

            /// <summary>
            /// Gets or sets the last update timestamp
            /// </summary>
            public DateTime UpdatedAt { get; set; }
        }
    }
}
