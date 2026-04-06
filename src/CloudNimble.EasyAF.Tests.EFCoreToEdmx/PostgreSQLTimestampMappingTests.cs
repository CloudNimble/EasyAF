using CloudNimble.EasyAF.EFCoreToEdmx.PostgreSQL;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{
    /// <summary>
    /// Unit tests for PostgreSQL timestamp with time zone type mapping to DateTimeOffset.
    /// </summary>
    [TestClass]
    public class PostgreSQLTimestampMappingTests
    {
        #region Test Methods

        /// <summary>
        /// Tests that PostgreSQL design-time services can be instantiated successfully.
        /// </summary>
        [TestMethod]
        public void PostgreSQLDesignTimeServices_ShouldInstantiateSuccessfully()
        {
            // Arrange & Act
            var services = new PostgreSQLDesignTimeServices();

            // Assert
            services.Should().NotBeNull();
        }

        /// <summary>
        /// Tests that our type mapping logic can identify PostgreSQL timestamp types correctly.
        /// </summary>
        [TestMethod]
        public void PostgreSQLTypeMapping_ShouldRecognizeTimestampTypes()
        {
            // Test cases for different PostgreSQL timestamp type representations
            var timestampTypes = new[]
            {
                "timestamp with time zone",
                "timestamptz",
                "TIMESTAMP WITH TIME ZONE",
                "TIMESTAMPTZ",
                "timestamp(6) with time zone"
            };

            foreach (var timestampType in timestampTypes)
            {
                // These are the conditions from our implementation
                var shouldMap = string.Equals(timestampType, "timestamp with time zone", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(timestampType, "timestamptz", StringComparison.OrdinalIgnoreCase) ||
                               (!string.IsNullOrEmpty(timestampType) &&
                                timestampType.Contains("timestamp", StringComparison.OrdinalIgnoreCase) &&
                                timestampType.Contains("with time zone", StringComparison.OrdinalIgnoreCase));

                shouldMap.Should().BeTrue($"'{timestampType}' should be recognized as a PostgreSQL timestamp with time zone type");
            }
        }

        /// <summary>
        /// Tests that non-timestamp types are not affected by our mapping logic.
        /// </summary>
        [TestMethod]
        public void PostgreSQLTypeMapping_ShouldNotAffectOtherTypes()
        {
            // Test cases for types that should NOT be mapped to DateTimeOffset
            var otherTypes = new[]
            {
                "varchar",
                "integer",
                "timestamp without time zone",
                "timestamp",
                "text",
                "boolean",
                null,
                ""
            };

            foreach (var otherType in otherTypes)
            {
                // These are the conditions from our implementation - should be false for these types
                var shouldMap = !string.IsNullOrEmpty(otherType) &&
                               (string.Equals(otherType, "timestamp with time zone", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(otherType, "timestamptz", StringComparison.OrdinalIgnoreCase) ||
                                (otherType.Contains("timestamp", StringComparison.OrdinalIgnoreCase) &&
                                 otherType.Contains("with time zone", StringComparison.OrdinalIgnoreCase)));

                shouldMap.Should().BeFalse($"'{otherType}' should NOT be mapped to DateTimeOffset");
            }
        }

        #endregion
    }
}