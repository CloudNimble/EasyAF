using System;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models
{

    /// <summary>
    /// Represents a NAICS (North American Industry Classification System) code entity.
    /// </summary>
    /// <remarks>
    /// This entity demonstrates PostgreSQL ltree type handling for hierarchical classification codes.
    /// The ltree type is specifically designed for representing labels of data stored in a hierarchical
    /// tree-like structure. In this case, NAICS codes form a hierarchy (e.g., "31-33.334.3345.334510").
    /// </remarks>
    public class NaicsCode
    {

        /// <summary>
        /// Gets or sets the unique identifier for the NAICS code.
        /// </summary>
        /// <value>
        /// A GUID representing the primary key of the NAICS code.
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the hierarchical path of the NAICS code.
        /// </summary>
        /// <value>
        /// A string representing the hierarchical path using PostgreSQL ltree format.
        /// </value>
        /// <remarks>
        /// This property uses PostgreSQL's ltree data type which represents labels
        /// of data stored in a hierarchical tree-like structure. For example:
        /// "31-33" for Manufacturing sector,
        /// "31-33.334" for Computer and Electronic Product Manufacturing,
        /// "31-33.334.3345" for Navigational, Measuring, Electromedical, and Control Instruments Manufacturing,
        /// "31-33.334.3345.334510" for Electromedical and Electrotherapeutic Apparatus Manufacturing.
        /// </remarks>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the title/description of the NAICS code.
        /// </summary>
        /// <value>
        /// A string containing the descriptive title of the NAICS classification.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the numeric NAICS code.
        /// </summary>
        /// <value>
        /// A string containing the numeric NAICS code (e.g., "334510").
        /// </value>
        public string Code { get; set; }

    }

}
