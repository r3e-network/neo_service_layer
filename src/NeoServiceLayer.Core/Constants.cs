namespace NeoServiceLayer.Core
{
    /// <summary>
    /// System-wide constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Enclave service types
        /// </summary>
        public static class EnclaveServiceTypes
        {
            /// <summary>
            /// Account service
            /// </summary>
            public const string Account = "account";

            /// <summary>
            /// Wallet service
            /// </summary>
            public const string Wallet = "wallet";

            /// <summary>
            /// Secrets service
            /// </summary>
            public const string Secrets = "secrets";

            /// <summary>
            /// Function service
            /// </summary>
            public const string Function = "function";

            /// <summary>
            /// Price feed service
            /// </summary>
            public const string PriceFeed = "pricefeed";
        }

        /// <summary>
        /// Account service operations
        /// </summary>
        public static class AccountOperations
        {
            /// <summary>
            /// Register a new account
            /// </summary>
            public const string Register = "register";

            /// <summary>
            /// Authenticate an account
            /// </summary>
            public const string Authenticate = "authenticate";

            /// <summary>
            /// Change account password
            /// </summary>
            public const string ChangePassword = "changePassword";

            /// <summary>
            /// Verify account
            /// </summary>
            public const string VerifyAccount = "verifyAccount";
        }

        /// <summary>
        /// Wallet service operations
        /// </summary>
        public static class WalletOperations
        {
            /// <summary>
            /// Create a new wallet
            /// </summary>
            public const string CreateWallet = "createWallet";

            /// <summary>
            /// Import a wallet from WIF
            /// </summary>
            public const string ImportFromWIF = "importFromWIF";

            /// <summary>
            /// Sign data with a wallet's private key
            /// </summary>
            public const string SignData = "signData";

            /// <summary>
            /// Transfer NEO tokens
            /// </summary>
            public const string TransferNeo = "transferNeo";

            /// <summary>
            /// Transfer GAS tokens
            /// </summary>
            public const string TransferGas = "transferGas";

            /// <summary>
            /// Transfer NEP-17 tokens
            /// </summary>
            public const string TransferToken = "transferToken";

            /// <summary>
            /// Get the current block height
            /// </summary>
            public const string GetBlockHeight = "getBlockHeight";

            /// <summary>
            /// Get a block by height
            /// </summary>
            public const string GetBlock = "getBlock";

            /// <summary>
            /// Get a transaction by hash
            /// </summary>
            public const string GetTransaction = "getTransaction";

            /// <summary>
            /// Get the balance of an address
            /// </summary>
            public const string GetBalance = "getBalance";

            /// <summary>
            /// Invoke a read-only contract method
            /// </summary>
            public const string InvokeRead = "invokeRead";

            /// <summary>
            /// Invoke a contract method that modifies state
            /// </summary>
            public const string InvokeWrite = "invokeWrite";
        }

        /// <summary>
        /// Secrets service operations
        /// </summary>
        public static class SecretsOperations
        {
            /// <summary>
            /// Create a new secret
            /// </summary>
            public const string CreateSecret = "createSecret";

            /// <summary>
            /// Get a secret by name
            /// </summary>
            public const string GetSecret = "getSecret";

            /// <summary>
            /// Get a secret by ID
            /// </summary>
            public const string GetSecretById = "getSecretById";

            /// <summary>
            /// Get the value of a secret
            /// </summary>
            public const string GetSecretValue = "getSecretValue";

            /// <summary>
            /// Update a secret
            /// </summary>
            public const string UpdateSecret = "updateSecret";

            /// <summary>
            /// Update the value of a secret
            /// </summary>
            public const string UpdateSecretValue = "updateSecretValue";

            /// <summary>
            /// Update the value of a secret
            /// </summary>
            public const string UpdateValue = "updateValue";

            /// <summary>
            /// Delete a secret
            /// </summary>
            public const string DeleteSecret = "deleteSecret";

            /// <summary>
            /// Rotate a secret
            /// </summary>
            public const string RotateSecret = "rotateSecret";

            /// <summary>
            /// Check if a function has access to a secret
            /// </summary>
            public const string HasAccess = "hasAccess";
        }

        /// <summary>
        /// Function service operations
        /// </summary>
        public static class FunctionOperations
        {
            /// <summary>
            /// Create a function
            /// </summary>
            public const string CreateFunction = "createFunction";

            /// <summary>
            /// Execute a function
            /// </summary>
            public const string Execute = "execute";

            /// <summary>
            /// Execute a function in response to an event
            /// </summary>
            public const string ExecuteForEvent = "executeForEvent";

            /// <summary>
            /// Update a function
            /// </summary>
            public const string UpdateFunction = "updateFunction";

            /// <summary>
            /// Update function source code
            /// </summary>
            public const string UpdateSourceCode = "updateSourceCode";

            /// <summary>
            /// Update function environment variables
            /// </summary>
            public const string UpdateEnvironmentVariables = "updateEnvironmentVariables";

            /// <summary>
            /// Update function secret access
            /// </summary>
            public const string UpdateSecretAccess = "updateSecretAccess";

            /// <summary>
            /// Execute a function
            /// </summary>
            public const string ExecuteFunction = "executeFunction";

            /// <summary>
            /// Execute a function for an event
            /// </summary>
            public const string ExecuteFunctionForEvent = "executeFunctionForEvent";

            /// <summary>
            /// Activate a function
            /// </summary>
            public const string ActivateFunction = "activateFunction";

            /// <summary>
            /// Deactivate a function
            /// </summary>
            public const string DeactivateFunction = "deactivateFunction";

            /// <summary>
            /// Delete a function
            /// </summary>
            public const string DeleteFunction = "deleteFunction";

            /// <summary>
            /// Get a storage value
            /// </summary>
            public const string GetStorageValue = "getStorageValue";

            /// <summary>
            /// Set a storage value
            /// </summary>
            public const string SetStorageValue = "setStorageValue";

            /// <summary>
            /// Delete a storage value
            /// </summary>
            public const string DeleteStorageValue = "deleteStorageValue";

            /// <summary>
            /// Register a blockchain event
            /// </summary>
            public const string RegisterBlockchainEvent = "registerBlockchainEvent";

            /// <summary>
            /// Register a time event
            /// </summary>
            public const string RegisterTimeEvent = "registerTimeEvent";

            /// <summary>
            /// Trigger a custom event
            /// </summary>
            public const string TriggerCustomEvent = "triggerCustomEvent";
        }

        /// <summary>
        /// Price feed service operations
        /// </summary>
        public static class PriceFeedOperations
        {
            /// <summary>
            /// Fetch prices from all sources
            /// </summary>
            public const string FetchPrices = "fetchPrices";

            /// <summary>
            /// Fetch price for a specific symbol
            /// </summary>
            public const string FetchPriceForSymbol = "fetchPriceForSymbol";

            /// <summary>
            /// Fetch price from a specific source
            /// </summary>
            public const string FetchPriceFromSource = "fetchPriceFromSource";

            /// <summary>
            /// Generate price history from individual prices
            /// </summary>
            public const string GeneratePriceHistory = "generatePriceHistory";

            /// <summary>
            /// Validate a price source configuration
            /// </summary>
            public const string ValidateSource = "validateSource";

            /// <summary>
            /// Submit price data to the Neo N3 oracle contract
            /// </summary>
            public const string SubmitToOracle = "submitToOracle";

            /// <summary>
            /// Submit multiple price data to the Neo N3 oracle contract
            /// </summary>
            public const string SubmitBatchToOracle = "submitBatchToOracle";
        }

        /// <summary>
        /// VSOCK configuration
        /// </summary>
        public static class VsockConfig
        {
            /// <summary>
            /// Parent CID
            /// </summary>
            public const int ParentCid = 3;

            /// <summary>
            /// Enclave CID
            /// </summary>
            public const int EnclaveCid = 16;

            /// <summary>
            /// Enclave port
            /// </summary>
            public const int EnclavePort = 5000;
        }

        /// <summary>
        /// JWT configuration
        /// </summary>
        public static class JwtConfig
        {
            /// <summary>
            /// JWT token expiration in minutes
            /// </summary>
            public const int TokenExpirationMinutes = 60;

            /// <summary>
            /// JWT issuer
            /// </summary>
            public const string Issuer = "NeoServiceLayer";

            /// <summary>
            /// JWT audience
            /// </summary>
            public const string Audience = "NeoServiceLayerApi";
        }

        /// <summary>
        /// Neo N3 configuration
        /// </summary>
        public static class NeoConfig
        {
            /// <summary>
            /// Neo N3 RPC URL
            /// </summary>
            public const string RpcUrl = "http://seed1.neo.org:10332";

            /// <summary>
            /// Neo N3 network magic number
            /// </summary>
            public const uint NetworkMagic = 860833102; // MainNet

            /// <summary>
            /// Neo N3 GAS token hash
            /// </summary>
            public const string GasTokenHash = "0xd2a4cff31913016155e38e474a2c06d08be276cf";

            /// <summary>
            /// Neo N3 NEO token hash
            /// </summary>
            public const string NeoTokenHash = "0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5";

            /// <summary>
            /// Neo N3 oracle contract hash
            /// </summary>
            public const string OracleContractHash = "0xfe924b7cfe89ddd271abaf7210a80a7e11178758";
        }
    }
}
