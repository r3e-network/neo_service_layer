using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function
{
    /// <summary>
    /// Initializes function templates
    /// </summary>
    public class FunctionTemplateInitializer
    {
        private readonly ILogger<FunctionTemplateInitializer> _logger;
        private readonly IFunctionTemplateRepository _templateRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionTemplateInitializer"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="templateRepository">Template repository</param>
        public FunctionTemplateInitializer(ILogger<FunctionTemplateInitializer> logger, IFunctionTemplateRepository templateRepository)
        {
            _logger = logger;
            _templateRepository = templateRepository;
        }

        /// <summary>
        /// Initializes the templates
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing function templates");

            try
            {
                // Check if templates already exist
                var existingTemplates = await _templateRepository.GetAllAsync();
                if (existingTemplates.GetEnumerator().MoveNext())
                {
                    _logger.LogInformation("Templates already initialized");
                    return;
                }

                // Create templates
                await CreatePriceFeedOracleTemplateAsync();
                await CreateBlockchainEventHandlerTemplateAsync();
                await CreateContractInteractionTemplateAsync();
                await CreateSecretsManagerTemplateAsync();

                _logger.LogInformation("Function templates initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing function templates");
                throw;
            }
        }

        private async Task CreatePriceFeedOracleTemplateAsync()
        {
            _logger.LogInformation("Creating price feed oracle template");

            var sourceCode = await ReadTemplateFileAsync("price-feed-oracle.js");
            var template = new FunctionTemplate
            {
                Name = "Price Feed Oracle",
                Description = "Fetches price data from multiple sources, processes it, and submits it to the Neo N3 blockchain oracle.",
                Runtime = "javascript",
                Category = "Blockchain",
                Tags = new List<string> { "price-feed", "oracle", "blockchain" },
                SourceCode = sourceCode,
                Handler = "processPrices",
                EntryPoint = "processPrices",
                DefaultEnvironmentVariables = new Dictionary<string, string>
                {
                    { "DEFAULT_BASE_CURRENCY", "USD" },
                    { "DEFAULT_SYMBOLS", "NEO,GAS,BTC,ETH" }
                },
                RequiredSecrets = new List<string> { "oracle_api_key" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Author = "Neo Service Layer",
                Version = "1.0.0",
                DocumentationUrl = "https://docs.neo.org/service-layer/functions/templates/price-feed-oracle"
            };

            await _templateRepository.CreateAsync(template);
        }

        private async Task CreateBlockchainEventHandlerTemplateAsync()
        {
            _logger.LogInformation("Creating blockchain event handler template");

            var sourceCode = await ReadTemplateFileAsync("blockchain-event-handler.js");
            var template = new FunctionTemplate
            {
                Name = "Blockchain Event Handler",
                Description = "Handles blockchain events and performs actions based on the event data.",
                Runtime = "javascript",
                Category = "Blockchain",
                Tags = new List<string> { "events", "blockchain", "handler" },
                SourceCode = sourceCode,
                Handler = "handleEvent",
                EntryPoint = "handleEvent",
                DefaultEnvironmentVariables = new Dictionary<string, string>
                {
                    { "EVENT_LOGGING_ENABLED", "true" }
                },
                RequiredSecrets = new List<string> { "notification_api_key" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Author = "Neo Service Layer",
                Version = "1.0.0",
                DocumentationUrl = "https://docs.neo.org/service-layer/functions/templates/blockchain-event-handler"
            };

            await _templateRepository.CreateAsync(template);
        }

        private async Task CreateContractInteractionTemplateAsync()
        {
            _logger.LogInformation("Creating contract interaction template");

            var sourceCode = @"/**
 * Neo Contract Interaction Function
 * 
 * This function demonstrates how to interact with Neo N3 smart contracts.
 */

// Entry point for the function
async function interactWithContract(params) {
    console.log(""Starting contract interaction function"");
    
    try {
        // Get contract parameters from input or use defaults
        const scriptHash = params.scriptHash || ""0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5""; // Example contract
        const operation = params.operation || ""balanceOf"";
        const args = params.args || [];
        
        console.log(`Interacting with contract: ${scriptHash}, operation: ${operation}`);
        
        // Perform a read-only invocation
        const result = await neoService.blockchain.invokeRead(scriptHash, operation, args);
        console.log(`Contract invocation result: ${JSON.stringify(result)}`);
        
        // If this is a write operation and params.execute is true, execute the transaction
        if (params.execute && params.isWriteOperation) {
            const txHash = await neoService.blockchain.invokeWrite(scriptHash, operation, args);
            console.log(`Transaction executed, hash: ${txHash}`);
            
            // Store the transaction hash
            await neoService.storage.set(`tx_${new Date().toISOString()}`, txHash);
            
            return {
                success: true,
                readResult: result,
                writeResult: {
                    txHash: txHash
                }
            };
        }
        
        // Store the invocation result
        await neoService.storage.set(`invoke_${new Date().toISOString()}`, JSON.stringify(result));
        
        return {
            success: true,
            result: result
        };
    } catch (error) {
        console.error(`Error in contract interaction function: ${error.message}`);
        return {
            success: false,
            error: error.message
        };
    }
}";

            var template = new FunctionTemplate
            {
                Name = "Contract Interaction",
                Description = "Interacts with Neo N3 smart contracts.",
                Runtime = "javascript",
                Category = "Blockchain",
                Tags = new List<string> { "contract", "blockchain", "interaction" },
                SourceCode = sourceCode,
                Handler = "interactWithContract",
                EntryPoint = "interactWithContract",
                DefaultEnvironmentVariables = new Dictionary<string, string>
                {
                    { "DEFAULT_NETWORK", "MainNet" }
                },
                RequiredSecrets = new List<string> { "wallet_password" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Author = "Neo Service Layer",
                Version = "1.0.0",
                DocumentationUrl = "https://docs.neo.org/service-layer/functions/templates/contract-interaction"
            };

            await _templateRepository.CreateAsync(template);
        }

        private async Task CreateSecretsManagerTemplateAsync()
        {
            _logger.LogInformation("Creating secrets manager template");

            var sourceCode = @"/**
 * Neo Secrets Manager Function
 * 
 * This function demonstrates how to use the Secrets service to securely
 * access and manage sensitive information.
 */

// Entry point for the function
async function manageSecrets(params) {
    console.log(""Starting secrets manager function"");
    
    try {
        // Get operation from parameters
        const operation = params.operation || ""get"";
        
        switch (operation) {
            case ""get"":
                return await getSecret(params);
            case ""list"":
                return await listSecrets(params);
            case ""use"":
                return await useSecretForApi(params);
            default:
                throw new Error(`Unknown operation: ${operation}`);
        }
    } catch (error) {
        console.error(`Error in secrets manager function: ${error.message}`);
        return {
            success: false,
            error: error.message
        };
    }
}

// Get a secret by name
async function getSecret(params) {
    const secretName = params.name;
    if (!secretName) {
        throw new Error(""Secret name is required"");
    }
    
    console.log(`Getting secret: ${secretName}`);
    
    const secretValue = await neoService.secrets.getSecret(secretName);
    
    // Never log the actual secret value in production!
    console.log(`Retrieved secret: ${secretName}`);
    
    return {
        success: true,
        name: secretName,
        // Return only metadata, not the actual value
        hasValue: !!secretValue
    };
}

// List available secrets (names only)
async function listSecrets(params) {
    console.log(""Listing available secrets"");
    
    // In a real implementation, you would get this from a repository
    // For this example, we'll just return a list of secret names that we know exist
    const secretNames = [
        ""api_key"",
        ""database_password"",
        ""jwt_secret"",
        ""notification_api_key""
    ];
    
    return {
        success: true,
        secrets: secretNames.map(name => ({ name }))
    };
}

// Use a secret to make an API call
async function useSecretForApi(params) {
    const apiName = params.api || ""example"";
    const secretName = `${apiName}_api_key`;
    
    console.log(`Using secret for API call: ${apiName}`);
    
    // Get the API key from secrets
    const apiKey = await neoService.secrets.getSecret(secretName);
    if (!apiKey) {
        throw new Error(`API key not found for: ${apiName}`);
    }
    
    // In a real implementation, you would use the API key to make an API call
    // For this example, we'll just simulate a successful API call
    console.log(`Making API call to ${apiName} with API key: ${apiKey.substring(0, 3)}...`);
    
    // Store the API call timestamp
    await neoService.storage.set(`api_call_${apiName}`, new Date().toISOString());
    
    return {
        success: true,
        api: apiName,
        message: ""API call successful""
    };
}";

            var template = new FunctionTemplate
            {
                Name = "Secrets Manager",
                Description = "Demonstrates how to use the Secrets service to securely access and manage sensitive information.",
                Runtime = "javascript",
                Category = "Security",
                Tags = new List<string> { "secrets", "security", "api" },
                SourceCode = sourceCode,
                Handler = "manageSecrets",
                EntryPoint = "manageSecrets",
                DefaultEnvironmentVariables = new Dictionary<string, string>
                {
                    { "DEFAULT_OPERATION", "get" }
                },
                RequiredSecrets = new List<string> { "api_key", "notification_api_key" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Author = "Neo Service Layer",
                Version = "1.0.0",
                DocumentationUrl = "https://docs.neo.org/service-layer/functions/templates/secrets-manager"
            };

            await _templateRepository.CreateAsync(template);
        }

        private async Task<string> ReadTemplateFileAsync(string fileName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"NeoServiceLayer.Services.Function.Templates.{fileName}";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        // Try to read from file system
                        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", fileName);
                        if (File.Exists(path))
                        {
                            return await File.ReadAllTextAsync(path);
                        }

                        // Try to read from current directory
                        path = Path.Combine("Templates", fileName);
                        if (File.Exists(path))
                        {
                            return await File.ReadAllTextAsync(path);
                        }

                        // Try to read from source directory
                        path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Function", "Templates", fileName);
                        if (File.Exists(path))
                        {
                            return await File.ReadAllTextAsync(path);
                        }

                        _logger.LogWarning("Template file not found: {FileName}", fileName);
                        return string.Empty;
                    }

                    using (var reader = new StreamReader(stream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading template file: {FileName}", fileName);
                throw;
            }
        }
    }
}
