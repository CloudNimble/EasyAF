namespace CloudNimble.EasyAF.EFCoreToEdmx.Models
{

    /// <summary>
    /// Represents a navigation property in an EDMX entity type, defining relationships to other entities.
    /// </summary>
    /// <remarks>
    /// Navigation properties enable traversal between related entities in the conceptual model.
    /// They correspond to foreign key relationships in the database and provide strongly-typed
    /// access to related entity instances.
    /// </remarks>
    public class EdmxNavigationProperty
    {

        /// <summary>
        /// Gets or sets the name of the navigation property.
        /// </summary>
        /// <value>
        /// The navigation property name, typically matching the CLR property name.
        /// Defaults to an empty string if not specified.
        /// </value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the association that defines this navigation.
        /// </summary>
        /// <value>
        /// The association name that this navigation property participates in.
        /// Defaults to an empty string if not specified.
        /// </value>
        public string Relationship { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role name for the source end of the navigation.
        /// </summary>
        /// <value>
        /// The role name representing the entity type that contains this navigation property.
        /// Defaults to an empty string if not specified.
        /// </value>
        public string FromRole { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role name for the target end of the navigation.
        /// </summary>
        /// <value>
        /// The role name representing the entity type that this navigation property references.
        /// Defaults to an empty string if not specified.
        /// </value>
        public string ToRole { get; set; } = string.Empty;

    }

}
