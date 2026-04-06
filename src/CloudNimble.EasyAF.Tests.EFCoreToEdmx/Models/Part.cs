using System;
using System.Collections.Generic;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models
{

    /// <summary>
    /// Represents a part entity that demonstrates self-referencing relationships.
    /// </summary>
    /// <remarks>
    /// This entity models a hierarchical part structure where parts can be components
    /// of other parts, creating parent-child relationships within the same table.
    /// This pattern is commonly used for bill-of-materials, organizational hierarchies,
    /// and other tree-like data structures.
    /// </remarks>
    public class Part
    {

        /// <summary>
        /// Gets or sets the unique identifier for the part.
        /// </summary>
        /// <value>
        /// A GUID representing the primary key of the part.
        /// </value>
        /// <remarks>
        /// Using GUID as primary key to match the PostgreSQL table structure
        /// and demonstrate non-integer primary keys in relationship handling.
        /// </remarks>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the parent part.
        /// </summary>
        /// <value>
        /// A nullable GUID representing the foreign key to the parent part.
        /// Null indicates this is a root-level part with no parent.
        /// </value>
        /// <remarks>
        /// This property creates the self-referencing foreign key relationship.
        /// The nullability allows for root-level parts that don't have a parent.
        /// </remarks>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer location identifier.
        /// </summary>
        /// <value>
        /// A GUID representing where this part is manufactured.
        /// </value>
        /// <remarks>
        /// Required field demonstrating non-nullable foreign key to another entity.
        /// </remarks>
        public Guid ManufacturerLocationId { get; set; }

        /// <summary>
        /// Gets or sets the internal identifier for the part.
        /// </summary>
        /// <value>
        /// An optional string containing the internal part number or identifier.
        /// </value>
        /// <remarks>
        /// Optional field for internal tracking purposes.
        /// </remarks>
        public string InternalId { get; set; }

        /// <summary>
        /// Gets or sets the display name for the part.
        /// </summary>
        /// <value>
        /// A string containing the human-readable name of the part.
        /// Defaults to an empty string if not specified.
        /// </value>
        /// <remarks>
        /// Required field for displaying the part to users.
        /// </remarks>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Universal Product Code for the part.
        /// </summary>
        /// <value>
        /// An optional string containing the 12-character UPC code.
        /// </value>
        /// <remarks>
        /// Limited to 12 characters to match standard UPC format.
        /// </remarks>
        public string UniversalProductCode { get; set; }

        /// <summary>
        /// Gets or sets the description of the part.
        /// </summary>
        /// <value>
        /// An optional string containing detailed description of the part.
        /// </value>
        /// <remarks>
        /// Optional field for detailed part information.
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the part was created.
        /// </summary>
        /// <value>
        /// A <see cref="DateTimeOffset"/> representing when the part record was created.
        /// </value>
        /// <remarks>
        /// Using DateTimeOffset to match PostgreSQL timestamp with time zone.
        /// </remarks>
        public DateTimeOffset DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who created the part.
        /// </summary>
        /// <value>
        /// A GUID representing the user who created this part record.
        /// </value>
        /// <remarks>
        /// Required field for audit tracking purposes.
        /// </remarks>
        public Guid CreatedById { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the part was last updated.
        /// </summary>
        /// <value>
        /// An optional <see cref="DateTimeOffset"/> representing when the part was last modified.
        /// </value>
        /// <remarks>
        /// Nullable field that tracks the last modification time.
        /// </remarks>
        public DateTimeOffset? DateUpdated { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who last updated the part.
        /// </summary>
        /// <value>
        /// A GUID representing the user who last modified this part record.
        /// </value>
        /// <remarks>
        /// Required field for audit tracking purposes, even on creation.
        /// </remarks>
        public Guid UpdatedById { get; set; }

        /// <summary>
        /// Gets or sets the parent part navigation property.
        /// </summary>
        /// <value>
        /// A <see cref="Part"/> entity representing the parent part.
        /// Null if this is a root-level part.
        /// </value>
        /// <remarks>
        /// This navigation property represents the "one" side of the self-referencing
        /// one-to-many relationship, allowing navigation from child to parent.
        /// </remarks>
        public virtual Part ParentPart { get; set; }

        /// <summary>
        /// Gets or sets the collection of child parts.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Part"/> entities that are components of this part.
        /// Initialized to an empty list by default.
        /// </value>
        /// <remarks>
        /// This navigation property represents the "many" side of the self-referencing
        /// one-to-many relationship, allowing navigation from parent to children.
        /// </remarks>
        public virtual ICollection<Part> ChildParts { get; set; } = new List<Part>();

    }

}