using System.Collections.Generic;

namespace CloudNimble.EasyAF.EFCoreToEdmx.Models
{

    /// <summary>
    /// Represents a complete EDMX model containing all entities, relationships, and metadata
    /// extracted from an Entity Framework Core model.
    /// </summary>
    /// <remarks>
    /// This is the root model class that contains all components of an EDMX file including
    /// entity types, associations, entity sets, and association sets. It serves as the
    /// intermediate representation between EF Core metadata and EDMX XML generation.
    /// </remarks>
    public class EdmxModel
    {

        /// <summary>
        /// Gets or sets the namespace for the conceptual model.
        /// </summary>
        /// <value>
        /// The namespace string used in the EDMX conceptual model schema.
        /// Defaults to an empty string if not specified.
        /// </value>
        /// <remarks>
        /// This namespace is used throughout the EDMX file to qualify entity types
        /// and other schema elements. It should be a valid .NET namespace format.
        /// </remarks>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the entity container.
        /// </summary>
        /// <value>
        /// The entity container name used in the EDMX model.
        /// Defaults to an empty string if not specified.
        /// </value>
        /// <remarks>
        /// The entity container groups all entity sets and association sets in the model.
        /// This name is referenced throughout the EDMX file and should be unique within the namespace.
        /// </remarks>
        public string ContainerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of entity types in the model.
        /// </summary>
        /// <value>
        /// A list of <see cref="EdmxEntityType"/> objects representing all entities in the model.
        /// Initialized to an empty list by default.
        /// </value>
        /// <remarks>
        /// Each entity type corresponds to a table or view in the database and contains
        /// all properties, keys, and navigation properties for that entity.
        /// </remarks>
        public List<EdmxEntityType> EntityTypes { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of associations (relationships) in the model.
        /// </summary>
        /// <value>
        /// A list of <see cref="EdmxAssociation"/> objects representing all relationships between entities.
        /// Initialized to an empty list by default.
        /// </value>
        /// <remarks>
        /// Associations define the relationships between entity types, including foreign key
        /// constraints, multiplicity, and referential integrity rules.
        /// </remarks>
        public List<EdmxAssociation> Associations { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of entity sets in the model.
        /// </summary>
        /// <value>
        /// A list of <see cref="EdmxEntitySet"/> objects representing all entity sets in the container.
        /// Initialized to an empty list by default.
        /// </value>
        /// <remarks>
        /// Entity sets are collections of entity instances and correspond to the actual
        /// tables or queryable collections in the data source.
        /// </remarks>
        public List<EdmxEntitySet> EntitySets { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of association sets in the model.
        /// </summary>
        /// <value>
        /// A list of <see cref="EdmxAssociationSet"/> objects representing all relationship instances in the container.
        /// Initialized to an empty list by default.
        /// </value>
        /// <remarks>
        /// Association sets connect associations to specific entity sets, defining which
        /// entity set instances participate in each relationship.
        /// </remarks>
        public List<EdmxAssociationSet> AssociationSets { get; set; } = [];

        /// <summary>
        /// Gets or sets the complete OnModelCreating method from scaffolded contexts.
        /// </summary>
        /// <value>
        /// The complete C# OnModelCreating method including signature and braces as a string.
        /// Defaults to an empty string if not available.
        /// </value>
        /// <remarks>
        /// This property stores the complete OnModelCreating method extracted during database scaffolding.
        /// The method includes the signature, opening brace, all method body statements, and closing brace.
        /// The complete method can be used for regenerating the DbContext or for documentation purposes.
        /// Stored in the EasyAF custom namespace within the EDMX Designer section.
        /// </remarks>
        public string OnModelCreatingBody { get; set; } = string.Empty;

    }

}
