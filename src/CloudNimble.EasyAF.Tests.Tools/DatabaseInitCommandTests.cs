using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.Tools.Commands;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.Tools
{

    /// <summary>
    /// Unit tests for the <see cref="DatabaseInitCommand"/> class.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class DatabaseInitCommandTests
    {

        #region Properties

        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        #endregion

        #region Test Setup

        /// <summary>
        /// Creates a temporary directory structure for testing.
        /// </summary>
        /// <returns>Path to the created temporary directory.</returns>
        private static string CreateTempSolutionStructure()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"EasyAF_Test_{Guid.NewGuid():N}");
            var dataProjectDir = Path.Combine(tempDir, "TestProject.Data");
            
            Directory.CreateDirectory(dataProjectDir);
            
            // Create a minimal .csproj file
            var projectContent = """
                <Project Sdk="Microsoft.NET.Sdk">
                    <PropertyGroup>
                        <TargetFramework>net8.0</TargetFramework>
                    </PropertyGroup>
                </Project>
                """;
            
            File.WriteAllText(Path.Combine(dataProjectDir, "TestProject.Data.csproj"), projectContent);
            
            return tempDir;
        }

        /// <summary>
        /// Creates a temporary directory structure with user secrets already initialized.
        /// </summary>
        /// <returns>Tuple containing the solution directory and user secrets ID.</returns>
        private static (string solutionDir, string userSecretsId) CreateTempSolutionWithUserSecrets()
        {
            var tempDir = CreateTempSolutionStructure();
            var dataProjectDir = Path.Combine(tempDir, "TestProject.Data");
            var userSecretsId = Guid.NewGuid().ToString();
            
            // Create project file with UserSecretsId
            var projectContent = $"""
                <Project Sdk="Microsoft.NET.Sdk">
                    <PropertyGroup>
                        <TargetFramework>net8.0</TargetFramework>
                        <UserSecretsId>{userSecretsId}</UserSecretsId>
                    </PropertyGroup>
                </Project>
                """;
            
            File.WriteAllText(Path.Combine(dataProjectDir, "TestProject.Data.csproj"), projectContent);
            
            return (tempDir, userSecretsId);
        }

        /// <summary>
        /// Cleans up temporary directory after test.
        /// </summary>
        /// <param name="tempDir">Temporary directory to clean up.</param>
        private static void CleanupTempDirectory(string tempDir)
        {
            if (Directory.Exists(tempDir))
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch
                {
                    // Best effort cleanup
                }
            }
        }

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullConfigManager_ShouldThrowArgumentNullException()
        {
            Action act = () => new DatabaseInitCommand(null);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("configManager");
        }

        [TestMethod]
        public void Constructor_WithValidConfigManager_ShouldNotThrow()
        {
            var configManager = new EdmxConfigManager();

            Action act = () => new DatabaseInitCommand(configManager);

            act.Should().NotThrow();
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void Properties_ShouldHaveExpectedDefaults()
        {
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager);

            command.ConnectionString.Should().Be(string.Empty);
            command.ContextName.Should().Be(string.Empty);
            command.Provider.Should().Be(string.Empty);
            command.SolutionFolder.Should().Be(Directory.GetCurrentDirectory());
            command.DbContextNamespace.Should().BeNull();
            command.ObjectsNamespace.Should().BeNull();
            command.ExcludeTables.Should().BeNull();
            command.Tables.Should().BeNull();
            command.NoDataAnnotations.Should().BeFalse();
            command.NoPluralize.Should().BeFalse();
        }

        #endregion

        #region OnExecuteAsync Tests

        [TestMethod]
        public async Task OnExecuteAsync_WithBothTablesAndExcludeTablesSpecified_ShouldReturnErrorCode()
        {
            var tempDir = CreateTempSolutionStructure();
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = "Server=test;Database=test;",
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir,
                Tables = ["Table1", "Table2"],
                ExcludeTables = ["Table3", "Table4"]
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(1);
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public async Task OnExecuteAsync_WithNonExistentDataFolder_ShouldReturnErrorCode()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"EasyAF_Test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);
            
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = "appsettings.json:ConnectionStrings:DefaultConnection",
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(1);
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public async Task OnExecuteAsync_WithValidConnectionStringSource_ShouldSucceed()
        {
            var tempDir = CreateTempSolutionStructure();
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = "appsettings.json:ConnectionStrings:DefaultConnection",
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(0);
                
                // Verify config file was created
                var configPath = Path.Combine(tempDir, "TestProject.Data", "TestContext.edmx.config");
                File.Exists(configPath).Should().BeTrue();
                
                // Verify config content
                var configContent = await File.ReadAllTextAsync(configPath);
                configContent.Should().Contain("appsettings.json:ConnectionStrings:DefaultConnection");
                configContent.Should().Contain("TestContext");
                configContent.Should().Contain("SqlServer");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public async Task OnExecuteAsync_WithCustomNamespaces_ShouldUseProvidedNamespaces()
        {
            var tempDir = CreateTempSolutionStructure();
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = "appsettings.json:ConnectionStrings:DefaultConnection",
                ContextName = "TestContext",
                Provider = "PostgreSQL",
                SolutionFolder = tempDir,
                DbContextNamespace = "Custom.Data.Namespace",
                ObjectsNamespace = "Custom.Core.Namespace"
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(0);
                
                var configPath = Path.Combine(tempDir, "TestProject.Data", "TestContext.edmx.config");
                var configContent = await File.ReadAllTextAsync(configPath);
                configContent.Should().Contain("Custom.Data.Namespace");
                configContent.Should().Contain("Custom.Core.Namespace");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public async Task OnExecuteAsync_WithIncludedTables_ShouldConfigureIncludedTables()
        {
            var tempDir = CreateTempSolutionStructure();
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = "appsettings.json:ConnectionStrings:DefaultConnection",
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir,
                Tables = ["Users", "Products", "Orders"]
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(0);
                
                var configPath = Path.Combine(tempDir, "TestProject.Data", "TestContext.edmx.config");
                var configContent = await File.ReadAllTextAsync(configPath);
                configContent.Should().Contain("Users");
                configContent.Should().Contain("Products");
                configContent.Should().Contain("Orders");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public async Task OnExecuteAsync_WithExcludedTables_ShouldConfigureExcludedTables()
        {
            var tempDir = CreateTempSolutionStructure();
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = "appsettings.json:ConnectionStrings:DefaultConnection",
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir,
                ExcludeTables = ["__MigrationHistory", "AspNetRoles"]
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(0);
                
                var configPath = Path.Combine(tempDir, "TestProject.Data", "TestContext.edmx.config");
                var configContent = await File.ReadAllTextAsync(configPath);
                configContent.Should().Contain("__MigrationHistory");
                configContent.Should().Contain("AspNetRoles");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public async Task OnExecuteAsync_WithNoDataAnnotations_ShouldDisableDataAnnotations()
        {
            var tempDir = CreateTempSolutionStructure();
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = "appsettings.json:ConnectionStrings:DefaultConnection",
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir,
                NoDataAnnotations = true
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(0);
                
                var configPath = Path.Combine(tempDir, "TestProject.Data", "TestContext.edmx.config");
                var configContent = await File.ReadAllTextAsync(configPath);
                configContent.Should().Contain("\"useDataAnnotations\": false");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public async Task OnExecuteAsync_WithNoPluralize_ShouldDisablePluralization()
        {
            var tempDir = CreateTempSolutionStructure();
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = "appsettings.json:ConnectionStrings:DefaultConnection",
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir,
                NoPluralize = true
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(0);
                
                var configPath = Path.Combine(tempDir, "TestProject.Data", "TestContext.edmx.config");
                var configContent = await File.ReadAllTextAsync(configPath);
                configContent.Should().Contain("\"usePluralizer\": false");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        #endregion

        #region User Secrets Integration Tests

        [TestMethod]
        public async Task OnExecuteAsync_WithActualConnectionString_ShouldInitializeUserSecretsAndStoreConnectionString()
        {
            var tempDir = CreateTempSolutionStructure();
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = "Server=localhost;Database=TestDb;User Id=testuser;Password=testpass123;",
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(0);
                
                // Verify UserSecretsId was added to project file
                var projectPath = Path.Combine(tempDir, "TestProject.Data", "TestProject.Data.csproj");
                var projectContent = await File.ReadAllTextAsync(projectPath);
                projectContent.Should().Contain("<UserSecretsId>");
                
                // Verify config file references user secrets
                var configPath = Path.Combine(tempDir, "TestProject.Data", "TestContext.edmx.config");
                var configContent = await File.ReadAllTextAsync(configPath);
                configContent.Should().Contain("secrets:ConnectionStrings:TestContextConnection");
                
                // The actual user secrets validation would require the dotnet CLI to be available
                // In a real environment, we'd verify using: dotnet user-secrets list --project [projectPath]
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public async Task OnExecuteAsync_WithExistingUserSecretsId_ShouldReuseExistingId()
        {
            var (tempDir, existingUserSecretsId) = CreateTempSolutionWithUserSecrets();
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = "Server=localhost;Database=TestDb;User Id=testuser;Password=testpass123;",
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(0);
                
                // Verify the existing UserSecretsId is preserved
                var projectPath = Path.Combine(tempDir, "TestProject.Data", "TestProject.Data.csproj");
                var projectContent = await File.ReadAllTextAsync(projectPath);
                projectContent.Should().Contain($"<UserSecretsId>{existingUserSecretsId}</UserSecretsId>");
                
                // Verify config file references user secrets
                var configPath = Path.Combine(tempDir, "TestProject.Data", "TestContext.edmx.config");
                var configContent = await File.ReadAllTextAsync(configPath);
                configContent.Should().Contain("secrets:ConnectionStrings:TestContextConnection");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public async Task OnExecuteAsync_WhenUserSecretsFailsToStore_ShouldFallBackToConnectionStringAsIs()
        {
            var tempDir = CreateTempSolutionStructure();
            
            // Create project file without proper structure to cause user secrets initialization to fail
            var projectPath = Path.Combine(tempDir, "TestProject.Data", "TestProject.Data.csproj");
            File.WriteAllText(projectPath, "<Project></Project>"); // Invalid project structure
            
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = "Server=localhost;Database=TestDb;User Id=testuser;Password=testpass123;",
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(0);
                
                // Verify it falls back to using the connection string directly
                var configPath = Path.Combine(tempDir, "TestProject.Data", "TestContext.edmx.config");
                var configContent = await File.ReadAllTextAsync(configPath);
                configContent.Should().Contain("Server=localhost;Database=TestDb;User Id=testuser;Password=testpass123;");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        #endregion

        #region Connection String Source Detection Tests

        [TestMethod]
        [DataRow("appsettings.json:ConnectionStrings:DefaultConnection")]
        [DataRow("appsettings.Development.json:ConnectionStrings:DefaultConnection")]
        [DataRow("secrets:ConnectionStrings:DefaultConnection")]
        [DataRow("user-secrets:ConnectionStrings:DefaultConnection")]
        [DataRow("environment:ConnectionStrings:DefaultConnection")]
        public async Task OnExecuteAsync_WithKnownConnectionStringSources_ShouldUseSourceDirectly(string connectionStringSource)
        {
            var tempDir = CreateTempSolutionStructure();
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = connectionStringSource,
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(0);
                
                var configPath = Path.Combine(tempDir, "TestProject.Data", "TestContext.edmx.config");
                var configContent = await File.ReadAllTextAsync(configPath);
                configContent.Should().Contain(connectionStringSource);
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        [DataRow("Server=localhost;Database=test;Integrated Security=true;")]
        [DataRow("Host=localhost;Database=test;Username=user;Password=pass;")]
        [DataRow("Data Source=test.db;")]
        public async Task OnExecuteAsync_WithActualConnectionStrings_ShouldStoreInUserSecrets(string actualConnectionString)
        {
            var tempDir = CreateTempSolutionStructure();
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = actualConnectionString,
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(0);
                
                var configPath = Path.Combine(tempDir, "TestProject.Data", "TestContext.edmx.config");
                var configContent = await File.ReadAllTextAsync(configPath);
                
                // Should reference user secrets, not contain the actual connection string
                configContent.Should().Contain("secrets:ConnectionStrings:TestContextConnection");
                configContent.Should().NotContain(actualConnectionString);
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        #endregion

        #region Namespace Auto-Detection Tests

        [TestMethod]
        public async Task OnExecuteAsync_WithDataProjectEndingInData_ShouldAutoDetectNamespaces()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"EasyAF_Test_{Guid.NewGuid():N}");
            var dataProjectDir = Path.Combine(tempDir, "MyCompany.MyProject.Data");
            
            Directory.CreateDirectory(dataProjectDir);
            
            var projectContent = """
                <Project Sdk="Microsoft.NET.Sdk">
                    <PropertyGroup>
                        <TargetFramework>net8.0</TargetFramework>
                    </PropertyGroup>
                </Project>
                """;
            
            File.WriteAllText(Path.Combine(dataProjectDir, "MyCompany.MyProject.Data.csproj"), projectContent);
            
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = "appsettings.json:ConnectionStrings:DefaultConnection",
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(0);
                
                var configPath = Path.Combine(dataProjectDir, "TestContext.edmx.config");
                var configContent = await File.ReadAllTextAsync(configPath);
                configContent.Should().Contain("MyCompany.MyProject.Data"); // DbContext namespace
                configContent.Should().Contain("MyCompany.MyProject.Core"); // Objects namespace
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public async Task OnExecuteAsync_WithDataProjectNotEndingInData_ShouldReturnErrorCode()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"EasyAF_Test_{Guid.NewGuid():N}");
            var dataProjectDir = Path.Combine(tempDir, "MyProject.DataLayer");
            
            Directory.CreateDirectory(dataProjectDir);
            
            var projectContent = """
                <Project Sdk="Microsoft.NET.Sdk">
                    <PropertyGroup>
                        <TargetFramework>net8.0</TargetFramework>
                    </PropertyGroup>
                </Project>
                """;
            
            File.WriteAllText(Path.Combine(dataProjectDir, "MyProject.DataLayer.csproj"), projectContent);
            
            var configManager = new EdmxConfigManager();
            var command = new DatabaseInitCommand(configManager)
            {
                ConnectionString = "appsettings.json:ConnectionStrings:DefaultConnection",
                ContextName = "TestContext",
                Provider = "SqlServer",
                SolutionFolder = tempDir
            };

            try
            {
                var result = await command.OnExecuteAsync();

                // Should return error code because FindDataFolder won't find "DataLayer" (must end with ".Data")
                result.Should().Be(1);
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        #endregion

    }

}
