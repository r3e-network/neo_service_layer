namespace NeoServiceLayer.Core.Enums
{
    /// <summary>
    /// Represents the status of a function
    /// </summary>
    public enum FunctionStatus
    {
        /// <summary>
        /// Function is active and ready for execution
        /// </summary>
        Active,

        /// <summary>
        /// Function is inactive and cannot be executed
        /// </summary>
        Inactive,

        /// <summary>
        /// Function has an error and cannot be executed
        /// </summary>
        Error,

        /// <summary>
        /// Function is being deployed
        /// </summary>
        Deploying,

        /// <summary>
        /// Function is being updated
        /// </summary>
        Updating
    }
}
