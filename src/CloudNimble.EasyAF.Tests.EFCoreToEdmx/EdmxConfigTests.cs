using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Contains unit tests for the <see cref="EdmxConfig"/> and <see cref="EdmxConfigManager"/> classes.
    /// </summary>
    /// <remarks>
    /// These tests verify the configuration model, serialization, validation, and file management
    /// functionality for EDMX database scaffolding configurations.
    /// </remarks>
    [TestClass]
    public class EdmxConfigTests
    {

        #region Fields

        private EdmxConfigManager _configManager;
        private string _tempDirectory;

        #endregion

        #region Test Setup and Cleanup

        /// <summary>
        /// Initializes test dependencies before each test method execution.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {

            _configManager = new EdmxConfigManager();
            _tempDirectory = Path.Combine(Path.GetTempPath(), "EdmxConfigTests_" + Guid.NewGuid().ToString("N")[..8]);
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

        #region EdmxConfig Model Tests

        /// <summary>
        /// Tests that <see cref="EdmxConfig"/> has correct default values.
        /// </summary>
        [TestMethod]
        public void EdmxConfig_DefaultValues_ShouldBeCorrect()
        {

            var config = new EdmxConfig();

            config.ConnectionStringSource.Should().Be(string.Empty);
            config.Provider.Should().Be("SqlServer");
            config.IncludedTables.Should().BeNull();
            config.ExcludedTables.Should().BeNull();
            config.UsePluralizer.Should().BeTrue();
            config.UseDataAnnotations.Should().BeTrue();
            config.DbContextNamespace.Should().Be(string.Empty);
            config.ObjectsNamespace.Should().Be(string.Empty);
            config.ContextName.Should().Be("GeneratedDbContext");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConfig"/> properties can be set correctly.
        /// </summary>
        [TestMethod]
        public void EdmxConfig_PropertyAssignment_ShouldWork()
        {

            var config = new EdmxConfig
            {

                ConnectionStringSource = "appsettings.json:ConnectionStrings:TestConnection",
                Provider = "PostgreSQL",
                IncludedTables = ["Users", "Orders"],
                UsePluralizer = false,
                UseDataAnnotations = false,
                DbContextNamespace = "MyApp.Data",
                ObjectsNamespace = "MyApp.Core",
                ContextName = "TestDbContext"

            };

            config.ConnectionStringSource.Should().Be("appsettings.json:ConnectionStrings:TestConnection");
            config.Provider.Should().Be("PostgreSQL");
            config.IncludedTables.Should().ContainInOrder("Users", "Orders");
            config.UsePluralizer.Should().BeFalse();
            config.UseDataAnnotations.Should().BeFalse();
            config.DbContextNamespace.Should().Be("MyApp.Data");
            config.ObjectsNamespace.Should().Be("MyApp.Core");
            config.ContextName.Should().Be("TestDbContext");

        }

        #endregion

        #region EdmxConfigManager Creation Tests

        /// <summary>
        /// Tests that <see cref="EdmxConfigManager.CreateDefaultConfig"/> creates a valid configuration.
        /// </summary>
        [TestMethod]
        public void CreateDefaultConfig_WithValidParameters_ShouldCreateCorrectConfig()
        {

            var config = _configManager.CreateDefaultConfig(
                "appsettings.json:ConnectionStrings:DefaultConnection",
                "PostgreSQL",
                "MyDbContext"
            );

            config.ConnectionStringSource.Should().Be("appsettings.json:ConnectionStrings:DefaultConnection");
            config.Provider.Should().Be("PostgreSQL");
            config.ContextName.Should().Be("MyDbContext");
            config.UsePluralizer.Should().BeTrue();
            config.UseDataAnnotations.Should().BeTrue();
            config.DbContextNamespace.Should().Be(string.Empty);
            config.IncludedTables.Should().BeNull();
            config.ExcludedTables.Should().BeNull();

        }

        /// <summary>
        /// Tests that <see cref="EdmxConfigManager.CreateDefaultConfig"/> throws exceptions for invalid parameters.
        /// </summary>
        [TestMethod]
        public void CreateDefaultConfig_WithInvalidParameters_ShouldThrowArgumentException()
        {

            var action1 = () => _configManager.CreateDefaultConfig("", "SqlServer", "MyContext");
            action1.Should().Throw<ArgumentException>()
                   .And.ParamName.Should().Be("connectionStringSource");

            var action2 = () => _configManager.CreateDefaultConfig("valid:source:key", "", "MyContext");
            action2.Should().Throw<ArgumentException>()
                   .And.ParamName.Should().Be("provider");

            var action3 = () => _configManager.CreateDefaultConfig("valid:source:key", "SqlServer", "");
            action3.Should().Throw<ArgumentException>()
                   .And.ParamName.Should().Be("contextName");

            var action4 = () => _configManager.CreateDefaultConfig(null!, "SqlServer", "MyContext");
            action4.Should().Throw<ArgumentException>()
                   .And.ParamName.Should().Be("connectionStringSource");

            var action5 = () => _configManager.CreateDefaultConfig("valid:source:key", null!, "MyContext");
            action5.Should().Throw<ArgumentException>()
                   .And.ParamName.Should().Be("provider");

            var action6 = () => _configManager.CreateDefaultConfig("valid:source:key", "SqlServer", null!);
            action6.Should().Throw<ArgumentException>()
                   .And.ParamName.Should().Be("contextName");

        }

        #endregion

        #region File Operations Tests

        /// <summary>
        /// Tests that <see cref="EdmxConfigManager.SaveConfigAsync"/> successfully saves a configuration file.
        /// </summary>
        [TestMethod]
        public async Task SaveConfigAsync_WithValidConfig_ShouldCreateFile()
        {

            var config = new EdmxConfig
            {

                ConnectionStringSource = "appsettings.json:ConnectionStrings:DefaultConnection",
                Provider = "SqlServer",
                ContextName = "TestContext",
                IncludedTables = ["Users", "Orders"],
                UsePluralizer = false,
                UseDataAnnotations = false,
                DbContextNamespace = "MyApp.Data",
                ObjectsNamespace = "MyApp.Core"

            };

            var configPath = Path.Combine(_tempDirectory, "test.edmx.config");

            await _configManager.SaveConfigAsync(config, configPath);

            File.Exists(configPath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(configPath);
            content.Should().Contain("appsettings.json:ConnectionStrings:DefaultConnection");
            content.Should().Contain("SqlServer");
            content.Should().Contain("TestContext");
            content.Should().Contain("Users");
            content.Should().Contain("Orders");
            content.Should().Contain("MyApp.Data");
            content.Should().Contain("MyApp.Core");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConfigManager.LoadConfigAsync"/> successfully loads a configuration file.
        /// </summary>
        [TestMethod]
        public async Task LoadConfigAsync_WithValidFile_ShouldLoadConfig()
        {

            var originalConfig = new EdmxConfig
            {

                ConnectionStringSource = "secrets:ConnectionStrings:TestConnection",
                Provider = "PostgreSQL",
                ContextName = "LoadTestContext",
                ExcludedTables = ["TempTable", "LogTable"],
                UsePluralizer = true,
                UseDataAnnotations = true,
                DbContextNamespace = "Test.Data",
                ObjectsNamespace = "Test.Core"

            };

            var configPath = Path.Combine(_tempDirectory, "load-test.edmx.config");
            await _configManager.SaveConfigAsync(originalConfig, configPath);

            var loadedConfig = await _configManager.LoadConfigAsync(configPath);

            loadedConfig.ConnectionStringSource.Should().Be("secrets:ConnectionStrings:TestConnection");
            loadedConfig.Provider.Should().Be("PostgreSQL");
            loadedConfig.ContextName.Should().Be("LoadTestContext");
            loadedConfig.ExcludedTables.Should().ContainInOrder("TempTable", "LogTable");
            loadedConfig.UsePluralizer.Should().BeTrue();
            loadedConfig.UseDataAnnotations.Should().BeTrue();
            loadedConfig.DbContextNamespace.Should().Be("Test.Data");
            loadedConfig.ObjectsNamespace.Should().Be("Test.Core");
            loadedConfig.IncludedTables.Should().BeNull();

        }

        /// <summary>
        /// Tests that <see cref="EdmxConfigManager.LoadConfigAsync"/> throws exception for non-existent file.
        /// </summary>
        [TestMethod]
        public async Task LoadConfigAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
        {

            var configPath = Path.Combine(_tempDirectory, "nonexistent.edmx.config");

            var action = async () => await _configManager.LoadConfigAsync(configPath);
            await action.Should().ThrowAsync<FileNotFoundException>();

        }

        /// <summary>
        /// Tests that <see cref="EdmxConfigManager.LoadConfigAsync"/> throws exception for null path.
        /// </summary>
        [TestMethod]
        public async Task LoadConfigAsync_WithNullPath_ShouldThrowArgumentException()
        {

            var action = async () => await _configManager.LoadConfigAsync(null!);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("configPath");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConfigManager.SaveConfigAsync"/> throws exception for null config.
        /// </summary>
        [TestMethod]
        public async Task SaveConfigAsync_WithNullConfig_ShouldThrowArgumentNullException()
        {

            var configPath = Path.Combine(_tempDirectory, "test.edmx.config");

            var action = async () => await _configManager.SaveConfigAsync(null!, configPath);
            await action.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("config");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConfigManager.SaveConfigAsync"/> throws exception for null path.
        /// </summary>
        [TestMethod]
        public async Task SaveConfigAsync_WithNullPath_ShouldThrowArgumentException()
        {

            var config = _configManager.CreateDefaultConfig(
                "appsettings.json:ConnectionStrings:DefaultConnection",
                "SqlServer",
                "TestContext"
            );

            var action = async () => await _configManager.SaveConfigAsync(config, null!);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("configPath");

        }

        #endregion

        #region Validation Tests

        /// <summary>
        /// Tests that configurations with both included and excluded tables are rejected.
        /// </summary>
        [TestMethod]
        public async Task SaveConfigAsync_WithBothIncludedAndExcludedTables_ShouldThrowInvalidOperationException()
        {

            var config = new EdmxConfig
            {

                ConnectionStringSource = "appsettings.json:ConnectionStrings:DefaultConnection",
                Provider = "SqlServer",
                ContextName = "TestContext",
                IncludedTables = ["Users"],
                ExcludedTables = ["Logs"]

            };

            var configPath = Path.Combine(_tempDirectory, "invalid.edmx.config");

            var action = async () => await _configManager.SaveConfigAsync(config, configPath);
            await action.Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*both IncludedTables and ExcludedTables*");

        }

        /// <summary>
        /// Tests that configurations with unsupported providers are rejected.
        /// </summary>
        [TestMethod]
        public async Task SaveConfigAsync_WithUnsupportedProvider_ShouldThrowInvalidOperationException()
        {

            var config = new EdmxConfig
            {

                ConnectionStringSource = "appsettings.json:ConnectionStrings:DefaultConnection",
                Provider = "Oracle",
                ContextName = "TestContext"

            };

            var configPath = Path.Combine(_tempDirectory, "invalid-provider.edmx.config");

            var action = async () => await _configManager.SaveConfigAsync(config, configPath);
            await action.Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*Unsupported provider: Oracle*");

        }

        /// <summary>
        /// Tests that configurations with empty required fields are rejected.
        /// </summary>
        [TestMethod]
        public async Task SaveConfigAsync_WithEmptyRequiredFields_ShouldThrowInvalidOperationException()
        {

            var config = new EdmxConfig
            {

                ConnectionStringSource = "",
                Provider = "SqlServer",
                ContextName = "TestContext"

            };

            var configPath = Path.Combine(_tempDirectory, "empty-connection.edmx.config");

            var action = async () => await _configManager.SaveConfigAsync(config, configPath);
            await action.Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*ConnectionStringSource is required*");

        }

        /// <summary>
        /// Tests that configurations with empty provider are rejected.
        /// </summary>
        [TestMethod]
        public async Task SaveConfigAsync_WithEmptyProvider_ShouldThrowInvalidOperationException()
        {

            var config = new EdmxConfig
            {

                ConnectionStringSource = "appsettings.json:ConnectionStrings:DefaultConnection",
                Provider = "",
                ContextName = "TestContext"

            };

            var configPath = Path.Combine(_tempDirectory, "empty-provider.edmx.config");

            var action = async () => await _configManager.SaveConfigAsync(config, configPath);
            await action.Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*Provider is required*");

        }

        /// <summary>
        /// Tests that configurations with empty context name are rejected.
        /// </summary>
        [TestMethod]
        public async Task SaveConfigAsync_WithEmptyContextName_ShouldThrowInvalidOperationException()
        {

            var config = new EdmxConfig
            {

                ConnectionStringSource = "appsettings.json:ConnectionStrings:DefaultConnection",
                Provider = "SqlServer",
                ContextName = ""

            };

            var configPath = Path.Combine(_tempDirectory, "empty-context.edmx.config");

            var action = async () => await _configManager.SaveConfigAsync(config, configPath);
            await action.Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("*ContextName is required*");

        }

        #endregion

        #region File Existence Tests

        /// <summary>
        /// Tests that <see cref="EdmxConfigManager.ConfigExists"/> correctly identifies existing files.
        /// </summary>
        [TestMethod]
        public async Task ConfigExists_WithExistingFile_ShouldReturnTrue()
        {

            var config = _configManager.CreateDefaultConfig(
                "appsettings.json:ConnectionStrings:DefaultConnection",
                "SqlServer",
                "TestContext"
            );

            var configPath = Path.Combine(_tempDirectory, "exists-test.edmx.config");
            await _configManager.SaveConfigAsync(config, configPath);

            _configManager.ConfigExists(configPath).Should().BeTrue();

        }

        /// <summary>
        /// Tests that <see cref="EdmxConfigManager.ConfigExists"/> correctly identifies non-existent files.
        /// </summary>
        [TestMethod]
        public void ConfigExists_WithNonExistentFile_ShouldReturnFalse()
        {

            var configPath = Path.Combine(_tempDirectory, "does-not-exist.edmx.config");

            _configManager.ConfigExists(configPath).Should().BeFalse();

        }

        /// <summary>
        /// Tests that <see cref="EdmxConfigManager.ConfigExists"/> throws exception for null path.
        /// </summary>
        [TestMethod]
        public void ConfigExists_WithNullPath_ShouldThrowArgumentException()
        {

            var action = () => _configManager.ConfigExists(null!);
            action.Should().Throw<ArgumentException>()
                  .And.ParamName.Should().Be("configPath");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConfigManager.ConfigExists"/> throws exception for empty path.
        /// </summary>
        [TestMethod]
        public void ConfigExists_WithEmptyPath_ShouldThrowArgumentException()
        {

            var action = () => _configManager.ConfigExists("");
            action.Should().Throw<ArgumentException>()
                  .And.ParamName.Should().Be("configPath");

        }

        #endregion

        #region Serialization Tests

        /// <summary>
        /// Tests that JSON serialization preserves all configuration properties correctly.
        /// </summary>
        [TestMethod]
        public async Task SaveAndLoadConfig_WithAllProperties_ShouldPreserveAllValues()
        {

            var originalConfig = new EdmxConfig
            {

                ConnectionStringSource = "environment:DATABASE:CONNECTION_STRING",
                Provider = "PostgreSQL",
                IncludedTables = ["Users", "Orders", "Products"],
                UsePluralizer = false,
                UseDataAnnotations = false,
                DbContextNamespace = "MyCompany.Data",
                ObjectsNamespace = "MyCompany.Core",
                ContextName = "ProductionDbContext"

            };

            var configPath = Path.Combine(_tempDirectory, "complete-test.edmx.config");
            await _configManager.SaveConfigAsync(originalConfig, configPath);

            var loadedConfig = await _configManager.LoadConfigAsync(configPath);

            loadedConfig.Should().BeEquivalentTo(originalConfig);

        }

        /// <summary>
        /// Tests that JSON serialization handles null collections correctly.
        /// </summary>
        [TestMethod]
        public async Task SaveAndLoadConfig_WithNullCollections_ShouldPreserveNullValues()
        {

            var originalConfig = new EdmxConfig
            {

                ConnectionStringSource = "appsettings.json:ConnectionStrings:DefaultConnection",
                Provider = "SqlServer",
                ContextName = "SimpleContext",
                IncludedTables = null,
                ExcludedTables = null

            };

            var configPath = Path.Combine(_tempDirectory, "null-collections-test.edmx.config");
            await _configManager.SaveConfigAsync(originalConfig, configPath);

            var loadedConfig = await _configManager.LoadConfigAsync(configPath);

            loadedConfig.IncludedTables.Should().BeNull();
            loadedConfig.ExcludedTables.Should().BeNull();
            loadedConfig.Should().BeEquivalentTo(originalConfig);

        }

        /// <summary>
        /// Tests that JSON serialization handles empty collections correctly.
        /// </summary>
        [TestMethod]
        public async Task SaveAndLoadConfig_WithEmptyCollections_ShouldPreserveEmptyValues()
        {

            var originalConfig = new EdmxConfig
            {

                ConnectionStringSource = "appsettings.json:ConnectionStrings:DefaultConnection",
                Provider = "SqlServer",
                ContextName = "EmptyCollectionsContext",
                IncludedTables = [],
                ExcludedTables = []

            };

            var configPath = Path.Combine(_tempDirectory, "empty-collections-test.edmx.config");
            await _configManager.SaveConfigAsync(originalConfig, configPath);

            var loadedConfig = await _configManager.LoadConfigAsync(configPath);

            loadedConfig.IncludedTables.Should().NotBeNull().And.BeEmpty();
            loadedConfig.ExcludedTables.Should().NotBeNull().And.BeEmpty();

        }

        #endregion

        #region Pluralization Override Tests

        /// <summary>
        /// Tests that pluralization overrides are correctly serialized and deserialized.
        /// </summary>
        [TestMethod]
        public async Task SaveAndLoadConfig_WithPluralizationOverrides_ShouldPreserveOverrides()
        {

            var originalConfig = new EdmxConfig
            {

                ConnectionStringSource = "appsettings.json:ConnectionStrings:DefaultConnection",
                Provider = "SqlServer",
                ContextName = "PluralizationTestContext",
                PluralizationOverrides = new Dictionary<string, string>
                {
                    { "FileMetadata", "FileMetadata" },
                    { "People", "Person" }
                }

            };

            var configPath = Path.Combine(_tempDirectory, "pluralization-overrides-test.edmx.config");
            await _configManager.SaveConfigAsync(originalConfig, configPath);

            var loadedConfig = await _configManager.LoadConfigAsync(configPath);

            loadedConfig.PluralizationOverrides.Should().NotBeNull();
            loadedConfig.PluralizationOverrides.Should().HaveCount(2);
            loadedConfig.PluralizationOverrides["FileMetadata"].Should().Be("FileMetadata");
            loadedConfig.PluralizationOverrides["People"].Should().Be("Person");
            loadedConfig.Should().BeEquivalentTo(originalConfig);

        }

        /// <summary>
        /// Tests that null pluralization overrides are correctly handled and not serialized.
        /// </summary>
        [TestMethod]
        public async Task SaveAndLoadConfig_WithNullPluralizationOverrides_ShouldPreserveNull()
        {

            var originalConfig = new EdmxConfig
            {

                ConnectionStringSource = "appsettings.json:ConnectionStrings:DefaultConnection",
                Provider = "SqlServer",
                ContextName = "NullOverridesContext",
                PluralizationOverrides = null

            };

            var configPath = Path.Combine(_tempDirectory, "null-pluralization-overrides-test.edmx.config");
            await _configManager.SaveConfigAsync(originalConfig, configPath);

            var loadedConfig = await _configManager.LoadConfigAsync(configPath);

            loadedConfig.PluralizationOverrides.Should().BeNull();

            // Verify that the pluralization overrides section is not present in the JSON
            var jsonContent = await File.ReadAllTextAsync(configPath);
            jsonContent.Should().NotContain("pluralizationOverrides");

        }

        /// <summary>
        /// Tests that empty pluralization overrides dictionary is correctly serialized and deserialized.
        /// </summary>
        [TestMethod]
        public async Task SaveAndLoadConfig_WithEmptyPluralizationOverrides_ShouldPreserveEmpty()
        {

            var originalConfig = new EdmxConfig
            {

                ConnectionStringSource = "appsettings.json:ConnectionStrings:DefaultConnection",
                Provider = "SqlServer",
                ContextName = "EmptyOverridesContext",
                PluralizationOverrides = new Dictionary<string, string>()

            };

            var configPath = Path.Combine(_tempDirectory, "empty-pluralization-overrides-test.edmx.config");
            await _configManager.SaveConfigAsync(originalConfig, configPath);

            var loadedConfig = await _configManager.LoadConfigAsync(configPath);

            loadedConfig.PluralizationOverrides.Should().NotBeNull().And.BeEmpty();
            loadedConfig.Should().BeEquivalentTo(originalConfig);

        }

        /// <summary>
        /// Tests that complex pluralization override scenarios are handled correctly.
        /// </summary>
        [TestMethod]
        public async Task SaveAndLoadConfig_WithComplexPluralizationOverrides_ShouldHandleAllCases()
        {

            var originalConfig = new EdmxConfig
            {

                ConnectionStringSource = "secrets:ConnectionStrings:TestConnection",
                Provider = "PostgreSQL",
                ContextName = "ComplexOverridesContext",
                PluralizationOverrides = new Dictionary<string, string>
                {
                    { "FileMetadata", "FileMetadata" },        // Prevent incorrect pluralization
                    { "People", "Person" },                    // Override correct but unwanted pluralization  
                    { "UserData", "UserInfo" },                // Complete name change
                    { "Categories", "Category" },              // Standard case
                    { "EventLogs", "EventLog" }                // Multiple word case
                },
                IncludedTables = ["FileMetadata", "People", "UserData"],
                UsePluralizer = true,
                UseDataAnnotations = false

            };

            var configPath = Path.Combine(_tempDirectory, "complex-pluralization-overrides-test.edmx.config");
            await _configManager.SaveConfigAsync(originalConfig, configPath);

            var loadedConfig = await _configManager.LoadConfigAsync(configPath);

            loadedConfig.PluralizationOverrides.Should().NotBeNull().And.HaveCount(5);
            loadedConfig.PluralizationOverrides["FileMetadata"].Should().Be("FileMetadata");
            loadedConfig.PluralizationOverrides["People"].Should().Be("Person");
            loadedConfig.PluralizationOverrides["UserData"].Should().Be("UserInfo");
            loadedConfig.PluralizationOverrides["Categories"].Should().Be("Category");
            loadedConfig.PluralizationOverrides["EventLogs"].Should().Be("EventLog");
            loadedConfig.Should().BeEquivalentTo(originalConfig);

        }

        #endregion

    }

}
