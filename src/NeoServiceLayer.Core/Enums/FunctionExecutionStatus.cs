namespace NeoServiceLayer.Core.Enums
{
    /// <summary>
    /// Status of a function execution
    /// </summary>
    public enum FunctionExecutionStatus
    {
        /// <summary>
        /// The function execution is pending
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The function execution is in progress
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// The function execution completed successfully
        /// </summary>
        Completed = 2,

        /// <summary>
        /// The function execution failed
        /// </summary>
        Failed = 3,

        /// <summary>
        /// The function execution timed out
        /// </summary>
        TimedOut = 4,

        /// <summary>
        /// The function execution was cancelled
        /// </summary>
        Cancelled = 5
    }
}
