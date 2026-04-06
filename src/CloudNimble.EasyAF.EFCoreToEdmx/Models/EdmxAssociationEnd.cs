namespace CloudNimble.EasyAF.EFCoreToEdmx.Models
{

    /// <summary>
    /// Represents one end of an association in an EDMX model.
    /// </summary>
    public class EdmxAssociationEnd
    {

        /// <summary>
        /// Gets or sets the conceptual model role name.
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the storage model role name (pluralized).
        /// </summary>
        public string StorageRole { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity type for this end of the association.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the multiplicity for this end of the association.
        /// </summary>
        public string Multiplicity { get; set; } = string.Empty;

    }

}
