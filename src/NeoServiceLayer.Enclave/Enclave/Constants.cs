namespace NeoServiceLayer.Enclave.Enclave
{
    /// <summary>
    /// Constants for the enclave
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// VSOCK configuration
        /// </summary>
        public static class VsockConfig
        {
            /// <summary>
            /// The port for the enclave
            /// </summary>
            public const uint EnclavePort = 5000;
        }

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
        /// Account operations
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
            /// Verify account email
            /// </summary>
            public const string VerifyEmail = "verifyEmail";
        }

        /// <summary>
        /// Wallet operations
        /// </summary>
        public static class WalletOperations
        {
            /// <summary>
            /// Create a new wallet
            /// </summary>
            public const string CreateWallet = "createWallet";

            /// <summary>
            /// Import a wallet
            /// </summary>
            public const string ImportWallet = "importWallet";

            /// <summary>
            /// Sign a transaction
            /// </summary>
            public const string SignTransaction = "signTransaction";

            /// <summary>
            /// Sign data
            /// </summary>
            public const string SignData = "signData";

            /// <summary>
            /// Transfer NEO
            /// </summary>
            public const string TransferNeo = "transferNeo";

            /// <summary>
            /// Transfer GAS
            /// </summary>
            public const string TransferGas = "transferGas";

            /// <summary>
            /// Transfer NEP-17 token
            /// </summary>
            public const string TransferToken = "transferToken";
        }

        /// <summary>
        /// Function operations
        /// </summary>
        public static class FunctionOperations
        {
            /// <summary>
            /// Create a new function
            /// </summary>
            public const string CreateFunction = "createFunction";

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
        }

        /// <summary>
        /// Secrets operations
        /// </summary>
        public static class SecretsOperations
        {
            /// <summary>
            /// Create a new secret
            /// </summary>
            public const string CreateSecret = "createSecret";

            /// <summary>
            /// Get a secret
            /// </summary>
            public const string GetSecret = "getSecret";

            /// <summary>
            /// Update a secret
            /// </summary>
            public const string UpdateSecret = "updateSecret";

            /// <summary>
            /// Delete a secret
            /// </summary>
            public const string DeleteSecret = "deleteSecret";

            /// <summary>
            /// Update secret value
            /// </summary>
            public const string UpdateSecretValue = "updateSecretValue";
        }

        /// <summary>
        /// Price feed operations
        /// </summary>
        public static class PriceFeedOperations
        {
            /// <summary>
            /// Add a new price source
            /// </summary>
            public const string AddPriceSource = "addPriceSource";

            /// <summary>
            /// Update a price source
            /// </summary>
            public const string UpdatePriceSource = "updatePriceSource";

            /// <summary>
            /// Delete a price source
            /// </summary>
            public const string DeletePriceSource = "deletePriceSource";

            /// <summary>
            /// Get price for a symbol
            /// </summary>
            public const string GetPrice = "getPrice";

            /// <summary>
            /// Get historical prices for a symbol
            /// </summary>
            public const string GetHistoricalPrices = "getHistoricalPrices";

            /// <summary>
            /// Submit price to oracle contract
            /// </summary>
            public const string SubmitPriceToOracle = "submitPriceToOracle";

            /// <summary>
            /// Get price feed configuration
            /// </summary>
            public const string GetConfiguration = "getConfiguration";
        }
    }
}
