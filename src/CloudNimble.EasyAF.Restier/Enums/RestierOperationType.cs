namespace CloudNimble.EasyAF.Restier
{

    /// <summary>
    /// Specifies the type of operation being performed in Restier for logging and tracking purposes.
    /// Used by RestierHelpers to provide consistent operation logging across entity lifecycle events.
    /// </summary>
    public enum RestierOperationType
    {
        /// <summary>
        /// Indicates that entities have been filtered during query operations.
        /// </summary>
        Filtered = 1,

        /// <summary>
        /// Indicates that an entity is currently being inserted (in progress).
        /// </summary>
        Inserting = 2,

        /// <summary>
        /// Indicates that an entity has been successfully inserted (completed).
        /// </summary>
        Inserted = 3,

        /// <summary>
        /// Indicates that an entity is currently being updated (in progress).
        /// </summary>
        Updating = 4,

        /// <summary>
        /// Indicates that an entity has been successfully updated (completed).
        /// </summary>
        Updated = 5,

        /// <summary>
        /// Indicates that an entity is currently being deleted (in progress).
        /// </summary>
        Deleting = 6,

        /// <summary>
        /// Indicates that an entity has been successfully deleted (completed).
        /// </summary>
        Deleted = 7

    }

}
