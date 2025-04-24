namespace NeoServiceLayer.Core.Enums
{
    /// <summary>
    /// Represents the type of an event
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// Event from the Neo N3 blockchain
        /// </summary>
        Blockchain,

        /// <summary>
        /// Time-based event
        /// </summary>
        Time,

        /// <summary>
        /// Custom event
        /// </summary>
        Custom,

        /// <summary>
        /// HTTP request event
        /// </summary>
        HttpRequest,

        /// <summary>
        /// Price update event
        /// </summary>
        PriceUpdate
    }
}
