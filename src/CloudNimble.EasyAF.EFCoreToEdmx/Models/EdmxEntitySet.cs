namespace CloudNimble.EasyAF.EFCoreToEdmx.Models
{
    /// <summary>
    /// Represents an entity set in an EDMX model, defining a collection of entity instances.
    /// </summary>
    /// <remarks>
    /// Entity sets correspond to tables or queryable collections in the data source and
    /// define the scope for entity instances of a particular type.
    /// </remarks>
    public class EdmxEntitySet
    {

        /// <summary>
        /// Gets or sets the name of the entity set.
        /// </summary>
        /// <value>
        /// A unique name for the entity set within the entity container.
        /// Defaults to an empty string if not specified.
        /// </value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the entity type for this set.
        /// </summary>
        /// <value>
        /// The entity type name that defines the structure of entities in this set.
        /// Defaults to an empty string if not specified.
        /// </value>
        public string EntityTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schema name associated with the current database context.
        /// </summary>
        public string Schema { get; set; } = "dbo";

    }

}

