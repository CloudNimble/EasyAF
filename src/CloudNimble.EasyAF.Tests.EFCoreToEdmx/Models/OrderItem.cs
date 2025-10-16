namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models
{
    /// <summary>
    /// Represents an order item entity in the test database model.
    /// </summary>
    /// <remarks>
    /// This entity demonstrates a dependent relationship scenario where it depends
    /// on the Order entity through a foreign key relationship. It includes various
    /// numeric property types for testing type mapping scenarios.
    /// </remarks>
    public class OrderItem
    {

        /// <summary>
        /// Gets or sets the unique identifier for the order item.
        /// </summary>
        /// <value>
        /// An integer representing the primary key of the order item.
        /// </value>
        /// <remarks>
        /// This property serves as the primary key for the OrderItem entity.
        /// </remarks>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the foreign key reference to the associated order.
        /// </summary>
        /// <value>
        /// An integer representing the ID of the order this item belongs to.
        /// </value>
        /// <remarks>
        /// This property serves as the foreign key in the Order-OrderItem relationship
        /// and is configured with cascade delete behavior.
        /// </remarks>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <value>
        /// A string containing the product name.
        /// Defaults to an empty string if not specified.
        /// </value>
        /// <remarks>
        /// This property is configured as required with a maximum length of 200 characters.
        /// </remarks>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the quantity of the product ordered.
        /// </summary>
        /// <value>
        /// An integer representing the number of units ordered.
        /// </value>
        /// <remarks>
        /// This property demonstrates integer type mapping and quantity handling.
        /// </remarks>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price of the product.
        /// </summary>
        /// <value>
        /// A decimal value representing the price per unit of the product.
        /// </value>
        /// <remarks>
        /// This property is configured with decimal precision (18,2) for monetary values
        /// and demonstrates precision/scale configuration scenarios.
        /// </remarks>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the order associated with this order item.
        /// </summary>
        /// <value>
        /// An <see cref="Order"/> entity representing the order this item belongs to.
        /// </value>
        /// <remarks>
        /// This navigation property represents the "one" side of the Order-OrderItem
        /// relationship and demonstrates reference navigation properties in dependent entities.
        /// </remarks>
        public virtual Order Order { get; set; } = null!;

    }

}
