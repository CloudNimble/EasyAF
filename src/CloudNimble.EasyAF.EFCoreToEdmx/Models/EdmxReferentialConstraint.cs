// EdmxReferentialConstraint.cs
// EdmxReferentialConstraint.cs
namespace CloudNimble.EasyAF.EFCoreToEdmx.Models
{
    /// <summary>
    /// Represents a referential constraint in an EDMX association, defining foreign key mappings.
    /// </summary>
    /// <remarks>
    /// Referential constraints specify how properties on the dependent entity map to properties
    /// on the principal entity, ensuring referential integrity in the conceptual model.
    /// </remarks>
    public class EdmxReferentialConstraint
    {

        /// <summary>
        /// Gets or sets the principal role of the referential constraint.
        /// </summary>
        /// <value>
        /// An <see cref="EdmxReferentialConstraintRole"/> representing the principal (referenced) side.
        /// Initialized to a new instance by default.
        /// </value>
        public EdmxReferentialConstraintRole Principal { get; set; } = new();

        /// <summary>
        /// Gets or sets the dependent role of the referential constraint.
        /// </summary>
        /// <value>
        /// An <see cref="EdmxReferentialConstraintRole"/> representing the dependent (referencing) side.
        /// Initialized to a new instance by default.
        /// </value>
        public EdmxReferentialConstraintRole Dependent { get; set; } = new();

    }

}

