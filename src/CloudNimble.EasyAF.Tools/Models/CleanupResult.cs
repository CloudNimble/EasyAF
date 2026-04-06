namespace CloudNimble.EasyAF.Tools.Models
{
    /// <summary>
    /// Represents the result of a cleanup operation.
    /// </summary>
    public class CleanupResult
    {
        /// <summary>
        /// Gets or sets whether the cleanup operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the result message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets any error message if the operation failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the number of orphaned files found.
        /// </summary>
        public int OrphanedFilesFound { get; set; }

        /// <summary>
        /// Gets or sets the number of files deleted.
        /// </summary>
        public int FilesDeleted { get; set; }

        /// <summary>
        /// Gets or sets the number of errors encountered during deletion.
        /// </summary>
        public int ErrorCount { get; set; }
    }

}
