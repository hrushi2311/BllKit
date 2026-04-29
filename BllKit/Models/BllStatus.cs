namespace BllKit.Models
{
    /// <summary>
    /// Represents the outcome status of a BLL operation.
    /// </summary>
    public enum BllStatus
    {
        /// <summary>
        /// Operation completed successfully.
        /// </summary>
        Success = 1,

        /// <summary>
        /// Operation failed due to a known business rule (e.g. entity not found, access denied).
        /// Not an unexpected error — the caller knows why it failed.
        /// </summary>
        Fail = 2,

        /// <summary>
        /// Operation failed due to an unexpected exception.
        /// </summary>
        Error = 3
    }
}
