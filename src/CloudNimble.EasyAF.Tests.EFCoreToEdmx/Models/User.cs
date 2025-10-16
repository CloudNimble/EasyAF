using System;
using System.Collections.Generic;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models
{
    /// <summary>
    /// Represents a user entity in the test database model.
    /// </summary>
    /// <remarks>
    /// This entity demonstrates various property types including strings, DateTime,
    /// and boolean values. It serves as the principal entity in a one-to-many
    /// relationship with Orders and includes examples of required and optional properties.
    /// </remarks>
    public class User
    {

        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        /// <value>
        /// An integer representing the primary key of the user.
        /// </value>
        /// <remarks>
        /// This property serves as the primary key and is typically configured
        /// as an identity column in the database.
        /// </remarks>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        /// <value>
        /// A string containing the user's email address.
        /// Defaults to an empty string if not specified.
        /// </value>
        /// <remarks>
        /// This property is configured as required with a maximum length of 255 characters
        /// and has a unique index constraint.
        /// </remarks>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        /// <value>
        /// A string containing the user's first name.
        /// Defaults to an empty string if not specified.
        /// </value>
        /// <remarks>
        /// This property has a maximum length constraint of 100 characters
        /// and includes documentation comments in the database.
        /// </remarks>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        /// <value>
        /// A string containing the user's last name.
        /// Defaults to an empty string if not specified.
        /// </value>
        /// <remarks>
        /// This property has a maximum length constraint of 100 characters.
        /// </remarks>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the user was created.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> representing when the user record was created.
        /// </value>
        /// <remarks>
        /// This property is configured with a default value SQL expression
        /// and demonstrates computed/default value scenarios.
        /// </remarks>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is active.
        /// </summary>
        /// <value>
        /// <c>true</c> if the user is active; otherwise, <c>false</c>.
        /// Defaults to <c>true</c>.
        /// </value>
        /// <remarks>
        /// This boolean property demonstrates simple flag scenarios and
        /// provides an example of a property with a default value.
        /// </remarks>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the collection of orders associated with this user.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Order"/> entities related to this user.
        /// Initialized to an empty list by default.
        /// </value>
        /// <remarks>
        /// This navigation property represents the "many" side of a one-to-many
        /// relationship and demonstrates collection navigation properties.
        /// </remarks>
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    }

}
