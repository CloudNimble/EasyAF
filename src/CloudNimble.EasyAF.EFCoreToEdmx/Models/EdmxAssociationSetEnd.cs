// EdmxAssociationSetEnd.cs
// EdmxAssociationSetEnd.cs
namespace CloudNimble.EasyAF.EFCoreToEdmx.Models
{
    /// <summary>
    /// Represents one end of an association set, connecting an association end to an entity set.
    /// </summary>
    /// <remarks>
    /// Association set ends specify which entity set contains the entities that participate
    /// in each end of a relationship instance.
    /// </remarks>
    public class EdmxAssociationSetEnd
    {

        /// <summary>
        /// Gets or sets the role name that matches the corresponding association end.
        /// </summary>
        /// <value>
        /// The role name from the association definition.
        /// Defaults to an empty string if not specified.
        /// </value>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the entity set for this end.
        /// </summary>
        /// <value>
        /// The entity set name that contains entities participating in this end of the relationship.
        /// Defaults to an empty string if not specified.
        /// </value>
        public string EntitySet { get; set; } = string.Empty;

    }

}
