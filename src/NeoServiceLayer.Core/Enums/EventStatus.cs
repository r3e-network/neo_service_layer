namespace NeoServiceLayer.Core.Enums
{
    /// <summary>
    /// Represents the status of an event
    /// </summary>
    public enum EventStatus
    {
        /// <summary>
        /// Event is pending processing
        /// </summary>
        Pending,

        /// <summary>
        /// Event is being processed
        /// </summary>
        Processing,

        /// <summary>
        /// Event has been processed successfully
        /// </summary>
        Completed,

        /// <summary>
        /// Event processing has failed
        /// </summary>
        Failed,

        /// <summary>
        /// Event has been retried
        /// </summary>
        Retried
    }
}
