using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Unit tests for EDMX Designer section generation and schema compliance.
    /// </summary>
    [TestClass]
    public class EdmxDesignerSectionTests
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
                .UseInMemoryDatabase(databaseName: $"DesignerTestDatabase_{Guid.NewGuid()}")
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

        #region Designer Section Schema Compliance Tests

        [TestMethod]
        public void GeneratedEdmx_ShouldNotContainDiagramsElementInEdmxNamespace()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model);
            var xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            // Act
            var edmxContent = xmlGenerator.Generate();

            // Assert
            edmxContent.Should().NotContain("<Diagrams />", 
                "EDMX should not contain empty Diagrams element in the EDMX namespace to avoid schema validation warnings");
            
            edmxContent.Should().NotContain("xmlns=\"http://schemas.microsoft.com/ado/2009/11/edmx\">Diagrams", 
                "Diagrams element should not be in the EDMX namespace");
        }

        [TestMethod]
        public void GeneratedEdmx_DesignerSection_ShouldContainRequiredElements()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model);
            var xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            // Act
            var edmxContent = xmlGenerator.Generate();

            // Assert
            edmxContent.Should().Contain("<Designer", "Designer section should be present");
            edmxContent.Should().Contain("<Connection>", "Connection settings should be present");
            edmxContent.Should().Contain("<Options>", "Designer options should be present");
            edmxContent.Should().Contain("<easyaf:Extensions", "EasyAF extensions section should be present");
            edmxContent.Should().Contain("MetadataArtifactProcessing", "Connection settings should include metadata processing");
            edmxContent.Should().Contain("ValidateOnBuild", "Designer options should include validation settings");
        }

        [TestMethod]
        public void GeneratedEdmx_DesignerSection_ShouldHaveCorrectNamespaceDeclarations()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model);
            var xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            // Act
            var edmxContent = xmlGenerator.Generate();

            // Assert
            edmxContent.Should().Contain("xmlns=\"http://schemas.microsoft.com/ado/2009/11/edmx\"", 
                "Designer should declare the EDMX namespace");
            edmxContent.Should().Contain("xmlns:easyaf=\"http://schemas.cloudnimble.com/easyaf/2025/01/edmx\"", 
                "Designer should declare the EasyAF namespace for extensions");
        }

        #endregion

    }

}