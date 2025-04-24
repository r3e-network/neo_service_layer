namespace NeoServiceLayer.Core.Enums
{
    /// <summary>
    /// Represents the type of price source
    /// </summary>
    public enum PriceSourceType
    {
        /// <summary>
        /// Unknown source type
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// API source (e.g., exchange API)
        /// </summary>
        API = 1,

        /// <summary>
        /// Aggregated source (e.g., median of multiple sources)
        /// </summary>
        Aggregated = 2,

        /// <summary>
        /// Oracle source (e.g., Chainlink)
        /// </summary>
        Oracle = 3,

        /// <summary>
        /// Smart contract source
        /// </summary>
        SmartContract = 4,

        /// <summary>
        /// Manual source (e.g., manually entered price)
        /// </summary>
        Manual = 5
    }
}
