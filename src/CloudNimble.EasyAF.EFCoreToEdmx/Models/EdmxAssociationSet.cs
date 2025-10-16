// EdmxAssociationSet.cs
// EdmxAssociationSet.cs
namespace CloudNimble.EasyAF.EFCoreToEdmx.Models
{
    /// <summary>
    /// Represents an association set in an EDMX model, connecting associations to entity sets.
    /// </summary>
    /// <remarks>
    /// Association sets define which entity sets participate in a relationship and provide
    /// the runtime context for association instances.
    /// </remarks>
    public class EdmxAssociationSet
    {

        /// <summary>
        /// Gets or sets the name of the association set.
        /// </summary>
        /// <value>
        /// A unique name for the association set within the entity container.
        /// Defaults to an empty string if not specified.
        /// </value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the association that this set instantiates.
        /// </summary>
        /// <value>
        /// The association name that defines the structure of relationships in this set.
        /// Defaults to an empty string if not specified.
        /// </value>
        public string Association { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first end of the association set.
        /// </summary>
        /// <value>
        /// An <see cref="EdmxAssociationSetEnd"/> connecting one association end to an entity set.
        /// Initialized to a new instance by default.
        /// </value>
        public EdmxAssociationSetEnd End1 { get; set; } = new();

        /// <summary>
        /// Gets or sets the second end of the association set.
        /// </summary>
        /// <value>
        /// An <see cref="EdmxAssociationSetEnd"/> connecting the other association end to an entity set.
        /// Initialized to a new instance by default.
        /// </value>
        public EdmxAssociationSetEnd End2 { get; set; } = new();

    }

}
