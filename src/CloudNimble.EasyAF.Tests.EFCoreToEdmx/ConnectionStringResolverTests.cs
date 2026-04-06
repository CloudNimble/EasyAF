using CloudNimble.EasyAF.EFCoreToEdmx;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Contains unit tests for the <see cref="ConnectionStringResolver"/> class.
    /// </summary>
    /// <remarks>
    /// These tests verify the connection string resolution functionality from various sources
    /// including JSON configuration files, environment variables, and user secrets.
    /// </remarks>
    [TestClass]
    [DoNotParallelize]
    public class ConnectionStringResolverTests
    {

        #region Fields

        private ConnectionStringResolver _resolver;
        private string _tempDirectory;

        #endregion

        #region Test Setup and Cleanup

        /// <summary>
        /// Initializes test dependencies before each test method execution.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {

            _resolver = new ConnectionStringResolver();
            _tempDirectory = Path.Combine(Path.GetTempPath(), "ConnectionStringResolverTests_" + Guid.NewGuid().ToString("N")[..8]);
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

        #region Input Validation Tests

        /// <summary>
        /// Tests that <see cref="ConnectionStringResolver.ResolveConnectionString"/> throws exception for invalid source format.
        /// </summary>
        [TestMethod]
        public void ResolveConnectionString_WithInvalidSourceFormat_ShouldThrowArgumentException()
        {

            var action1 = () => _resolver.ResolveConnectionString("invalid-format", _tempDirectory);
            action1.Should().Throw<ArgumentException>()
                   .WithMessage("*filename:section:key*");

            var action2 = () => _resolver.ResolveConnectionString("only:two", _tempDirectory);
            action2.Should().Throw<ArgumentException>()
                   .WithMessage("*filename:section:key*");

        }

        /// <summary>
        /// Tests that <see cref="ConnectionStringResolver.ResolveConnectionString"/> throws exception for null or empty parameters.
        /// </summary>
        [TestMethod]
        public void ResolveConnectionString_WithNullOrEmptyParameters_ShouldThrowArgumentException()
        {

            var action1 = () => _resolver.ResolveConnectionString("", _tempDirectory);
            action1.Should().Throw<ArgumentException>()
                   .And.ParamName.Should().Be("connectionStringSource");

            var action2 = () => _resolver.ResolveConnectionString("valid:source:key", "");
            action2.Should().Throw<ArgumentException>()
                   .And.ParamName.Should().Be("projectPath");

            var action3 = () => _resolver.ResolveConnectionString(null!, _tempDirectory);
            action3.Should().Throw<ArgumentException>()
                   .And.ParamName.Should().Be("connectionStringSource");

            var action4 = () => _resolver.ResolveConnectionString("valid:source:key", null!);
            action4.Should().Throw<ArgumentException>()
                   .And.ParamName.Should().Be("projectPath");

        }

        #endregion

        #region JSON Configuration Tests

        /// <summary>
        /// Tests that connection strings can be resolved from JSON configuration files.
        /// </summary>
        [TestMethod]
        public void ResolveConnectionString_FromJsonFile_ShouldReturnConnectionString()
        {

            var appsettingsContent = """
            {
              "ConnectionStrings": {
                "DefaultConnection": "Server=localhost;Database=TestDb;Trusted_Connection=true;",
                "SecondaryConnection": "Server=remote;Database=TestDb2;User=test;Password=secret;"
              },
              "Logging": {
                "LogLevel": {
                  "Default": "Information"
                }
              }
            }
            """;

            var appsettingsPath = Path.Combine(_tempDirectory, "appsettings.json");
            File.WriteAllText(appsettingsPath, appsettingsContent);

            var connectionString = _resolver.ResolveConnectionString(
                "appsettings.json:ConnectionStrings:DefaultConnection",
                _tempDirectory
            );

            connectionString.Should().Be("Server=localhost;Database=TestDb;Trusted_Connection=true;");

        }

        /// <summary>
        /// Tests that connection strings can be resolved from nested JSON configuration sections.
        /// </summary>
        [TestMethod]
        public void ResolveConnectionString_FromNestedJsonSection_ShouldReturnConnectionString()
        {

            var configContent = """
            {
              "Database": {
                "Primary": {
                  "ConnectionString": "Data Source=primary.db"
                },
                "Secondary": {
                  "ConnectionString": "Data Source=secondary.db"
                }
              }
            }
            """;

            var configPath = Path.Combine(_tempDirectory, "database.json");
            File.WriteAllText(configPath, configContent);

            var connectionString = _resolver.ResolveConnectionString(
                "database.json:Database:Primary:ConnectionString",
                _tempDirectory
            );

            connectionString.Should().Be("Data Source=primary.db");

        }

        /// <summary>
        /// Tests that <see cref="ConnectionStringResolver.ResolveConnectionString"/> throws exception when JSON file is not found.
        /// </summary>
        [TestMethod]
        public void ResolveConnectionString_FromNonExistentJsonFile_ShouldThrowFileNotFoundException()
        {

            var action = () => _resolver.ResolveConnectionString(
                "nonexistent.json:ConnectionStrings:DefaultConnection",
                _tempDirectory
            );

            action.Should().Throw<FileNotFoundException>()
                  .WithMessage("*nonexistent.json*");

        }

        /// <summary>
        /// Tests that <see cref="ConnectionStringResolver.ResolveConnectionString"/> throws exception when connection string is not found in JSON.
        /// </summary>
        [TestMethod]
        public void ResolveConnectionString_FromJsonWithMissingKey_ShouldThrowInvalidOperationException()
        {

            var appsettingsContent = """
            {
              "ConnectionStrings": {
                "DefaultConnection": "Server=localhost;Database=TestDb;Trusted_Connection=true;"
              }
            }
            """;

            var appsettingsPath = Path.Combine(_tempDirectory, "appsettings.json");
            File.WriteAllText(appsettingsPath, appsettingsContent);

            var action = () => _resolver.ResolveConnectionString(
                "appsettings.json:ConnectionStrings:MissingConnection",
                _tempDirectory
            );

            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("*Connection string not found*MissingConnection*");

        }

        #endregion

        #region Environment Variable Tests

        /// <summary>
        /// Tests that connection strings can be resolved from environment variables using double underscore format.
        /// </summary>
        [TestMethod]
        public void ResolveConnectionString_FromEnvironmentDoubleUnderscore_ShouldReturnConnectionString()
        {

            var envVarName = "ConnectionStrings__TestConnection";
            var connectionString = "Server=env-server;Database=EnvDb;";

            Environment.SetEnvironmentVariable(envVarName, connectionString);

            try
            {

                var result = _resolver.ResolveConnectionString(
                    "environment:ConnectionStrings:TestConnection",
                    _tempDirectory
                );

                result.Should().Be(connectionString);

            }
            finally
            {

                Environment.SetEnvironmentVariable(envVarName, null);

            }

        }

        /// <summary>
        /// Tests that connection strings can be resolved from environment variables using single underscore format.
        /// </summary>
        [TestMethod]
        public void ResolveConnectionString_FromEnvironmentSingleUnderscore_ShouldReturnConnectionString()
        {

            var envVarName = "DATABASE_CONNECTION";
            var connectionString = "Server=single-env-server;Database=EnvDb2;";

            Environment.SetEnvironmentVariable(envVarName, connectionString);

            try
            {

                var result = _resolver.ResolveConnectionString(
                    "environment:DATABASE:CONNECTION",
                    _tempDirectory
                );

                result.Should().Be(connectionString);

            }
            finally
            {

                Environment.SetEnvironmentVariable(envVarName, null);

            }

        }

        /// <summary>
        /// Tests that <see cref="ConnectionStringResolver.ResolveConnectionString"/> throws exception when environment variable is not found.
        /// </summary>
        [TestMethod]
        public void ResolveConnectionString_FromMissingEnvironmentVariable_ShouldThrowInvalidOperationException()
        {

            var action = () => _resolver.ResolveConnectionString(
                "environment:Missing:Variable",
                _tempDirectory
            );

            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("*Connection string not found in environment variables*");

        }

        #endregion

        #region User Secrets Tests

        /// <summary>
        /// Tests that user secrets resolution throws appropriate exception when not configured.
        /// </summary>
        [TestMethod]
        public void ResolveConnectionString_FromUserSecretsWithoutConfig_ShouldThrowInvalidOperationException()
        {

            // Create a project file without UserSecretsId
            var projectContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net9.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """;

            var projectPath = Path.Combine(_tempDirectory, "TestProject.csproj");
            File.WriteAllText(projectPath, projectContent);

            var action = () => _resolver.ResolveConnectionString(
                "secrets:ConnectionStrings:DefaultConnection",
                _tempDirectory
            );

            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("*UserSecretsId not found*");

        }

        /// <summary>
        /// Tests that <see cref="ConnectionStringResolver.ResolveConnectionString"/> throws exception when no project file exists.
        /// </summary>
        [TestMethod]
        public void ResolveConnectionString_FromUserSecretsWithoutProjectFile_ShouldThrowInvalidOperationException()
        {

            var action = () => _resolver.ResolveConnectionString(
                "user-secrets:ConnectionStrings:DefaultConnection",
                _tempDirectory
            );

            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("*No .csproj file found*");

        }

        /// <summary>
        /// Tests that both "secrets" and "user-secrets" source identifiers work for user secrets.
        /// </summary>
        [TestMethod]
        public void ResolveConnectionString_WithDifferentUserSecretsIdentifiers_ShouldBehaveConsistently()
        {

            var projectContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net9.0</TargetFramework>
                <UserSecretsId>12345678-1234-1234-1234-123456789012</UserSecretsId>
              </PropertyGroup>
            </Project>
            """;

            var projectPath = Path.Combine(_tempDirectory, "TestProject.csproj");
            File.WriteAllText(projectPath, projectContent);

            // Both should fail the same way since we don't have actual user secrets configured
            var action1 = () => _resolver.ResolveConnectionString(
                "secrets:ConnectionStrings:DefaultConnection",
                _tempDirectory
            );

            var action2 = () => _resolver.ResolveConnectionString(
                "user-secrets:ConnectionStrings:DefaultConnection",
                _tempDirectory
            );

            action1.Should().Throw<InvalidOperationException>();
            action2.Should().Throw<InvalidOperationException>();

        }

        #endregion

    }

}
