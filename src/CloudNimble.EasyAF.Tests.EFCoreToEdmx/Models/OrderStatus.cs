using System.ComponentModel.DataAnnotations;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models
{

    /// <summary>
    /// Represents the possible states of an order in the system.
    /// </summary>
    /// <remarks>
    /// This enumeration demonstrates how enum types are handled in the EF Core to EDMX
    /// conversion process and provides a realistic example of order workflow states.
    /// </remarks>
    public enum OrderStatus
    {

        /// <summary>
        /// The order has been created but not yet processed.
        /// </summary>
        /// <remarks>
        /// This is typically the initial state when an order is first placed.
        /// </remarks>
        Pending = 0,

        /// <summary>
        /// The order is currently being processed.
        /// </summary>
        /// <remarks>
        /// This state indicates that the order is being prepared or fulfilled.
        /// </remarks>
        Processing = 1,

        /// <summary>
        /// The order has been shipped to the customer.
        /// </summary>
        /// <remarks>
        /// This state indicates that the order has left the fulfillment center.
        /// </remarks>
        Shipped = 2,

        /// <summary>
        /// The order has been delivered to the customer.
        /// </summary>
        /// <remarks>
        /// This is the final successful state of an order.
        /// </remarks>
        Delivered = 3,

        /// <summary>
        /// The order has been cancelled.
        /// </summary>
        /// <remarks>
        /// This state indicates that the order was cancelled before completion.
        /// </remarks>
        Cancelled = 4

    }

}
