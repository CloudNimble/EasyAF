using CloudNimble.EasyAF.Tools.Scaffolding;
using FluentAssertions;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.Tools.Scaffolding
{

    /// <summary>
    /// Unit tests for the <see cref="SolutionFileBuilder"/> class.
    /// </summary>
    [TestClass]
    public class SolutionFileBuilderTests
    {

        #region Fields

        private string _tempDir;

        #endregion

        #region Test Setup

        /// <summary>
        /// Creates a temporary solution directory for each test.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"EasyAF_SolutionFileBuilder_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDir);
        }

        /// <summary>
        /// Removes the temporary solution directory after each test.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_tempDir))
            {
                try
                {
                    Directory.Delete(_tempDir, recursive: true);
                }
                catch
                {
                    // Best effort.
                }
            }
        }

        /// <summary>
        /// Returns the full path a project file would have under the temporary solution directory.
        /// </summary>
        /// <param name="projectName">The project name.</param>
        /// <returns>The full path of <c>{projectName}/{projectName}.csproj</c>.</returns>
        private string ProjectPath(string projectName)
        {
            return Path.Combine(_tempDir, projectName, $"{projectName}.csproj");
        }

        /// <summary>
        /// Finds a project in a reopened model regardless of which directory separator the serializer normalized to.
        /// </summary>
        /// <param name="model">The reopened solution model.</param>
        /// <param name="relativePath">The project path with forward slashes.</param>
        /// <returns>The matching project.</returns>
        private static SolutionProjectModel FindProject(SolutionModel model, string relativePath)
        {
            return model.SolutionProjects.Single(p => p.FilePath.Replace('\\', '/') == relativePath);
        }

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullSolutionDirectory_ShouldThrowArgumentException()
        {
            Action act = () => new SolutionFileBuilder(null, "Contoso");

            act.Should().Throw<ArgumentException>().WithParameterName("solutionDirectory");
        }

        [TestMethod]
        public void Constructor_WithNullSolutionName_ShouldThrowArgumentException()
        {
            Action act = () => new SolutionFileBuilder(_tempDir, null);

            act.Should().Throw<ArgumentException>().WithParameterName("solutionName");
        }

        [TestMethod]
        public void Constructor_ShouldDefaultToDebugAndReleaseBuildTypes()
        {
            var builder = new SolutionFileBuilder(_tempDir, "Contoso");

            builder.BuildTypes.Should().Equal("Debug", "Release");
        }

        [TestMethod]
        public void Constructor_WithRelativeSolutionDirectory_ShouldStoreFullPath()
        {
            var builder = new SolutionFileBuilder(".", "Contoso");

            Path.IsPathRooted(builder.SolutionDirectory).Should().BeTrue();
        }

        #endregion

        #region AddBuildType Tests

        [TestMethod]
        public void AddBuildType_WithNull_ShouldThrowArgumentException()
        {
            var builder = new SolutionFileBuilder(_tempDir, "Contoso");

            Action act = () => builder.AddBuildType(null);

            act.Should().Throw<ArgumentException>().WithParameterName("buildType");
        }

        [TestMethod]
        public void AddBuildType_ShouldAppendInOrderAndIgnoreDuplicates()
        {
            var builder = new SolutionFileBuilder(_tempDir, "Contoso");

            builder.AddBuildType("DEV").AddBuildType("BETA").AddBuildType("DEV").AddBuildType("PROD");

            builder.BuildTypes.Should().Equal("Debug", "Release", "DEV", "BETA", "PROD");
        }

        #endregion

        #region AddProject Tests

        [TestMethod]
        public void AddProject_WithNullFolder_ShouldThrowArgumentException()
        {
            var builder = new SolutionFileBuilder(_tempDir, "Contoso");

            Action act = () => builder.AddProject(null, ProjectPath("Contoso.Core"));

            act.Should().Throw<ArgumentException>().WithParameterName("solutionFolder");
        }

        [TestMethod]
        public void AddProject_WithNullProjectFilePath_ShouldThrowArgumentException()
        {
            var builder = new SolutionFileBuilder(_tempDir, "Contoso");

            Action act = () => builder.AddProject("Core", null);

            act.Should().Throw<ArgumentException>().WithParameterName("projectFilePath");
        }

        [TestMethod]
        public void AddProject_WithSameProjectTwice_ShouldThrowInvalidOperationException()
        {
            var builder = new SolutionFileBuilder(_tempDir, "Contoso").AddProject("Core", ProjectPath("Contoso.Core"));

            Action act = () => builder.AddProject("Core", ProjectPath("Contoso.Core"));

            act.Should().Throw<InvalidOperationException>().WithMessage("*Contoso.Core*");
        }

        #endregion

        #region AddSolutionItem Tests

        [TestMethod]
        public void AddSolutionItem_WithNullFolder_ShouldThrowArgumentException()
        {
            var builder = new SolutionFileBuilder(_tempDir, "Contoso");

            Action act = () => builder.AddSolutionItem(null, Path.Combine(_tempDir, "global.json"));

            act.Should().Throw<ArgumentException>().WithParameterName("solutionFolder");
        }

        [TestMethod]
        public void AddSolutionItem_WithNullFilePath_ShouldThrowArgumentException()
        {
            var builder = new SolutionFileBuilder(_tempDir, "Contoso");

            Action act = () => builder.AddSolutionItem("Solution Items", null);

            act.Should().Throw<ArgumentException>().WithParameterName("filePath");
        }

        #endregion

        #region SaveAsync Tests

        [TestMethod]
        public async Task SaveAsync_ShouldWriteSlnxNamedAfterSolution()
        {
            var builder = new SolutionFileBuilder(_tempDir, "CloudNimble.Contoso");

            var path = await builder.SaveAsync();

            path.Should().Be(Path.Combine(_tempDir, "CloudNimble.Contoso.slnx"));
            File.Exists(path).Should().BeTrue();
            (await File.ReadAllTextAsync(path)).Should().StartWith("<Solution");
        }

        [TestMethod]
        public async Task SaveAsync_ShouldPlaceProjectsInSolutionFoldersWithRelativeForwardSlashPaths()
        {
            var builder = new SolutionFileBuilder(_tempDir, "Contoso")
                .AddProject("Core", ProjectPath("Contoso.Core"))
                .AddProject("Core", ProjectPath("Contoso.Data"))
                .AddProject("/Web/", ProjectPath("Contoso.Api"));

            var path = await builder.SaveAsync();

            var content = await File.ReadAllTextAsync(path);
            content.Should().Contain("Path=\"Contoso.Core/Contoso.Core.csproj\"");
            var model = await SolutionSerializers.SlnXml.OpenAsync(path, default);
            model.SolutionFolders.Select(f => f.Path).Should().BeEquivalentTo("/Core/", "/Web/");
            FindProject(model, "Contoso.Core/Contoso.Core.csproj").Parent.Path.Should().Be("/Core/");
            FindProject(model, "Contoso.Data/Contoso.Data.csproj").Parent.Path.Should().Be("/Core/");
            FindProject(model, "Contoso.Api/Contoso.Api.csproj").Parent.Path.Should().Be("/Web/");
        }

        [TestMethod]
        public async Task SaveAsync_ShouldWriteCustomBuildTypes()
        {
            var builder = new SolutionFileBuilder(_tempDir, "Contoso")
                .AddBuildType("DEV").AddBuildType("BETA").AddBuildType("PROD")
                .AddProject("Core", ProjectPath("Contoso.Core"));

            var path = await builder.SaveAsync();

            var content = await File.ReadAllTextAsync(path);
            content.Should().Contain("<BuildType Name=\"DEV\" />");
            var model = await SolutionSerializers.SlnXml.OpenAsync(path, default);
            model.BuildTypes.Should().BeEquivalentTo("Debug", "Release", "DEV", "BETA", "PROD");
        }

        [TestMethod]
        public async Task SaveAsync_ShouldWriteSolutionItemsIntoTheirFolder()
        {
            var builder = new SolutionFileBuilder(_tempDir, "Contoso")
                .AddSolutionItem("Solution Items", Path.Combine(_tempDir, "Directory.Build.props"))
                .AddSolutionItem("Solution Items", Path.Combine(_tempDir, "global.json"));

            var path = await builder.SaveAsync();

            var content = await File.ReadAllTextAsync(path);
            content.Should().Contain("<File Path=\"Directory.Build.props\" />");
            var model = await SolutionSerializers.SlnXml.OpenAsync(path, default);
            model.FindFolder("/Solution Items/").Files.Should().BeEquivalentTo("Directory.Build.props", "global.json");
        }

        [TestMethod]
        public async Task SaveAsync_WhenFileExists_ShouldOverwriteIt()
        {
            var path = Path.Combine(_tempDir, "Contoso.slnx");
            await File.WriteAllTextAsync(path, "stale");
            var builder = new SolutionFileBuilder(_tempDir, "Contoso").AddProject("Core", ProjectPath("Contoso.Core"));

            await builder.SaveAsync();

            (await File.ReadAllTextAsync(path)).Should().NotBe("stale").And.Contain("Contoso.Core");
        }

        #endregion

    }

}
