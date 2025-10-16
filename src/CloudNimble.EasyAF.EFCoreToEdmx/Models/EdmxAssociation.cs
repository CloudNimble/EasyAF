namespace CloudNimble.EasyAF.EFCoreToEdmx.Models
{

    /// <summary>
    /// Represents an association (relationship) between two entity types in an EDMX model.
    /// </summary>
    public class EdmxAssociation
    {

        /// <summary>
        /// Gets or sets the conceptual model name of the association.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the storage model name with FK_ prefix.
        /// </summary>
        public string StorageName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first end of the association.
        /// </summary>
        public EdmxAssociationEnd End1 { get; set; } = new();

        /// <summary>
        /// Gets or sets the second end of the association.
        /// </summary>
        public EdmxAssociationEnd End2 { get; set; } = new();

        /// <summary>
        /// Gets or sets the referential constraint for the association.
        /// </summary>
        public EdmxReferentialConstraint ReferentialConstraint { get; set; }

    }

}
