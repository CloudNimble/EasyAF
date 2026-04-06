using System.Collections.Generic;

namespace CloudNimble.EasyAF.EFCoreToEdmx.Models
{

    /// <summary>
    /// Represents an entity type in an EDMX model, corresponding to a table or view in the database.
    /// </summary>
    public class EdmxEntityType
    {

        /// <summary>
        /// Gets or sets the conceptual model name (singular, e.g., "User").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the storage model name (plural, e.g., "Users").
        /// </summary>
        public string StorageName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of scalar properties for this entity type.
        /// </summary>
        public List<EdmxProperty> Properties { get; set; } = new();

        /// <summary>
        /// Gets or sets the collection of navigation properties for this entity type.
        /// </summary>
        public List<EdmxNavigationProperty> NavigationProperties { get; set; } = new();

        /// <summary>
        /// Gets or sets the collection of property names that form the primary key.
        /// </summary>
        public List<string> Keys { get; set; } = new();

        /// <summary>
        /// Gets or sets the documentation comment for the entity type.
        /// </summary>
        public string Documentation { get; set; } = string.Empty;

    }

}
