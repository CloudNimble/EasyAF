using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Contains unit tests for the <see cref="EdmxConverter"/> class.
    /// </summary>
    /// <remarks>
    /// These tests verify both DbContext-based conversion and database scaffolding functionality.
    /// </remarks>
    [TestClass]
    public class EdmxConverterTests
    {

        #region Fields

        private EdmxConverter _converter;
        private string _tempDirectory;

        #endregion

        #region Test Setup and Cleanup

        /// <summary>
        /// Initializes test dependencies before each test method execution.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {

            _converter = new EdmxConverter();
            _tempDirectory = Path.Combine(Path.GetTempPath(), "EdmxConverterTests_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(_tempDirectory);

        }

        /// <summary>
        /// Cleans up test resources after each test method execution.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {

            if (Directory.Exists(_tempDirectory))
            {

                Directory.Delete(_tempDirectory, true);

            }

        }

        #endregion

        #region DbContext Conversion Tests

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ConvertToEdmx(DbContext)"/> returns valid result.
        /// </summary>
        [TestMethod]
        public void ConvertToEdmx_WithDbContext_ShouldReturnValidResult()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var result = _converter.ConvertToEdmx(context);

            result.Should().NotBeNull();
            result.DbContextName.Should().Be("TestDbContext");
            result.EdmxContent.Should().NotBeNullOrWhiteSpace();

            var doc = XDocument.Parse(result.EdmxContent);
            doc.Root.Should().NotBeNull();
            doc.Root!.Name.LocalName.Should().Be("Edmx");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ConvertToEdmx(DbContext)"/> throws exception for null context.
        /// </summary>
        [TestMethod]
        public void ConvertToEdmx_WithNullContext_ShouldThrowArgumentNullException()
        {

            var action = () => _converter.ConvertToEdmx((DbContext)null!);
            action.Should().Throw<ArgumentNullException>()
                  .And.ParamName.Should().Be("context");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ConvertToEdmxFileAsync"/> saves file correctly.
        /// </summary>
        [TestMethod]
        public async Task ConvertToEdmxFileAsync_WithValidContext_ShouldCreateFile()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);
            var filePath = Path.Combine(_tempDirectory, "test.edmx");

            await _converter.ConvertToEdmxFileAsync(context, filePath);

            File.Exists(filePath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(filePath);
            content.Should().NotBeNullOrWhiteSpace();

            var doc = XDocument.Parse(content);
            doc.Root.Should().NotBeNull();
            doc.Root!.Name.LocalName.Should().Be("Edmx");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ConvertToEdmxFileAsync"/> throws exception for null file path.
        /// </summary>
        [TestMethod]
        public async Task ConvertToEdmxFileAsync_WithNullFilePath_ShouldThrowArgumentException()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var action = async () => await _converter.ConvertToEdmxFileAsync(context, null!);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("filePath");

        }

        #endregion

        #region Path-based Conversion Tests

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ConvertToEdmx(string)"/> throws exception for empty path.
        /// </summary>
        [TestMethod]
        public void ConvertToEdmx_WithEmptyPath_ShouldThrowArgumentException()
        {

            var action = () => _converter.ConvertToEdmx(string.Empty);
            action.Should().Throw<ArgumentException>();

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ConvertToEdmx(string)"/> throws exception for non-existent path.
        /// </summary>
        [TestMethod]
        public void ConvertToEdmx_WithNonExistentPath_ShouldThrowArgumentException()
        {

            var nonExistentPath = Path.Combine(_tempDirectory, "nonexistent");

            var action = () => _converter.ConvertToEdmx(nonExistentPath);
            action.Should().Throw<ArgumentException>()
                  .WithMessage("*does not exist*");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ConvertToEdmx(string)"/> throws exception when no assemblies found.
        /// </summary>
        [TestMethod]
        public void ConvertToEdmx_WithNoAssemblies_ShouldThrowInvalidOperationException()
        {

            // Create empty directory
            var emptyDir = Path.Combine(_tempDirectory, "empty");
            Directory.CreateDirectory(emptyDir);

            var action = () => _converter.ConvertToEdmx(emptyDir);
            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("*No assemblies found*");

        }

        #endregion

        #region Design-Time Factory Tests

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ImplementsDesignTimeFactory"/> correctly identifies factory implementations.
        /// </summary>
        [TestMethod]
        public void ImplementsDesignTimeFactory_WithCustomFactory_ShouldReturnTrue()
        {

            var customFactoryType = typeof(CustomDbContextFactory);
            var contextType = typeof(TestDbContext);

            EdmxConverter.ImplementsDesignTimeFactory(customFactoryType, contextType).Should().BeTrue();

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ImplementsDesignTimeFactory"/> correctly identifies base factory implementations.
        /// </summary>
        [TestMethod]
        public void ImplementsDesignTimeFactory_WithBaseFactory_ShouldReturnTrue()
        {

            var baseFactoryType = typeof(MigrationDbContextFactory);
            var contextType = typeof(TestDbContext);

            EdmxConverter.ImplementsDesignTimeFactory(baseFactoryType, contextType).Should().BeTrue();

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ImplementsDesignTimeFactory"/> returns false for non-factory types.
        /// </summary>
        [TestMethod]
        public void ImplementsDesignTimeFactory_WithNonFactory_ShouldReturnFalse()
        {

            var nonFactoryType = typeof(TestDbContext);
            var contextType = typeof(TestDbContext);

            EdmxConverter.ImplementsDesignTimeFactory(nonFactoryType, contextType).Should().BeFalse();

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ImplementsDesignTimeFactory"/> returns false for wrong context type.
        /// </summary>
        [TestMethod]
        public void ImplementsDesignTimeFactory_WithWrongContextType_ShouldReturnFalse()
        {

            var factoryType = typeof(MigrationDbContextFactory);
            var wrongContextType = typeof(DbContext);

            EdmxConverter.ImplementsDesignTimeFactory(factoryType, wrongContextType).Should().BeFalse();

        }

        #endregion

        #region Configuration Management Tests

        /// <summary>
        /// Tests that <see cref="EdmxConverter.CreateConfigAsync"/> creates configuration file.
        /// </summary>
        [TestMethod]
        public async Task CreateConfigAsync_WithValidParameters_ShouldCreateFile()
        {

            var configPath = Path.Combine(_tempDirectory, "test.edmx.config");

            await _converter.CreateConfigAsync(
                configPath,
                "appsettings.json:ConnectionStrings:DefaultConnection",
                "SqlServer",
                "TestDbContext"
            );

            File.Exists(configPath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(configPath);
            content.Should().Contain("appsettings.json:ConnectionStrings:DefaultConnection");
            content.Should().Contain("SqlServer");
            content.Should().Contain("TestDbContext");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.CreateConfigAsync"/> throws exception for null path.
        /// </summary>
        [TestMethod]
        public async Task CreateConfigAsync_WithNullPath_ShouldThrowArgumentException()
        {

            var action = async () => await _converter.CreateConfigAsync(
                null!,
                "appsettings.json:ConnectionStrings:DefaultConnection",
                "SqlServer",
                "TestDbContext"
            );

            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("configPath");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.HasConfig"/> correctly identifies existing configurations.
        /// </summary>
        [TestMethod]
        public async Task HasConfig_WithExistingConfig_ShouldReturnTrue()
        {

            var edmxPath = Path.Combine(_tempDirectory, "test.edmx");
            var configPath = edmxPath + ".config";

            await _converter.CreateConfigAsync(
                configPath,
                "appsettings.json:ConnectionStrings:DefaultConnection",
                "SqlServer",
                "TestDbContext"
            );

            _converter.HasConfig(edmxPath).Should().BeTrue();

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.HasConfig"/> returns false for non-existent configurations.
        /// </summary>
        [TestMethod]
        public void HasConfig_WithNonExistentConfig_ShouldReturnFalse()
        {

            var edmxPath = Path.Combine(_tempDirectory, "nonexistent.edmx");

            _converter.HasConfig(edmxPath).Should().BeFalse();

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.HasConfig"/> throws exception for null path.
        /// </summary>
        [TestMethod]
        public void HasConfig_WithNullPath_ShouldThrowArgumentException()
        {

            var action = () => _converter.HasConfig(null!);
            action.Should().Throw<ArgumentException>()
                  .And.ParamName.Should().Be("edmxPath");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.HasConfig"/> throws exception for empty path.
        /// </summary>
        [TestMethod]
        public void HasConfig_WithEmptyPath_ShouldThrowArgumentException()
        {

            var action = () => _converter.HasConfig("");
            action.Should().Throw<ArgumentException>()
                  .And.ParamName.Should().Be("edmxPath");

        }

        #endregion

        #region Database Scaffolding Tests

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ConvertFromDatabaseAsync"/> throws exception for null config path.
        /// </summary>
        [TestMethod]
        public async Task ConvertFromDatabaseAsync_WithNullConfigPath_ShouldThrowArgumentException()
        {

            var action = async () => await _converter.ConvertFromDatabaseAsync(null!, _tempDirectory);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("configPath");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ConvertFromDatabaseAsync"/> throws exception for null project path.
        /// </summary>
        [TestMethod]
        public async Task ConvertFromDatabaseAsync_WithNullProjectPath_ShouldThrowArgumentException()
        {

            var configPath = Path.Combine(_tempDirectory, "test.edmx.config");

            var action = async () => await _converter.ConvertFromDatabaseAsync(configPath, null!);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("projectPath");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.ConvertFromDatabaseAsync"/> throws exception for non-existent config file.
        /// </summary>
        [TestMethod]
        public async Task ConvertFromDatabaseAsync_WithNonExistentConfig_ShouldThrowFileNotFoundException()
        {

            var configPath = Path.Combine(_tempDirectory, "nonexistent.edmx.config");

            var action = async () => await _converter.ConvertFromDatabaseAsync(configPath, _tempDirectory);
            await action.Should().ThrowAsync<FileNotFoundException>();

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.RefreshFromDatabaseAsync"/> throws exception for null EDMX path.
        /// </summary>
        [TestMethod]
        public async Task RefreshFromDatabaseAsync_WithNullEdmxPath_ShouldThrowArgumentException()
        {

            var action = async () => await _converter.RefreshFromDatabaseAsync(null!, _tempDirectory);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("edmxPath");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.RefreshFromDatabaseAsync"/> throws exception for null project path.
        /// </summary>
        [TestMethod]
        public async Task RefreshFromDatabaseAsync_WithNullProjectPath_ShouldThrowArgumentException()
        {

            var edmxPath = Path.Combine(_tempDirectory, "test.edmx");

            var action = async () => await _converter.RefreshFromDatabaseAsync(edmxPath, null!);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("projectPath");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConverter.RefreshFromDatabaseAsync"/> throws exception for non-existent EDMX file.
        /// </summary>
        [TestMethod]
        public async Task RefreshFromDatabaseAsync_WithNonExistentEdmx_ShouldThrowFileNotFoundException()
        {

            var edmxPath = Path.Combine(_tempDirectory, "nonexistent.edmx");

            var action = async () => await _converter.RefreshFromDatabaseAsync(edmxPath, _tempDirectory);
            await action.Should().ThrowAsync<FileNotFoundException>();

        }

        /// <summary>
        /// Tests the complete workflow from configuration creation to database scaffolding simulation.
        /// </summary>
        [TestMethod]
        public async Task DatabaseScaffolding_CompleteWorkflow_ShouldCreateExpectedFiles()
        {

            // Create mock appsettings.json file
            var appsettingsContent = """
            {
              "ConnectionStrings": {
                "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=true;"
              }
            }
            """;

            var appsettingsPath = Path.Combine(_tempDirectory, "appsettings.json");
            await File.WriteAllTextAsync(appsettingsPath, appsettingsContent);

            // Create configuration
            var configPath = Path.Combine(_tempDirectory, "TestContext.edmx.config");
            await _converter.CreateConfigAsync(
                configPath,
                "appsettings.json:ConnectionStrings:DefaultConnection",
                "SqlServer",
                "TestContext"
            );

            // Verify configuration was created
            File.Exists(configPath).Should().BeTrue();

            // Verify HasConfig returns true
            var edmxPath = Path.Combine(_tempDirectory, "TestContext.edmx");
            _converter.HasConfig(edmxPath).Should().BeTrue();

            // Note: We can't test actual database scaffolding without a real database connection
            // But we can test that the methods accept the parameters correctly

        }

        #endregion

    }

}
