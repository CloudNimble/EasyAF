using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Unit tests for PostgreSQL type mapping in EF Core to EDMX conversion.
    /// </summary>
    /// <remarks>
    /// These tests verify that PostgreSQL-specific data types like "timestamp with time zone"
    /// are correctly mapped to appropriate CLR types (DateTimeOffset) and EDMX storage types.
    /// </remarks>
    [TestClass]
    public class PostgreSQLTypeTests
    {

        #region Fields

        private TestDbContext _context;
        private EdmxModelBuilder _modelBuilder;

        #endregion

        #region Test Setup and Cleanup

        /// <summary>
        /// Initializes test fixtures before each test method execution.
        /// </summary>
        [TestInitialize]
        public async Task TestInitialize()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: $"PostgreSQLTestDatabase_{Guid.NewGuid()}")
                .Options;

            _context = new TestDbContext(options);
            await _context.Database.EnsureCreatedAsync();

            _modelBuilder = new EdmxModelBuilder();
        }

        /// <summary>
        /// Cleans up test fixtures after each test method execution.
        /// </summary>
        [TestCleanup]
        public async Task TestCleanup()
        {
            if (_context is not null)
            {
                await _context.Database.EnsureDeletedAsync();
                await _context.DisposeAsync();
            }
        }

        #endregion

        #region PostgreSQL Type Mapping Tests

        [TestMethod]
        public void BuildEdmxModel_WithDateTimeOffsetProperties_ShouldRecognizeAsDateTimeOffset()
        {
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(model);

            var partEntity = edmxModel.EntityTypes.FirstOrDefault(e => e.Name == "Part");
            partEntity.Should().NotBeNull();

            // Verify DateTimeOffset properties are correctly identified in the conceptual model
            var dateCreatedProperty = partEntity.Properties.FirstOrDefault(p => p.Name == "DateCreated");
            var dateUpdatedProperty = partEntity.Properties.FirstOrDefault(p => p.Name == "DateUpdated");

            dateCreatedProperty.Should().NotBeNull();
            dateCreatedProperty.Type.Should().Be("DateTimeOffset", "DateCreated should be recognized as DateTimeOffset CLR type");

            dateUpdatedProperty.Should().NotBeNull();
            dateUpdatedProperty.Type.Should().Be("DateTimeOffset", "DateUpdated should be recognized as DateTimeOffset CLR type");
        }

        [TestMethod]
        public void GenerateEdmxWithPostgreSQLProvider_ShouldMapDateTimeOffsetToTimestampWithTimeZone()
        {
            var model = _context.Model;

            // Build EDMX model with PostgreSQL provider type
            var edmxModel = _modelBuilder.BuildEdmxModel(
                model, 
                @namespace: "TestNamespace", 
                name: "TestContainer", 
                providerType: CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.PostgreSQL
            );

            // Create XML generator with explicit PostgreSQL provider type
            var xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.PostgreSQL);
            var edmxContent = xmlGenerator.Generate();

            var result = new CloudNimble.EasyAF.EFCoreToEdmx.Models.EdmxConversionResult("TestDbContext", edmxContent);

            result.Should().NotBeNull();
            result.EdmxContent.Should().NotBeNullOrEmpty();

            // In the SSDL (Storage Schema Definition Language) section, 
            // DateTimeOffset should be mapped to "timestamp with time zone" for PostgreSQL
            result.EdmxContent.Should().Contain("timestamp with time zone", 
                "PostgreSQL storage model should map DateTimeOffset to 'timestamp with time zone'");

            // Verify it doesn't contain SQL Server-specific datetimeoffset
            result.EdmxContent.Should().NotContain("Type=\"datetimeoffset\"", 
                "PostgreSQL storage model should not contain SQL Server 'datetimeoffset' type");

            // The conceptual model should still use DateTimeOffset
            result.EdmxContent.Should().Contain("Type=\"DateTimeOffset\"", 
                "Conceptual model should still use DateTimeOffset CLR type");
        }

        [TestMethod]
        public void GenerateEdmxWithSqlServerProvider_ShouldMapDateTimeOffsetToDateTimeOffset()
        {
            var model = _context.Model;

            var converter = new EdmxConverter();
            var result = converter.ConvertToEdmx(_context);

            result.Should().NotBeNull();
            result.EdmxContent.Should().NotBeNullOrEmpty();

            // In the SSDL for SQL Server, DateTimeOffset should be mapped to "datetimeoffset"
            result.EdmxContent.Should().Contain("Type=\"datetimeoffset\"", 
                "SQL Server storage model should map DateTimeOffset to 'datetimeoffset'");

            // Verify it doesn't contain PostgreSQL-specific timestamp with time zone
            result.EdmxContent.Should().NotContain("timestamp with time zone", 
                "SQL Server storage model should not contain PostgreSQL 'timestamp with time zone' type");

            // The conceptual model should use DateTimeOffset
            result.EdmxContent.Should().Contain("Type=\"DateTimeOffset\"", 
                "Conceptual model should use DateTimeOffset CLR type");
        }

        [TestMethod]
        public void PostgreSQLTypeMappingLogic_ShouldHandleAllCommonTypes()
        {
            // This test verifies the PostgreSQL type mapping logic directly
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(
                model, 
                providerType: CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.PostgreSQL
            );

            // Create XML generator with explicit PostgreSQL provider type
            var xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.PostgreSQL);
            var edmxContent = xmlGenerator.Generate();

            var result = new CloudNimble.EasyAF.EFCoreToEdmx.Models.EdmxConversionResult("TestDbContext", edmxContent);

            // Verify various PostgreSQL type mappings in the generated EDMX
            result.EdmxContent.Should().Contain("character varying", 
                "PostgreSQL should map string properties to 'character varying'");
            result.EdmxContent.Should().Contain("integer", 
                "PostgreSQL should map int properties to 'integer'");
            result.EdmxContent.Should().Contain("uuid", 
                "PostgreSQL should map Guid properties to 'uuid'");
            result.EdmxContent.Should().Contain("boolean", 
                "PostgreSQL should map bool properties to 'boolean'");
            result.EdmxContent.Should().Contain("timestamp with time zone", 
                "PostgreSQL should map DateTimeOffset properties to 'timestamp with time zone'");
        }

        #endregion

        #region Issue Reproduction Tests

        [TestMethod]
        public void ReproduceIssue_PostgreSQLTimestampWithTimeZoneNotRecognizedAsDateTimeOffset()
        {
            // This test reproduces the issue where PostgreSQL "timestamp with time zone" 
            // columns are not being recognized as DateTimeOffset CLR types during 
            // reverse engineering

            var model = _context.Model;

            // Build EDMX model with PostgreSQL provider type
            var edmxModel = _modelBuilder.BuildEdmxModel(
                model, 
                providerType: CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.PostgreSQL
            );

            // Create XML generator with explicit PostgreSQL provider type
            var xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.PostgreSQL);
            var edmxContent = xmlGenerator.Generate();

            var result = new CloudNimble.EasyAF.EFCoreToEdmx.Models.EdmxConversionResult("TestDbContext", edmxContent);

            result.Should().NotBeNull();
            result.EdmxContent.Should().NotBeNullOrEmpty();

            // The issue: Check if the conceptual model correctly identifies DateTimeOffset
            // Note: This test currently uses in-memory database, so EF Core already knows
            // the CLR types. The real issue occurs during reverse engineering from actual PostgreSQL.
            result.EdmxContent.Should().Contain("Type=\"DateTimeOffset\"", 
                "Conceptual model should recognize timestamp with time zone as DateTimeOffset");

            // Storage model should use PostgreSQL-specific types
            result.EdmxContent.Should().Contain("timestamp with time zone", 
                "Storage model should use PostgreSQL 'timestamp with time zone' type");

            // Print EDMX for debugging
            Console.WriteLine("PostgreSQL EDMX Content:");
            Console.WriteLine(result.EdmxContent);
        }

        #endregion

    }

}