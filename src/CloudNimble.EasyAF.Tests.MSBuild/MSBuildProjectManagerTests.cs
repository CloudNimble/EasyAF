using CloudNimble.EasyAF.MSBuild;
using FluentAssertions;
using Microsoft.Build.Construction;
using Microsoft.Build.Locator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CloudNimble.EasyAF.Tests.MSBuild
{

    /// <summary>
    /// Unit tests for the <see cref="MSBuildProjectManager"/> class.
    /// </summary>
    [TestClass]
    public class MSBuildProjectManagerTests
    {

        #region Test Initialization

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Ensure MSBuild is registered before any tests run
            MSBuildProjectManager.EnsureMSBuildRegistered();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the test context.
        /// </summary>
        public TestContext TestContext { get; set; }

        #endregion

        #region Test Setup

        /// <summary>
        /// Creates a temporary directory for testing.
        /// </summary>
        /// <returns>Path to the created temporary directory.</returns>
        private static string CreateTempDirectory()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"MSBuildProjectManager_Test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        /// <summary>
        /// Creates a minimal .csproj test file.
        /// </summary>
        /// <param name="directory">The directory to create the file in.</param>
        /// <param name="fileName">The file name (default: "TestProject.csproj").</param>
        /// <returns>Full path to the created file.</returns>
        private static string CreateTestCsprojFile(string directory, string fileName = "TestProject.csproj")
        {
            var filePath = Path.Combine(directory, fileName);
            var projectContent = """
                <Project Sdk="Microsoft.NET.Sdk">
                    <PropertyGroup>
                        <TargetFramework>net8.0</TargetFramework>
                    </PropertyGroup>
                </Project>
                """;
            
            File.WriteAllText(filePath, projectContent);
            return filePath;
        }

        /// <summary>
        /// Creates a Directory.Build.props test file.
        /// </summary>
        /// <param name="directory">The directory to create the file in.</param>
        /// <returns>Full path to the created file.</returns>
        private static string CreateTestDirectoryBuildPropsFile(string directory)
        {
            var filePath = Path.Combine(directory, "Directory.Build.props");
            var projectContent = """
                <?xml version="1.0" encoding="utf-8"?>
                <Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
                    <PropertyGroup>
                        <EasyAFNamespace>TestNamespace</EasyAFNamespace>
                        <UserSecretsId>test-guid</UserSecretsId>
                    </PropertyGroup>
                </Project>
                """;
            
            File.WriteAllText(filePath, projectContent);
            return filePath;
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

        #region Static Method Tests

        [TestMethod]
        public void EnsureMSBuildRegistered_ShouldNotThrow()
        {
            // MSBuild should already be registered by ClassInitialize
            MSBuildLocator.IsRegistered.Should().BeTrue();
        }

        [TestMethod]
        public void EnsureMSBuildRegistered_WhenCalledMultipleTimes_ShouldNotThrow()
        {
            // MSBuild should already be registered by ClassInitialize
            // Multiple calls should not throw
            Action act = () =>
            {
                MSBuildProjectManager.EnsureMSBuildRegistered();
                MSBuildProjectManager.EnsureMSBuildRegistered();
                MSBuildProjectManager.EnsureMSBuildRegistered();
            };

            act.Should().NotThrow();
            MSBuildLocator.IsRegistered.Should().BeTrue();
        }

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_Default_ShouldInitializeWithDefaults()
        {
            var manager = new MSBuildProjectManager();

            manager.Project.Should().BeNull();
            manager.FilePath.Should().BeNull();
            manager.IsLoaded.Should().BeFalse();
            manager.ProjectErrors.Should().NotBeNull().And.BeEmpty();
            manager.PreserveFormatting.Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_WithFilePath_ShouldSetFilePath()
        {
            var tempDir = CreateTempDirectory();
            var testFilePath = Path.Combine(tempDir, "test.csproj");

            try
            {
                var manager = new MSBuildProjectManager(testFilePath);

                manager.FilePath.Should().Be(Path.GetFullPath(testFilePath));
                manager.Project.Should().BeNull();
                manager.IsLoaded.Should().BeFalse();
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void Constructor_WithNullFilePath_ShouldThrowArgumentException()
        {
            Action act = () => new MSBuildProjectManager(null);

            act.Should().Throw<ArgumentException>()
                .WithParameterName("filePath");
        }

        [TestMethod]
        public void Constructor_WithWhitespaceFilePath_ShouldThrowArgumentException()
        {
            Action act = () => new MSBuildProjectManager("   ");

            act.Should().Throw<ArgumentException>()
                .WithParameterName("filePath");
        }

        #endregion

        #region Load Tests

        [TestMethod]
        public void Load_WithoutFilePath_ShouldThrowInvalidOperationException()
        {
            var manager = new MSBuildProjectManager();

            Action act = () => manager.Load();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*No file path has been specified*");
        }

        [TestMethod]
        public void Load_WithNonExistentFile_ShouldAddErrorAndReturnManager()
        {
            var tempDir = CreateTempDirectory();
            var nonExistentFile = Path.Combine(tempDir, "NonExistent.csproj");

            try
            {
                var manager = new MSBuildProjectManager();
                var result = manager.Load(nonExistentFile);

                result.Should().BeSameAs(manager);
                manager.IsLoaded.Should().BeFalse();
                manager.Project.Should().BeNull();
                manager.ProjectErrors.Should().HaveCount(1);
                manager.ProjectErrors[0].ErrorNumber.Should().Be("FILE_NOT_FOUND");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void Load_WithValidCsprojFile_ShouldLoadSuccessfully()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                var result = manager.Load(testFile);

                result.Should().BeSameAs(manager);
                manager.IsLoaded.Should().BeTrue();
                manager.Project.Should().NotBeNull();
                manager.FilePath.Should().Be(Path.GetFullPath(testFile));
                manager.ProjectErrors.Should().BeEmpty();
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void Load_WithValidDirectoryBuildPropsFile_ShouldLoadSuccessfully()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestDirectoryBuildPropsFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                var result = manager.Load(testFile);

                result.Should().BeSameAs(manager);
                manager.IsLoaded.Should().BeTrue();
                manager.Project.Should().NotBeNull();
                manager.FilePath.Should().Be(Path.GetFullPath(testFile));
                manager.ProjectErrors.Should().BeEmpty();
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void Load_WithPreserveFormattingTrue_ShouldSetPreserveFormatting()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile, preserveFormatting: true);

                manager.PreserveFormatting.Should().BeTrue();
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void Load_WithPreserveFormattingFalse_ShouldSetPreserveFormatting()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile, preserveFormatting: false);

                manager.PreserveFormatting.Should().BeFalse();
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        #endregion

        #region CreateNew Tests

        [TestMethod]
        public void CreateNew_WithValidPath_ShouldCreateNewProject()
        {
            var tempDir = CreateTempDirectory();
            var newProjectPath = Path.Combine(tempDir, "NewProject.csproj");

            try
            {
                var manager = new MSBuildProjectManager();
                manager.CreateNew(newProjectPath);

                manager.Project.Should().NotBeNull();
                manager.FilePath.Should().Be(Path.GetFullPath(newProjectPath));
                manager.IsLoaded.Should().BeTrue();
                manager.PreserveFormatting.Should().BeFalse();
                manager.Project.Sdk.Should().Be("Microsoft.NET.Sdk");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void CreateNew_WithCustomTargetFramework_ShouldSetTargetFramework()
        {
            var tempDir = CreateTempDirectory();
            var newProjectPath = Path.Combine(tempDir, "NewProject.csproj");

            try
            {
                var manager = new MSBuildProjectManager();
                manager.CreateNew(newProjectPath, "net9.0");

                manager.Project.Should().NotBeNull();
                var targetFramework = manager.Project.Properties.FirstOrDefault(p => p.Name == "TargetFramework");
                targetFramework.Should().NotBeNull();
                targetFramework.Value.Should().Be("net9.0");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void CreateNew_WithNullFilePath_ShouldThrowArgumentException()
        {
            var manager = new MSBuildProjectManager();

            Action act = () => manager.CreateNew(null);

            act.Should().Throw<ArgumentException>()
                .WithParameterName("filePath");
        }

        [TestMethod]
        public void CreateNew_WithDirectoryBuildPropsExtension_ShouldNotAddSdk()
        {
            var tempDir = CreateTempDirectory();
            var newProjectPath = Path.Combine(tempDir, "Directory.Build.props");

            try
            {
                var manager = new MSBuildProjectManager();
                manager.CreateNew(newProjectPath);

                manager.Project.Should().NotBeNull();
                manager.Project.Sdk.Should().BeNullOrEmpty();
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        #endregion

        #region Save Tests

        [TestMethod]
        public void Save_WithoutLoadedProject_ShouldThrowInvalidOperationException()
        {
            var manager = new MSBuildProjectManager();

            Action act = () => manager.Save();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*No project is loaded*");
        }

        [TestMethod]
        public void Save_WithoutFilePath_ShouldThrowInvalidOperationException()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);
                // Clear the file path by using reflection since it's read-only
                var fieldInfo = typeof(MSBuildProjectManager).GetField("_filePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                fieldInfo?.SetValue(manager, null);

                Action act = () => manager.Save();

                act.Should().Throw<InvalidOperationException>()
                    .WithMessage("*No file path is specified*");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void Save_WithLoadedProject_ShouldSaveToOriginalPath()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);
                manager.SetProperty("TestProperty", "TestValue");

                manager.Save();

                File.Exists(testFile).Should().BeTrue();
                var savedContent = File.ReadAllText(testFile);
                savedContent.Should().Contain("TestProperty");
                savedContent.Should().Contain("TestValue");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void Save_WithSpecificPath_ShouldSaveToSpecifiedPath()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);
            var saveFile = Path.Combine(tempDir, "SavedProject.csproj");

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);
                manager.SetProperty("TestProperty", "TestValue");

                manager.Save(saveFile);

                File.Exists(saveFile).Should().BeTrue();
                var savedContent = File.ReadAllText(saveFile);
                savedContent.Should().Contain("TestProperty");
                savedContent.Should().Contain("TestValue");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void Save_WithNullPath_ShouldThrowArgumentException()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                Action act = () => manager.Save(null);

                act.Should().Throw<ArgumentException>()
                    .WithParameterName("filePath");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        #endregion

        #region Property Management Tests

        [TestMethod]
        public void SetProperty_WithValidNameAndValue_ShouldSetProperty()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                var result = manager.SetProperty("TestProperty", "TestValue");

                result.Should().BeSameAs(manager);
                var property = manager.Project.Properties.FirstOrDefault(p => p.Name == "TestProperty");
                property.Should().NotBeNull();
                property.Value.Should().Be("TestValue");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void SetProperty_WithExistingProperty_ShouldUpdateProperty()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                manager.SetProperty("TargetFramework", "net9.0");

                var property = manager.Project.Properties.FirstOrDefault(p => p.Name == "TargetFramework");
                property.Should().NotBeNull();
                property.Value.Should().Be("net9.0");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void SetProperty_WithoutLoadedProject_ShouldThrowInvalidOperationException()
        {
            var manager = new MSBuildProjectManager();

            Action act = () => manager.SetProperty("Test", "Value");

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*No project is loaded*");
        }

        [TestMethod]
        public void SetProperty_WithNullName_ShouldThrowArgumentException()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                Action act = () => manager.SetProperty(null, "Value");

                act.Should().Throw<ArgumentException>()
                    .WithParameterName("name");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void SetProperty_WithNullValue_ShouldThrowArgumentException()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                Action act = () => manager.SetProperty("TestProperty", null);

                act.Should().Throw<ArgumentException>()
                    .WithParameterName("value");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void GetPropertyValue_WithExistingProperty_ShouldReturnValue()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);
                manager.SetProperty("TestProperty", "TestValue");

                var value = manager.GetPropertyValue("TestProperty");

                value.Should().Be("TestValue");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void GetPropertyValue_WithNonExistentProperty_ShouldReturnNull()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                var value = manager.GetPropertyValue("NonExistentProperty");

                value.Should().BeNull();
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void RemoveProperty_WithExistingProperty_ShouldRemoveProperty()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);
                manager.SetProperty("TestProperty", "TestValue");

                var result = manager.RemoveProperty("TestProperty");

                result.Should().BeSameAs(manager);
                var property = manager.Project.Properties.FirstOrDefault(p => p.Name == "TestProperty");
                property.Should().BeNull();
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void RemoveProperty_WithNonExistentProperty_ShouldNotThrow()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                Action act = () => manager.RemoveProperty("NonExistentProperty");

                act.Should().NotThrow();
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        #endregion

        #region PackageReference Tests

        [TestMethod]
        public void AddPackageReference_WithValidPackage_ShouldAddPackageReference()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                var result = manager.AddPackageReference("TestPackage", "1.0.0");

                result.Should().BeSameAs(manager);
                var itemGroup = manager.Project.ItemGroups.FirstOrDefault(ig => 
                    ig.Items.Any(item => item.ItemType == "PackageReference"));
                itemGroup.Should().NotBeNull();
                
                var packageRef = itemGroup.Items.FirstOrDefault(item => 
                    item.ItemType == "PackageReference" && item.Include == "TestPackage");
                packageRef.Should().NotBeNull();
                
                var versionMetadata = packageRef.Metadata.FirstOrDefault(m => m.Name == "Version");
                versionMetadata.Should().NotBeNull();
                versionMetadata.Value.Should().Be("1.0.0");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void AddPackageReference_WithExistingPackage_ShouldUpdateVersion()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);
                manager.AddPackageReference("TestPackage", "1.0.0");

                manager.AddPackageReference("TestPackage", "2.0.0");

                var itemGroups = manager.Project.ItemGroups.Where(ig => 
                    ig.Items.Any(item => item.ItemType == "PackageReference"));
                var packageRefs = itemGroups.SelectMany(ig => ig.Items)
                    .Where(item => item.ItemType == "PackageReference" && item.Include == "TestPackage");
                
                packageRefs.Should().HaveCount(1);
                var versionMetadata = packageRefs.First().Metadata.FirstOrDefault(m => m.Name == "Version");
                versionMetadata.Value.Should().Be("2.0.0");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void AddPackageReference_WithCondition_ShouldAddConditionalPackageReference()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                manager.AddPackageReference("TestPackage", "1.0.0", "'$(Configuration)' == 'Debug'");

                var itemGroup = manager.Project.ItemGroups.FirstOrDefault(ig => 
                    ig.Condition == "'$(Configuration)' == 'Debug'");
                itemGroup.Should().NotBeNull();
                
                var packageRef = itemGroup.Items.FirstOrDefault(item => 
                    item.ItemType == "PackageReference" && item.Include == "TestPackage");
                packageRef.Should().NotBeNull();
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void AddPackageReference_WithNullPackageId_ShouldThrowArgumentException()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                Action act = () => manager.AddPackageReference(null, "1.0.0");

                act.Should().Throw<ArgumentException>()
                    .WithParameterName("packageId");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        #endregion

        #region EasyAF-Specific Tests

        [TestMethod]
        public void SetEasyAFProjectType_WithValidType_ShouldSetProperty()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                var result = manager.SetEasyAFProjectType("Data");

                result.Should().BeSameAs(manager);
                manager.GetPropertyValue("EasyAFProjectType").Should().Be("Data");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void SetUserSecretsId_WithValidId_ShouldSetProperty()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                var testId = Guid.NewGuid().ToString();
                var result = manager.SetUserSecretsId(testId);

                result.Should().BeSameAs(manager);
                manager.GetPropertyValue("UserSecretsId").Should().Be(testId);
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void SetEasyAFNamespace_WithValidNamespace_ShouldSetProperty()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                var result = manager.SetEasyAFNamespace("TestCompany.TestProject");

                result.Should().BeSameAs(manager);
                manager.GetPropertyValue("EasyAFNamespace").Should().Be("TestCompany.TestProject");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void AddEasyAFAnalyzers_WithoutDataProject_ShouldAddAnalyzerPackage()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                var result = manager.AddEasyAFAnalyzers();

                result.Should().BeSameAs(manager);
                
                var itemGroup = manager.Project.ItemGroups.FirstOrDefault(ig => 
                    ig.Condition == " '$(EasyAFProjectType)' != '' ");
                itemGroup.Should().NotBeNull();
                
                var analyzerPackage = itemGroup.Items.FirstOrDefault(item => 
                    item.ItemType == "PackageReference" && item.Include == "EasyAF.Analyzers.EF6");
                analyzerPackage.Should().NotBeNull();
                
                var versionMetadata = analyzerPackage.Metadata.FirstOrDefault(m => m.Name == "Version");
                versionMetadata.Should().NotBeNull();
                versionMetadata.Value.Should().Be("3.*-*");
                
                var privateAssetsMetadata = analyzerPackage.Metadata.FirstOrDefault(m => m.Name == "PrivateAssets");
                privateAssetsMetadata.Should().NotBeNull();
                privateAssetsMetadata.Value.Should().Be("all");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void AddEasyAFAnalyzers_WithDataProject_ShouldAddAnalyzerPackageAndAdditionalFiles()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                var result = manager.AddEasyAFAnalyzers("TestProject.Data");

                result.Should().BeSameAs(manager);
                
                var itemGroup = manager.Project.ItemGroups.FirstOrDefault(ig => 
                    ig.Condition == " '$(EasyAFProjectType)' != '' ");
                itemGroup.Should().NotBeNull();
                
                var additionalFiles = itemGroup.Items.FirstOrDefault(item => 
                    item.ItemType == "AdditionalFiles");
                additionalFiles.Should().NotBeNull();
                additionalFiles.Include.Should().Be("..\\TestProject.Data\\*.edmx");
                
                var linkMetadata = additionalFiles.Metadata.FirstOrDefault(m => m.Name == "Link");
                linkMetadata.Should().NotBeNull();
                linkMetadata.Value.Should().Be("EasyAF\\%(FileName).edmx");
                
                var visibleMetadata = additionalFiles.Metadata.FirstOrDefault(m => m.Name == "Visible");
                visibleMetadata.Should().NotBeNull();
                visibleMetadata.Value.Should().Be("false");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void CreateDirectoryBuildProps_WithValidParameters_ShouldCreateConfiguredProject()
        {
            var tempDir = CreateTempDirectory();
            var dbpPath = Path.Combine(tempDir, "Directory.Build.props");

            try
            {
                var testNamespace = "TestCompany.TestProject";
                var testUserSecretsId = Guid.NewGuid().ToString();
                
                var manager = MSBuildProjectManager.CreateDirectoryBuildProps(
                    dbpPath, testNamespace, testUserSecretsId);

                manager.Should().NotBeNull();
                manager.IsLoaded.Should().BeTrue();
                manager.FilePath.Should().Be(Path.GetFullPath(dbpPath));
                
                manager.GetPropertyValue("EasyAFNamespace").Should().Be(testNamespace);
                manager.GetPropertyValue("UserSecretsId").Should().Be(testUserSecretsId);
                manager.GetPropertyValue("TargetFramework").Should().BeNull();
                
                var analyzerItemGroup = manager.Project.ItemGroups.FirstOrDefault(ig => 
                    ig.Condition == " '$(EasyAFProjectType)' != '' ");
                analyzerItemGroup.Should().NotBeNull();
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void CreateDirectoryBuildProps_WithDataProject_ShouldIncludeAdditionalFiles()
        {
            var tempDir = CreateTempDirectory();
            var dbpPath = Path.Combine(tempDir, "Directory.Build.props");

            try
            {
                var testNamespace = "TestCompany.TestProject";
                var testUserSecretsId = Guid.NewGuid().ToString();
                var dataProjectPath = "TestProject.Data";
                
                var manager = MSBuildProjectManager.CreateDirectoryBuildProps(
                    dbpPath, testNamespace, testUserSecretsId, dataProjectPath);

                var itemGroup = manager.Project.ItemGroups.FirstOrDefault(ig => 
                    ig.Condition == " '$(EasyAFProjectType)' != '' ");
                
                var additionalFiles = itemGroup.Items.FirstOrDefault(item => 
                    item.ItemType == "AdditionalFiles");
                additionalFiles.Should().NotBeNull();
                additionalFiles.Include.Should().Be("..\\TestProject.Data\\*.edmx");
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        #endregion

        #region ItemGroup Builder Tests

        [TestMethod]
        public void AddItemGroup_WithValidConditionAndConfiguration_ShouldAddItemGroup()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                var result = manager.AddItemGroup("'$(Configuration)' == 'Debug'", itemGroup =>
                {
                    itemGroup.AddPackageReference("DebugPackage", "1.0.0");
                });

                result.Should().BeSameAs(manager);
                
                var itemGroup = manager.Project.ItemGroups.FirstOrDefault(ig => 
                    ig.Condition == "'$(Configuration)' == 'Debug'");
                itemGroup.Should().NotBeNull();
                
                var packageRef = itemGroup.Items.FirstOrDefault(item => 
                    item.ItemType == "PackageReference" && item.Include == "DebugPackage");
                packageRef.Should().NotBeNull();
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        [TestMethod]
        public void AddItemGroup_WithNullConfigureAction_ShouldThrowArgumentNullException()
        {
            var tempDir = CreateTempDirectory();
            var testFile = CreateTestCsprojFile(tempDir);

            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(testFile);

                Action act = () => manager.AddItemGroup("test", null);

                act.Should().Throw<ArgumentNullException>();
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        #endregion

    }

}