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
    /// are correctly mapped to appropriate CLR types (DateTimeOffset) in the conceptual model.
    /// The storage model always uses SQL Server types for EDMX compatibility, as the EDMX
    /// ProviderManifestToken is set to "2012.Azure" and the storage types are not used by
    /// the code generator (only the conceptual model CLR types matter).
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
        public void GenerateEdmxWithPostgreSQLProvider_ShouldMapDateTimeOffsetCorrectly()
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

            // The storage model uses SQL Server types for EDMX compatibility
            // (ProviderManifestToken is "2012.Azure" regardless of source database)
            result.EdmxContent.Should().Contain("Type=\"datetimeoffset\"",
                "Storage model should use SQL Server 'datetimeoffset' type for EDMX compatibility");

            // The conceptual model should use DateTimeOffset CLR type
            // This is what the code generator uses
            result.EdmxContent.Should().Contain("Type=\"DateTimeOffset\"",
                "Conceptual model should use DateTimeOffset CLR type");
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
        public void PostgreSQLTypeMappingLogic_ShouldUseConceptualModelTypes()
        {
            // This test verifies that PostgreSQL sources produce correct conceptual model types
            // The storage model always uses SQL Server types for EDMX compatibility
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(
                model,
                providerType: CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.PostgreSQL
            );

            // Create XML generator with explicit PostgreSQL provider type
            var xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.PostgreSQL);
            var edmxContent = xmlGenerator.Generate();

            var result = new CloudNimble.EasyAF.EFCoreToEdmx.Models.EdmxConversionResult("TestDbContext", edmxContent);

            // Verify the conceptual model contains correct CLR types (what the code generator uses)
            result.EdmxContent.Should().Contain("Type=\"String\"",
                "Conceptual model should have String type for string properties");
            result.EdmxContent.Should().Contain("Type=\"Int32\"",
                "Conceptual model should have Int32 type for int properties");
            result.EdmxContent.Should().Contain("Type=\"Guid\"",
                "Conceptual model should have Guid type for Guid properties");
            result.EdmxContent.Should().Contain("Type=\"Boolean\"",
                "Conceptual model should have Boolean type for bool properties");
            result.EdmxContent.Should().Contain("Type=\"DateTimeOffset\"",
                "Conceptual model should have DateTimeOffset type for DateTimeOffset properties");

            // Verify storage model uses SQL Server types
            result.EdmxContent.Should().Contain("Type=\"nvarchar\"",
                "Storage model should use SQL Server nvarchar type");
            result.EdmxContent.Should().Contain("Type=\"datetimeoffset\"",
                "Storage model should use SQL Server datetimeoffset type");
        }

        [TestMethod]
        public void GenerateEdmxWithPostgreSQLProvider_ShouldConvertLTreeToString()
        {
            // This test verifies that PostgreSQL ltree type is converted to String in the conceptual model
            var model = _context.Model;

            // Build EDMX model with PostgreSQL provider type
            var edmxModel = _modelBuilder.BuildEdmxModel(
                model,
                @namespace: "TestNamespace",
                name: "TestContainer",
                providerType: CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.PostgreSQL
            );

            // Verify NaicsCode entity exists in the model
            var naicsCodeEntity = edmxModel.EntityTypes.FirstOrDefault(e => e.Name == "NaicsCode");
            naicsCodeEntity.Should().NotBeNull("NaicsCode entity should exist in the model");

            // Verify Path property is mapped to String in the conceptual model
            var pathProperty = naicsCodeEntity.Properties.FirstOrDefault(p => p.Name == "Path");
            pathProperty.Should().NotBeNull("Path property should exist on NaicsCode entity");
            pathProperty.Type.Should().Be("String",
                "PostgreSQL ltree type should be converted to String in the conceptual model");

            // Create XML generator with explicit PostgreSQL provider type
            var xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.PostgreSQL);
            var edmxContent = xmlGenerator.Generate();

            var result = new CloudNimble.EasyAF.EFCoreToEdmx.Models.EdmxConversionResult("TestDbContext", edmxContent);

            result.Should().NotBeNull();
            result.EdmxContent.Should().NotBeNullOrEmpty();

            // The conceptual model should use String for ltree columns
            result.EdmxContent.Should().Contain("Type=\"String\"",
                "Conceptual model should convert ltree to String type");

            // The conceptual model should NOT contain Type="LTree"
            result.EdmxContent.Should().NotContain("Type=\"LTree\"",
                "Conceptual model should not contain LTree as a type (it should be converted to String)");

            // The storage model uses SQL Server types for EDMX compatibility
            // ltree maps to String which maps to nvarchar in SQL Server
            result.EdmxContent.Should().Contain("Type=\"nvarchar\"",
                "Storage model should use SQL Server nvarchar type for string properties");

            // Print EDMX for debugging
            Console.WriteLine("PostgreSQL LTree EDMX Content:");
            Console.WriteLine(result.EdmxContent);
        }

        #endregion

        #region Issue Reproduction Tests

        [TestMethod]
        public void ReproduceIssue_PostgreSQLTimestampWithTimeZoneNotRecognizedAsDateTimeOffset()
        {
            // This test verifies that DateTimeOffset CLR types are correctly identified
            // in the conceptual model regardless of the source database provider.
            // The storage model always uses SQL Server types for EDMX compatibility
            // (ProviderManifestToken is "2012.Azure").

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

            // The conceptual model should correctly identify DateTimeOffset
            // This is what the code generator uses
            result.EdmxContent.Should().Contain("Type=\"DateTimeOffset\"",
                "Conceptual model should recognize DateTimeOffset CLR type");

            // Storage model should use SQL Server types for EDMX compatibility
            result.EdmxContent.Should().Contain("Type=\"datetimeoffset\"",
                "Storage model should use SQL Server 'datetimeoffset' type for EDMX compatibility");

            // Storage model should NOT contain PostgreSQL-specific types
            result.EdmxContent.Should().NotContain("timestamp with time zone",
                "Storage model should not contain PostgreSQL 'timestamp with time zone' type");

            // Print EDMX for debugging
            Console.WriteLine("PostgreSQL EDMX Content:");
            Console.WriteLine(result.EdmxContent);
        }

        #endregion

    }

}