using CloudNimble.EasyAF.MSBuild;
using FluentAssertions;
using Microsoft.Build.Locator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.MSBuild
{

    /// <summary>
    /// Simple tests for the <see cref="MSBuildProjectManager"/> class to verify basic functionality.
    /// </summary>
    [TestClass]
    public class MSBuildProjectManagerSimpleTest
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

        #region Simple Tests

        [TestMethod]
        public void EnsureMSBuildRegistered_ShouldNotThrow()
        {
            // MSBuild should already be registered by ClassInitialize
            MSBuildLocator.IsRegistered.Should().BeTrue();
        }

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
        public void CreateNew_ShouldCreateBasicProject()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"MSBuildTest_{Guid.NewGuid():N}");
            var projectPath = Path.Combine(tempDir, "TestProject.csproj");

            try
            {
                Directory.CreateDirectory(tempDir);

                var manager = new MSBuildProjectManager();
                manager.CreateNew(projectPath, "net8.0");

                // Debug output
                if (!manager.IsLoaded)
                {
                    Console.WriteLine($"Project not loaded. Errors: {string.Join(", ", manager.ProjectErrors.Select(e => e.ErrorText))}");
                }

                manager.IsLoaded.Should().BeTrue();
                manager.Project.Should().NotBeNull();
                manager.Project.Sdk.Should().Be("Microsoft.NET.Sdk");
                
                // Save and verify file exists
                manager.Save();
                File.Exists(projectPath).Should().BeTrue();
                
                var content = File.ReadAllText(projectPath);
                content.Should().Contain("net8.0");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        [TestMethod] 
        public void SetProperty_ShouldAddProperty()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"MSBuildTest_{Guid.NewGuid():N}");
            var projectPath = Path.Combine(tempDir, "TestProject.csproj");

            try
            {
                Directory.CreateDirectory(tempDir);

                var manager = new MSBuildProjectManager();
                manager.CreateNew(projectPath, "net8.0");
                
                manager.SetProperty("TestProperty", "TestValue");
                manager.GetPropertyValue("TestProperty").Should().Be("TestValue");
                
                manager.Save();
                var content = File.ReadAllText(projectPath);
                content.Should().Contain("TestProperty");
                content.Should().Contain("TestValue");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
        }

        #endregion

    }

}