namespace NeoServiceLayer.Tests
{
    /// <summary>
    /// Constants used in tests
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Price feed operations
        /// </summary>
        public static class PriceFeedOperations
        {
            /// <summary>
            /// Fetch prices operation
            /// </summary>
            public const string FetchPrices = "FetchPrices";

            /// <summary>
            /// Fetch price for symbol operation
            /// </summary>
            public const string FetchPriceForSymbol = "FetchPriceForSymbol";

            /// <summary>
            /// Fetch price from source operation
            /// </summary>
            public const string FetchPriceFromSource = "FetchPriceFromSource";

            /// <summary>
            /// Generate price history operation
            /// </summary>
            public const string GeneratePriceHistory = "GeneratePriceHistory";

            /// <summary>
            /// Validate source operation
            /// </summary>
            public const string ValidateSource = "ValidateSource";

            /// <summary>
            /// Submit to oracle operation
            /// </summary>
            public const string SubmitToOracle = "SubmitToOracle";

            /// <summary>
            /// Submit batch to oracle operation
            /// </summary>
            public const string SubmitBatchToOracle = "SubmitBatchToOracle";
        }

        /// <summary>
        /// Account operations
        /// </summary>
        public static class AccountOperations
        {
            /// <summary>
            /// Register operation
            /// </summary>
            public const string Register = "Register";

            /// <summary>
            /// Authenticate operation
            /// </summary>
            public const string Authenticate = "Authenticate";

            /// <summary>
            /// Change password operation
            /// </summary>
            public const string ChangePassword = "ChangePassword";

            /// <summary>
            /// Verify account operation
            /// </summary>
            public const string VerifyAccount = "VerifyAccount";
        }

        /// <summary>
        /// Wallet operations
        /// </summary>
        public static class WalletOperations
        {
            /// <summary>
            /// Create wallet operation
            /// </summary>
            public const string CreateWallet = "CreateWallet";

            /// <summary>
            /// Import from WIF operation
            /// </summary>
            public const string ImportFromWIF = "ImportFromWIF";

            /// <summary>
            /// Sign data operation
            /// </summary>
            public const string SignData = "SignData";

            /// <summary>
            /// Transfer NEO operation
            /// </summary>
            public const string TransferNeo = "TransferNeo";

            /// <summary>
            /// Transfer GAS operation
            /// </summary>
            public const string TransferGas = "TransferGas";

            /// <summary>
            /// Transfer token operation
            /// </summary>
            public const string TransferToken = "TransferToken";
        }

        /// <summary>
        /// Function operations
        /// </summary>
        public static class FunctionOperations
        {
            /// <summary>
            /// Execute operation
            /// </summary>
            public const string Execute = "Execute";

            /// <summary>
            /// Execute for event operation
            /// </summary>
            public const string ExecuteForEvent = "ExecuteForEvent";
        }

        /// <summary>
        /// Secrets operations
        /// </summary>
        public static class SecretsOperations
        {
            /// <summary>
            /// Create secret operation
            /// </summary>
            public const string CreateSecret = "CreateSecret";

            /// <summary>
            /// Get secret value operation
            /// </summary>
            public const string GetSecretValue = "GetSecretValue";

            /// <summary>
            /// Update value operation
            /// </summary>
            public const string UpdateValue = "UpdateValue";

            /// <summary>
            /// Rotate secret operation
            /// </summary>
            public const string RotateSecret = "RotateSecret";

            /// <summary>
            /// Has access operation
            /// </summary>
            public const string HasAccess = "HasAccess";
        }
    }
}
