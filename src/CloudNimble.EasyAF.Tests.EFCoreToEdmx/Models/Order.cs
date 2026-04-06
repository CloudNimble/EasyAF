using System;
using System.Collections.Generic;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models
{
    /// <summary>
    /// Represents an order entity in the test database model.
    /// </summary>
    /// <remarks>
    /// This entity demonstrates foreign key relationships, decimal precision,
    /// enumeration properties, and serves as both dependent (to User) and
    /// principal (to OrderItem) in different relationships.
    /// </remarks>
    public class Order
    {

        /// <summary>
        /// Gets or sets the unique identifier for the order.
        /// </summary>
        /// <value>
        /// An integer representing the primary key of the order.
        /// </value>
        /// <remarks>
        /// This property serves as the primary key for the Order entity.
        /// </remarks>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the order number.
        /// </summary>
        /// <value>
        /// A string containing the unique order number.
        /// Defaults to an empty string if not specified.
        /// </value>
        /// <remarks>
        /// This property is configured as required with a maximum length of 50 characters
        /// and typically contains a business-meaningful order identifier.
        /// </remarks>
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the foreign key reference to the associated user.
        /// </summary>
        /// <value>
        /// An integer representing the ID of the user who placed this order.
        /// </value>
        /// <remarks>
        /// This property serves as the foreign key in the User-Order relationship
        /// and is configured with cascade delete behavior.
        /// </remarks>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the total amount of the order.
        /// </summary>
        /// <value>
        /// A decimal value representing the total monetary amount of the order.
        /// </value>
        /// <remarks>
        /// This property is configured with decimal precision (18,2) to properly
        /// handle monetary values and demonstrates precision/scale configuration.
        /// </remarks>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the date when the order was placed.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> representing when the order was created.
        /// </value>
        /// <remarks>
        /// This property demonstrates DateTime handling in the model conversion process.
        /// </remarks>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the current status of the order.
        /// </summary>
        /// <value>
        /// An <see cref="OrderStatus"/> enumeration value representing the order's current state.
        /// </value>
        /// <remarks>
        /// This property demonstrates enumeration type mapping and shows how
        /// enum properties are handled in the EDMX conversion process.
        /// </remarks>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the user associated with this order.
        /// </summary>
        /// <value>
        /// A <see cref="User"/> entity representing the customer who placed the order.
        /// </value>
        /// <remarks>
        /// This navigation property represents the "one" side of the User-Order
        /// relationship and demonstrates reference navigation properties.
        /// </remarks>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the collection of order items associated with this order.
        /// </summary>
        /// <value>
        /// A collection of <see cref="OrderItem"/> entities that belong to this order.
        /// Initialized to an empty list by default.
        /// </value>
        /// <remarks>
        /// This navigation property represents the "many" side of the Order-OrderItem
        /// relationship and demonstrates collection navigation properties.
        /// </remarks>
        public virtual ICollection<OrderItem> OrderItems { get; set; } = [];

    }

}
