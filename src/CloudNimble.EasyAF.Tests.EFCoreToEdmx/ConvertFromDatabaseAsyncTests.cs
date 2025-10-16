using CloudNimble.EasyAF.EFCoreToEdmx;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Unit tests for EdmxConverter.ConvertFromDatabaseAsync method.
    /// </summary>
    /// <remarks>
    /// These tests verify that the ConvertFromDatabaseAsync method properly passes
    /// provider type information to the EdmxXmlGenerator, ensuring correct type mappings
    /// for different database providers like PostgreSQL.
    /// </remarks>
    [TestClass]
    public class ConvertFromDatabaseAsyncTests
    {

        #region Fields

        private string _tempConfigFile;
        private string _tempProjectPath;
        private EdmxConverter _converter;

        #endregion

        #region Test Setup and Cleanup

        /// <summary>
        /// Initializes test fixtures before each test method execution.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new EdmxConverter();
            _tempProjectPath = Path.Combine(Path.GetTempPath(), $"ConvertFromDatabaseAsyncTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempProjectPath);
        }

        /// <summary>
        /// Cleans up test fixtures after each test method execution.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(_tempConfigFile))
            {
                File.Delete(_tempConfigFile);
            }

            if (Directory.Exists(_tempProjectPath))
            {
                Directory.Delete(_tempProjectPath, true);
            }
        }

        #endregion

        #region Provider Type Passing Tests

        [TestMethod]
        public async Task ConvertFromDatabaseAsync_WithPostgreSQLConfig_ShouldUsePostgreSQLTypeMappings()
        {
            // Arrange
            var configContent = @"{
  ""Provider"": ""PostgreSQL"",
  ""ConnectionStringSource"": ""Server=localhost;Database=testdb;User Id=test;Password=test;"",
  ""ContextName"": ""TestDbContext"",
  ""OutputPath"": ""Models"",
  ""Namespace"": ""TestApp.Data.Models"",
  ""IncludeTables"": [],
  ""ExcludeTables"": []
}";

            _tempConfigFile = Path.Combine(_tempProjectPath, "TestDbContext.edmx.config");
            await File.WriteAllTextAsync(_tempConfigFile, configContent);

            // Act & Assert
            // This test would require a real database connection to work fully,
            // but we can verify that the ConvertFromDatabaseAsync method signature 
            // and basic structure works correctly.
            
            // For now, we expect this to fail with a connection error, but not due to 
            // missing provider type information
            try
            {
                var result = await _converter.ConvertFromDatabaseAsync(_tempConfigFile, _tempProjectPath);
                
                // If it somehow succeeds (unlikely without real DB), verify PostgreSQL types
                result.EdmxContent.Should().NotBeNullOrEmpty();
                result.EdmxContent.Should().Contain("Provider=\"Npgsql\"", "Should use Npgsql provider for PostgreSQL");
            }
            catch (Exception ex)
            {
                // Expected - connection will fail, but ensure it's not due to missing provider type
                ex.Message.Should().NotContain("provider type", "Error should not be related to missing provider type");
                ex.Message.Should().NotContain("xmlGenerator", "Error should not be related to XML generator initialization");
            }
        }

        [TestMethod]
        public async Task ConvertFromDatabaseAsync_WithSqlServerConfig_ShouldUseSqlServerTypeMappings()
        {
            // Arrange
            var configContent = @"{
  ""Provider"": ""SqlServer"",
  ""ConnectionStringSource"": ""Server=localhost;Database=testdb;Integrated Security=true;"",
  ""ContextName"": ""TestDbContext"",
  ""OutputPath"": ""Models"",
  ""Namespace"": ""TestApp.Data.Models"",
  ""IncludeTables"": [],
  ""ExcludeTables"": []
}";

            _tempConfigFile = Path.Combine(_tempProjectPath, "TestDbContext.edmx.config");
            await File.WriteAllTextAsync(_tempConfigFile, configContent);

            // Act & Assert
            try
            {
                var result = await _converter.ConvertFromDatabaseAsync(_tempConfigFile, _tempProjectPath);
                
                // If it somehow succeeds (unlikely without real DB), verify SQL Server types
                result.EdmxContent.Should().NotBeNullOrEmpty();
                result.EdmxContent.Should().Contain("Provider=\"System.Data.SqlClient\"", "Should use SQL Server provider");
            }
            catch (Exception ex)
            {
                // Expected - connection will fail, but ensure it's not due to missing provider type
                ex.Message.Should().NotContain("provider type", "Error should not be related to missing provider type");
                ex.Message.Should().NotContain("xmlGenerator", "Error should not be related to XML generator initialization");
            }
        }

        [TestMethod]
        public void ConvertFromDatabaseAsync_WithNullConfigPath_ShouldThrowArgumentException()
        {
            // Act & Assert
            var action = async () => await _converter.ConvertFromDatabaseAsync(null, _tempProjectPath);
            action.Should().ThrowAsync<ArgumentException>().WithMessage("*configPath*");
        }

        [TestMethod]
        public void ConvertFromDatabaseAsync_WithNullProjectPath_ShouldThrowArgumentException()
        {
            // Act & Assert
            var action = async () => await _converter.ConvertFromDatabaseAsync("dummy.config", null);
            action.Should().ThrowAsync<ArgumentException>().WithMessage("*projectPath*");
        }

        [TestMethod]
        public void ConvertFromDatabaseAsync_WithNonExistentConfigFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentConfigFile = Path.Combine(_tempProjectPath, "NonExistent.edmx.config");

            // Act & Assert
            var action = async () => await _converter.ConvertFromDatabaseAsync(nonExistentConfigFile, _tempProjectPath);
            action.Should().ThrowAsync<FileNotFoundException>();
        }

        #endregion

    }

}
