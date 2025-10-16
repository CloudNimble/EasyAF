// EdmxReferentialConstraintRole.cs
using System.Collections.Generic;

// EdmxReferentialConstraintRole.cs
namespace CloudNimble.EasyAF.EFCoreToEdmx.Models
{
    /// <summary>
    /// Represents one role in a referential constraint, containing the property mappings.
    /// </summary>
    /// <remarks>
    /// A referential constraint role defines which properties participate in the foreign key
    /// relationship for either the principal or dependent side of the constraint.
    /// </remarks>
    public class EdmxReferentialConstraintRole
    {

        /// <summary>
        /// Gets or sets the role name for this side of the referential constraint.
        /// </summary>
        /// <value>
        /// The role name that matches one of the association ends.
        /// Defaults to an empty string if not specified.
        /// </value>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of property names that participate in the constraint.
        /// </summary>
        /// <value>
        /// A list of property names that form the key for this side of the constraint.
        /// Initialized to an empty list by default.
        /// </value>
        public List<string> PropertyRefs { get; set; } = new();

    }

}

