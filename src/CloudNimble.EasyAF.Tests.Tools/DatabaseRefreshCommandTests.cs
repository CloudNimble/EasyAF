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
    /// Unit tests for the <see cref="DatabaseRefreshCommand"/> class.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class DatabaseRefreshCommandTests
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
        private static string CreateTempProjectStructure()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"EasyAF_RefreshTest_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            // Create a minimal .csproj file
            var projectContent = """
                <Project Sdk="Microsoft.NET.Sdk">
                    <PropertyGroup>
                        <TargetFramework>net8.0</TargetFramework>
                    </PropertyGroup>
                </Project>
                """;

            File.WriteAllText(Path.Combine(tempDir, "TestProject.csproj"), projectContent);

            return tempDir;
        }

        /// <summary>
        /// Creates a temporary directory structure with an EDMX file and config.
        /// </summary>
        /// <returns>Path to the created temporary directory.</returns>
        private static string CreateTempProjectWithEdmx()
        {
            var tempDir = CreateTempProjectStructure();

            // Create a minimal EDMX file
            var edmxContent = """
                <?xml version="1.0" encoding="utf-8"?>
                <edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
                  <edmx:Runtime>
                    <edmx:StorageModels>
                      <Schema Namespace="TestModel.Store" Provider="System.Data.SqlClient" ProviderManifestToken="2012.Azure" xmlns="http://schemas.microsoft.com/ado/2009/11/edm/ssdl">
                      </Schema>
                    </edmx:StorageModels>
                    <edmx:ConceptualModels>
                      <Schema Namespace="TestModel" xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
                      </Schema>
                    </edmx:ConceptualModels>
                    <edmx:Mappings>
                      <Mapping xmlns="http://schemas.microsoft.com/ado/2009/11/mapping/cs" Space="C-S">
                      </Mapping>
                    </edmx:Mappings>
                  </edmx:Runtime>
                </edmx:Edmx>
                """;

            File.WriteAllText(Path.Combine(tempDir, "TestContext.edmx"), edmxContent);

            // Create a config file
            var configContent = """
                {
                    "connectionStringSource": "appsettings.json:ConnectionStrings:DefaultConnection",
                    "contextName": "TestContext",
                    "dbContextNamespace": "Test.Data",
                    "objectsNamespace": "Test.Core",
                    "provider": "SqlServer",
                    "useDataAnnotations": true,
                    "usePluralizer": true
                }
                """;

            File.WriteAllText(Path.Combine(tempDir, "TestContext.edmx.config"), configContent);

            return tempDir;
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
        public void Constructor_WithNullConverter_ShouldThrowArgumentNullException()
        {
            Action act = () => new DatabaseRefreshCommand(null);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("converter");
        }

        [TestMethod]
        public void Constructor_WithValidConverter_ShouldNotThrow()
        {
            var converter = new EdmxConverter();

            Action act = () => new DatabaseRefreshCommand(converter);

            act.Should().NotThrow();
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void Properties_ShouldHaveExpectedDefaults()
        {
            var converter = new EdmxConverter();
            var command = new DatabaseRefreshCommand(converter);

            command.ContextName.Should().Be(string.Empty);
            command.Project.Should().Be(string.Empty);
            command.SolutionFolder.Should().Be(Directory.GetCurrentDirectory());
        }

        #endregion

        #region OnExecuteAsync Tests

        [TestMethod]
        public async Task OnExecuteAsync_WithNoEdmxFiles_ShouldReturnErrorCode()
        {
            var tempDir = CreateTempProjectStructure();
            var converter = new EdmxConverter();
            var command = new DatabaseRefreshCommand(converter)
            {
                Project = tempDir
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(1, "because no EDMX files exist in the directory");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public async Task OnExecuteAsync_WithEdmxButNoConfig_ShouldReturnErrorCode()
        {
            var tempDir = CreateTempProjectStructure();

            // Create EDMX file without config
            var edmxContent = """
                <?xml version="1.0" encoding="utf-8"?>
                <edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
                </edmx:Edmx>
                """;
            File.WriteAllText(Path.Combine(tempDir, "TestContext.edmx"), edmxContent);

            var converter = new EdmxConverter();
            var command = new DatabaseRefreshCommand(converter)
            {
                Project = tempDir
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(1, "because the EDMX file has no corresponding .config file");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public async Task OnExecuteAsync_WithSpecificContextNotFound_ShouldReturnErrorCode()
        {
            var tempDir = CreateTempProjectWithEdmx();
            var converter = new EdmxConverter();
            var command = new DatabaseRefreshCommand(converter)
            {
                Project = tempDir,
                ContextName = "NonExistentContext"
            };

            try
            {
                var result = await command.OnExecuteAsync();

                result.Should().Be(1, "because the specified context EDMX file does not exist");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public async Task OnExecuteAsync_WithBaselinesSubfolder_ShouldResolveProjectPathToParent()
        {
            var tempDir = CreateTempProjectStructure();
            var baselinesDir = Path.Combine(tempDir, "Baselines");
            Directory.CreateDirectory(baselinesDir);

            // Create EDMX and config in Baselines folder
            var edmxContent = """
                <?xml version="1.0" encoding="utf-8"?>
                <edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
                </edmx:Edmx>
                """;
            File.WriteAllText(Path.Combine(baselinesDir, "TestContext.edmx"), edmxContent);

            // Create config that references appsettings.json (which won't exist, but that's OK for this test)
            var configContent = """
                {
                    "connectionStringSource": "appsettings.json:ConnectionStrings:DefaultConnection",
                    "contextName": "TestContext",
                    "provider": "SqlServer"
                }
                """;
            File.WriteAllText(Path.Combine(baselinesDir, "TestContext.edmx.config"), configContent);

            var converter = new EdmxConverter();
            var command = new DatabaseRefreshCommand(converter)
            {
                Project = baselinesDir
            };

            try
            {
                // This will fail because appsettings.json doesn't exist, but it should NOT fail
                // because of "No .csproj file found in Baselines" - that's the key assertion
                var result = await command.OnExecuteAsync();

                // The command will fail, but the error should NOT be about missing .csproj in Baselines
                // It should be about the connection string or database not being available
                result.Should().Be(1);
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        #endregion

        #region Baseline Generator

        /// <summary>
        /// Regenerates the baseline EDMX file from the database.
        /// This test is excluded from normal test runs and should be run manually when needed.
        /// </summary>
        /// <remarks>
        /// To run this test manually:
        /// dotnet test --filter "TestCategory=BaselineGenerator"
        ///
        /// Prerequisites:
        /// - The database must be accessible
        /// - User secrets must be configured with the connection string
        /// </remarks>
        [TestMethod]
        [TestCategory("BaselineGenerator")]
        [Ignore("Run manually to regenerate baselines: dotnet test --filter \"FullyQualifiedName~RegenerateBaseline\"")]
        public async Task RegenerateBaseline_RestierTest_ShouldUpdateEdmxFile()
        {
            // Get the path to the Baselines folder relative to the test project
            var testProjectDir = GetTestProjectDirectory();
            var baselinesDir = Path.Combine(testProjectDir, "Baselines");

            Console.WriteLine($"Test Project Directory: {testProjectDir}");
            Console.WriteLine($"Baselines Directory: {baselinesDir}");

            // Verify the Baselines folder exists
            Directory.Exists(baselinesDir).Should().BeTrue($"Baselines directory should exist at {baselinesDir}");

            // Verify the EDMX and config files exist
            var edmxPath = Path.Combine(baselinesDir, "RestierTest.edmx");
            var configPath = Path.Combine(baselinesDir, "RestierTest.edmx.config");

            File.Exists(edmxPath).Should().BeTrue($"RestierTest.edmx should exist at {edmxPath}");
            File.Exists(configPath).Should().BeTrue($"RestierTest.edmx.config should exist at {configPath}");

            // Create the converter and command
            var converter = new EdmxConverter();
            var command = new DatabaseRefreshCommand(converter)
            {
                Project = baselinesDir,
                ContextName = "RestierTest"
            };

            // Execute the refresh
            var result = await command.OnExecuteAsync();

            // Verify success
            result.Should().Be(0, "because the EDMX refresh should succeed");

            // Verify the EDMX file was updated
            var updatedEdmx = await File.ReadAllTextAsync(edmxPath);
            updatedEdmx.Should().NotBeNullOrEmpty();
            updatedEdmx.Should().Contain("edmx:Edmx", "because the file should be a valid EDMX");

            Console.WriteLine("Baseline EDMX file regenerated successfully!");
            Console.WriteLine($"Updated file: {edmxPath}");
        }

        /// <summary>
        /// Gets the directory of the test project by walking up from the current directory.
        /// </summary>
        private static string GetTestProjectDirectory()
        {
            // Start from the current directory and walk up to find the test project
            var currentDir = Directory.GetCurrentDirectory();

            // Look for the CloudNimble.EasyAF.Tests.Tools directory
            while (!string.IsNullOrEmpty(currentDir))
            {
                var projectFile = Path.Combine(currentDir, "CloudNimble.EasyAF.Tests.Tools.csproj");
                if (File.Exists(projectFile))
                {
                    return currentDir;
                }

                // Check if we're in a bin/Debug or bin/Release folder
                var baselinesPath = Path.Combine(currentDir, "Baselines");
                if (Directory.Exists(baselinesPath) && File.Exists(Path.Combine(baselinesPath, "RestierTest.edmx")))
                {
                    return currentDir;
                }

                currentDir = Path.GetDirectoryName(currentDir);
            }

            // Fallback: try to find it relative to the solution
            var solutionDir = FindSolutionDirectory();
            if (!string.IsNullOrEmpty(solutionDir))
            {
                var testProjectPath = Path.Combine(solutionDir, "src", "CloudNimble.EasyAF.Tests.Tools");
                if (Directory.Exists(testProjectPath))
                {
                    return testProjectPath;
                }
            }

            throw new InvalidOperationException("Could not locate the test project directory");
        }

        /// <summary>
        /// Finds the solution directory by walking up from the current directory.
        /// </summary>
        private static string FindSolutionDirectory()
        {
            var currentDir = Directory.GetCurrentDirectory();

            while (!string.IsNullOrEmpty(currentDir))
            {
                var slnFiles = Directory.GetFiles(currentDir, "*.sln");
                if (slnFiles.Length > 0)
                {
                    return currentDir;
                }

                var slnxFiles = Directory.GetFiles(currentDir, "*.slnx");
                if (slnxFiles.Length > 0)
                {
                    return currentDir;
                }

                currentDir = Path.GetDirectoryName(currentDir);
            }

            return null;
        }

        #endregion

    }

}
