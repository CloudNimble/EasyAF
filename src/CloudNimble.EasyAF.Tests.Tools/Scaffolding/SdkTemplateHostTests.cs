using CloudNimble.EasyAF.Tools.Scaffolding;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.Tools.Scaffolding
{

    /// <summary>
    /// Unit tests for the <see cref="SdkTemplateHost"/> class. Tests that create projects instantiate the SDK's own
    /// <c>classlib</c> / <c>web</c> / <c>console</c> templates from the installed .NET SDK, so they require
    /// an SDK with a <c>templates/10.0.*</c> folder on the machine.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class SdkTemplateHostTests
    {

        #region Fields

        private string _tempDir;

        #endregion

        #region Test Setup

        /// <summary>
        /// Creates a temporary directory for each test.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"EasyAF_SdkTemplateHost_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDir);
        }

        /// <summary>
        /// Removes the temporary directory after each test.
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
        /// Creates a fake versioned template folder under <paramref name="root"/>, optionally containing a placeholder nupkg.
        /// </summary>
        /// <param name="root">The templates root to create the folder in.</param>
        /// <param name="version">The folder name, for example <c>10.0.12</c>.</param>
        /// <param name="withPackage">Whether to drop a placeholder <c>.nupkg</c> into the folder.</param>
        /// <returns>The full path of the created folder.</returns>
        private static string CreateVersionFolder(string root, string version, bool withPackage = true)
        {
            var folder = Path.Combine(root, version);
            Directory.CreateDirectory(folder);
            if (withPackage)
            {
                File.WriteAllText(Path.Combine(folder, "placeholder.nupkg"), string.Empty);
            }
            return folder;
        }

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullTemplatesRoot_ShouldThrowArgumentException()
        {
            Action act = () => new SdkTemplateHost(null, "net10.0");

            act.Should().Throw<ArgumentException>().WithParameterName("templatesRoot");
        }

        [TestMethod]
        public void Constructor_WithWhitespaceTemplatesRoot_ShouldThrowArgumentException()
        {
            Action act = () => new SdkTemplateHost("   ", "net10.0");

            act.Should().Throw<ArgumentException>().WithParameterName("templatesRoot");
        }

        [TestMethod]
        public void Constructor_WithNullTargetFramework_ShouldThrowArgumentException()
        {
            Action act = () => new SdkTemplateHost(_tempDir, null);

            act.Should().Throw<ArgumentException>().WithParameterName("targetFramework");
        }

        [TestMethod]
        [DataRow("net472")]
        [DataRow("netstandard2.0")]
        [DataRow("net4.8")]
        [DataRow("garbage")]
        [DataRow("10.0")]
        public void Constructor_WithUnsupportedTargetFramework_ShouldThrowArgumentException(string targetFramework)
        {
            Action act = () => new SdkTemplateHost(_tempDir, targetFramework);

            act.Should().Throw<ArgumentException>().WithParameterName("targetFramework");
        }

        [TestMethod]
        public void Constructor_WithNonexistentTemplatesRoot_ShouldLeaveTemplateFolderNull()
        {
            var missing = Path.Combine(_tempDir, "does-not-exist");

            using var host = new SdkTemplateHost(missing, "net10.0");

            host.TemplateFolder.Should().BeNull();
            host.TemplatesRoot.Should().Be(missing);
        }

        [TestMethod]
        public void Constructor_WithRelativeTemplatesRoot_ShouldStoreFullPath()
        {
            using var host = new SdkTemplateHost(".", "net10.0");

            Path.IsPathRooted(host.TemplatesRoot).Should().BeTrue();
        }

        [TestMethod]
        public void Constructor_WithInstalledSdk_ShouldResolveTemplateFolderMatchingTargetFrameworkMajor()
        {
            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), "net10.0");

            host.TemplateFolder.Should().NotBeNull();
            Path.GetFileName(host.TemplateFolder).Should().StartWith("10.0.");
        }

        [TestMethod]
        [DataRow("net8.0", "8.0.")]
        [DataRow("net11.0", "11.0.")]
        public void Constructor_WithOtherInstalledMajors_ShouldResolveMatchingFolder(string targetFramework, string expectedPrefix)
        {
            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), targetFramework);

            if (host.TemplateFolder is null)
            {
                Assert.Inconclusive($"No templates folder for {targetFramework} is installed on this machine.");
            }

            Path.GetFileName(host.TemplateFolder).Should().StartWith(expectedPrefix);
        }

        [TestMethod]
        public void Constructor_WithEmptyTemplatesRoot_ShouldLeaveTemplateFolderNull()
        {
            using var host = new SdkTemplateHost(_tempDir, "net10.0");

            host.TemplateFolder.Should().BeNull();
        }

        [TestMethod]
        public void Constructor_WithNoFolderForMajor_ShouldLeaveTemplateFolderNull()
        {
            CreateVersionFolder(_tempDir, "9.0.5");

            using var host = new SdkTemplateHost(_tempDir, "net10.0");

            host.TemplateFolder.Should().BeNull();
        }

        #endregion

        #region GetMajorVersion Tests

        [TestMethod]
        [DataRow("net8.0", 8)]
        [DataRow("NET10.0", 10)]
        [DataRow("net11.0", 11)]
        public void GetMajorVersion_WithSupportedMoniker_ShouldReturnMajor(string targetFramework, int expected)
        {
            SdkTemplateHost.GetMajorVersion(targetFramework).Should().Be(expected);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("net472")]
        [DataRow("netcoreapp3.1")]
        public void GetMajorVersion_WithUnsupportedMoniker_ShouldThrowArgumentException(string targetFramework)
        {
            Action act = () => SdkTemplateHost.GetMajorVersion(targetFramework);

            act.Should().Throw<ArgumentException>().WithParameterName("targetFramework");
        }

        #endregion

        #region ResolveTemplateFolder Tests

        [TestMethod]
        public void ResolveTemplateFolder_WithMissingRoot_ShouldReturnNull()
        {
            SdkTemplateHost.ResolveTemplateFolder(Path.Combine(_tempDir, "nope"), 10).Should().BeNull();
        }

        [TestMethod]
        public void ResolveTemplateFolder_WithMultiplePatches_ShouldPickHighest()
        {
            CreateVersionFolder(_tempDir, "10.0.3");
            var expected = CreateVersionFolder(_tempDir, "10.0.12");
            CreateVersionFolder(_tempDir, "10.0.7");

            SdkTemplateHost.ResolveTemplateFolder(_tempDir, 10).Should().Be(expected);
        }

        [TestMethod]
        public void ResolveTemplateFolder_WithStableAndPrerelease_ShouldPreferStable()
        {
            CreateVersionFolder(_tempDir, "10.0.0-rc.1.25451.107");
            var expected = CreateVersionFolder(_tempDir, "10.0.0");
            CreateVersionFolder(_tempDir, "10.0.0-preview.6.25358.103");

            SdkTemplateHost.ResolveTemplateFolder(_tempDir, 10).Should().Be(expected);
        }

        [TestMethod]
        public void ResolveTemplateFolder_WithOnlyPrereleases_ShouldPickHighestLabel()
        {
            CreateVersionFolder(_tempDir, "11.0.0-preview.6.25358.103");
            var expected = CreateVersionFolder(_tempDir, "11.0.0-rc.1.26425.128");

            SdkTemplateHost.ResolveTemplateFolder(_tempDir, 11).Should().Be(expected);
        }

        [TestMethod]
        public void ResolveTemplateFolder_WithNonVersionFolders_ShouldIgnoreThem()
        {
            Directory.CreateDirectory(Path.Combine(_tempDir, "10.0.12-notes"));
            Directory.CreateDirectory(Path.Combine(_tempDir, "latest"));
            var expected = CreateVersionFolder(_tempDir, "10.0.1");

            SdkTemplateHost.ResolveTemplateFolder(_tempDir, 10).Should().Be(expected);
        }

        [TestMethod]
        public void ResolveTemplateFolder_WithFolderLackingPackages_ShouldSkipIt()
        {
            CreateVersionFolder(_tempDir, "10.0.12", withPackage: false);
            var expected = CreateVersionFolder(_tempDir, "10.0.3");

            SdkTemplateHost.ResolveTemplateFolder(_tempDir, 10).Should().Be(expected);
        }

        [TestMethod]
        public void ResolveTemplateFolder_WithOnlyOtherMajors_ShouldReturnNull()
        {
            CreateVersionFolder(_tempDir, "9.0.5");
            CreateVersionFolder(_tempDir, "11.0.0-rc.1.26425.128");

            SdkTemplateHost.ResolveTemplateFolder(_tempDir, 10).Should().BeNull();
        }

        #endregion

        #region FindDotnetRoot Tests

        [TestMethod]
        public void FindDotnetRoot_ShouldReturnDirectoryContainingTemplatesFolder()
        {
            var dotnetRoot = SdkTemplateHost.FindDotnetRoot();

            dotnetRoot.Should().NotBeNullOrWhiteSpace();
            Directory.Exists(Path.Combine(dotnetRoot, "templates")).Should().BeTrue();
        }

        [TestMethod]
        public void FindDotnetRoot_WithValidDotnetRootVariable_ShouldPreferIt()
        {
            var fakeRoot = Path.Combine(_tempDir, "fake-dotnet");
            Directory.CreateDirectory(Path.Combine(fakeRoot, "templates"));
            var original = Environment.GetEnvironmentVariable("DOTNET_ROOT");

            try
            {
                Environment.SetEnvironmentVariable("DOTNET_ROOT", fakeRoot);

                SdkTemplateHost.FindDotnetRoot().Should().Be(fakeRoot);
            }
            finally
            {
                Environment.SetEnvironmentVariable("DOTNET_ROOT", original);
            }
        }

        [TestMethod]
        public void FindDotnetRoot_WithBogusDotnetRootVariable_ShouldFallThroughToPath()
        {
            var original = Environment.GetEnvironmentVariable("DOTNET_ROOT");

            try
            {
                Environment.SetEnvironmentVariable("DOTNET_ROOT", Path.Combine(_tempDir, "not-dotnet"));

                var dotnetRoot = SdkTemplateHost.FindDotnetRoot();

                Directory.Exists(Path.Combine(dotnetRoot, "templates")).Should().BeTrue();
            }
            finally
            {
                Environment.SetEnvironmentVariable("DOTNET_ROOT", original);
            }
        }

        [TestMethod]
        public void FindDotnetRoot_WithNothingDiscoverable_ShouldThrowDirectoryNotFoundException()
        {
            var originalRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
            var originalHostPath = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
            var originalPath = Environment.GetEnvironmentVariable("PATH");

            try
            {
                Environment.SetEnvironmentVariable("DOTNET_ROOT", null);
                Environment.SetEnvironmentVariable("DOTNET_HOST_PATH", null);
                Environment.SetEnvironmentVariable("PATH", _tempDir);

                Action act = () => SdkTemplateHost.FindDotnetRoot();

                act.Should().Throw<DirectoryNotFoundException>().WithMessage("*DOTNET_ROOT*");
            }
            finally
            {
                Environment.SetEnvironmentVariable("DOTNET_ROOT", originalRoot);
                Environment.SetEnvironmentVariable("DOTNET_HOST_PATH", originalHostPath);
                Environment.SetEnvironmentVariable("PATH", originalPath);
            }
        }

        #endregion

        #region SdkVersion Tests

        [TestMethod]
        [DataRow("microsoft.dotnet.common.projecttemplates.10.0.10.0.401.nupkg", "10.0.401")]
        [DataRow("microsoft.dotnet.common.projecttemplates.11.0.11.0.100-rc.1.26425.128.nupkg", "11.0.100-rc.1.26425.128")]
        [DataRow("Microsoft.DotNet.Common.ProjectTemplates.10.0.10.0.103.nupkg", "10.0.103")]
        public void SdkVersion_ShouldComeFromTheCommonProjectTemplatesPackage(string packageName, string expected)
        {
            var folder = CreateVersionFolder(_tempDir, "10.0.12", withPackage: false);
            File.WriteAllText(Path.Combine(folder, packageName), string.Empty);
            File.WriteAllText(Path.Combine(folder, "microsoft.dotnet.web.projecttemplates.10.0.10.0.12.nupkg"), string.Empty);

            using var host = new SdkTemplateHost(_tempDir, "net10.0");

            host.SdkVersion.Should().Be(expected);
        }

        [TestMethod]
        public void SdkVersion_WithoutCommonProjectTemplatesPackage_ShouldBeNull()
        {
            CreateVersionFolder(_tempDir, "10.0.12");

            using var host = new SdkTemplateHost(_tempDir, "net10.0");

            host.SdkVersion.Should().BeNull();
        }

        [TestMethod]
        public void SdkVersion_WithoutTemplateFolder_ShouldBeNull()
        {
            using var host = new SdkTemplateHost(Path.Combine(_tempDir, "missing"), "net10.0");

            host.SdkVersion.Should().BeNull();
        }

        [TestMethod]
        public void SdkVersion_OnThisMachine_ShouldMatchAnInstalledSdk()
        {
            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), "net10.0");
            if (host.TemplateFolder is null)
            {
                Assert.Inconclusive("No templates folder for net10.0 is installed on this machine.");
            }

            host.SdkVersion.Should().StartWith("10.0.");
            Directory.Exists(Path.Combine(SdkTemplateHost.FindDotnetRoot(), "sdk", host.SdkVersion)).Should().BeTrue();
        }

        #endregion

        #region GetMissingTemplatesAsync Tests

        [TestMethod]
        public async Task GetMissingTemplatesAsync_WithNullShortNames_ShouldThrowArgumentNullException()
        {
            using var host = new SdkTemplateHost(_tempDir, "net10.0");

            var act = () => host.GetMissingTemplatesAsync(null);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task GetMissingTemplatesAsync_WithoutTemplateFolder_ShouldReturnEveryShortName()
        {
            using var host = new SdkTemplateHost(Path.Combine(_tempDir, "missing"), "net10.0");

            var missing = await host.GetMissingTemplatesAsync(["classlib", "web", "console"]);

            missing.Should().Equal("classlib", "web", "console");
        }

        [TestMethod]
        public async Task GetMissingTemplatesAsync_OnThisMachine_ShouldFindTheBuiltInTemplates()
        {
            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), "net10.0");
            if (host.TemplateFolder is null)
            {
                Assert.Inconclusive("No templates folder for net10.0 is installed on this machine.");
            }

            var missing = await host.GetMissingTemplatesAsync(["classlib", "web", "console", "not-a-real-template"]);

            missing.Should().Equal("not-a-real-template");
        }

        #endregion

        #region CreateProjectAsync Tests

        [TestMethod]
        public async Task CreateProjectAsync_WithNullShortName_ShouldThrowArgumentException()
        {
            using var host = new SdkTemplateHost(_tempDir, "net10.0");

            var act = () => host.CreateProjectAsync(null, "Contoso.Core", Path.Combine(_tempDir, "out"));

            await act.Should().ThrowAsync<ArgumentException>().WithParameterName("shortName");
        }

        [TestMethod]
        public async Task CreateProjectAsync_WithNullProjectName_ShouldThrowArgumentException()
        {
            using var host = new SdkTemplateHost(_tempDir, "net10.0");

            var act = () => host.CreateProjectAsync("classlib", null, Path.Combine(_tempDir, "out"));

            await act.Should().ThrowAsync<ArgumentException>().WithParameterName("projectName");
        }

        [TestMethod]
        public async Task CreateProjectAsync_WithNullOutputDirectory_ShouldThrowArgumentException()
        {
            using var host = new SdkTemplateHost(_tempDir, "net10.0");

            var act = () => host.CreateProjectAsync("classlib", "Contoso.Core", null);

            await act.Should().ThrowAsync<ArgumentException>().WithParameterName("outputDirectory");
        }

        [TestMethod]
        public async Task CreateProjectAsync_WithClasslib_ShouldWriteProjectFileForTargetFramework()
        {
            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), "net10.0");
            var outputDirectory = Path.Combine(_tempDir, "Contoso.Core");

            var projectFile = await host.CreateProjectAsync("classlib", "Contoso.Core", outputDirectory);

            projectFile.Should().Be(Path.Combine(outputDirectory, "Contoso.Core.csproj"));
            File.Exists(projectFile).Should().BeTrue();
            var content = await File.ReadAllTextAsync(projectFile);
            content.Should().Contain("<TargetFramework>net10.0</TargetFramework>");
            content.Should().Contain("Sdk=\"Microsoft.NET.Sdk\"");
        }

        [TestMethod]
        public async Task CreateProjectAsync_WithWeb_ShouldWriteWebSdkProject()
        {
            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), "net10.0");
            var outputDirectory = Path.Combine(_tempDir, "Contoso.Api");

            var projectFile = await host.CreateProjectAsync("web", "Contoso.Api", outputDirectory);

            var content = await File.ReadAllTextAsync(projectFile);
            content.Should().Contain("Sdk=\"Microsoft.NET.Sdk.Web\"");
            File.Exists(Path.Combine(outputDirectory, "Program.cs")).Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateProjectAsync_WithConsole_ShouldWriteExeProject()
        {
            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), "net10.0");
            var outputDirectory = Path.Combine(_tempDir, "Contoso.MessageBus.Runtime");

            var projectFile = await host.CreateProjectAsync("console", "Contoso.MessageBus.Runtime", outputDirectory);

            var content = await File.ReadAllTextAsync(projectFile);
            content.Should().Contain("<OutputType>Exe</OutputType>");
            content.Should().NotContain("Microsoft.NET.Sdk.Worker");
        }

        [TestMethod]
        public async Task CreateProjectAsync_CalledTwice_ShouldCreateBothProjects()
        {
            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), "net10.0");

            var first = await host.CreateProjectAsync("classlib", "Contoso.Core", Path.Combine(_tempDir, "Contoso.Core"));
            var second = await host.CreateProjectAsync("classlib", "Contoso.Data", Path.Combine(_tempDir, "Contoso.Data"));

            File.Exists(first).Should().BeTrue();
            File.Exists(second).Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateProjectAsync_WithEmptyTemplatesRoot_ShouldThrowNamingTheSearchedPath()
        {
            using var host = new SdkTemplateHost(_tempDir, "net10.0");
            var outputDirectory = Path.Combine(_tempDir, "Contoso.Core");

            var action = () => host.CreateProjectAsync("classlib", "Contoso.Core", outputDirectory);

            (await action.Should().ThrowAsync<TemplateNotFoundException>())
                .Which.Message.Should().Contain("classlib").And.Contain(_tempDir);
            Directory.Exists(outputDirectory).Should().BeFalse();
        }

        [TestMethod]
        public async Task CreateProjectAsync_WithUnknownShortName_ShouldThrowNamingTheVersionFolder()
        {
            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), "net10.0");
            var outputDirectory = Path.Combine(_tempDir, "Contoso.Nope");

            var action = () => host.CreateProjectAsync("easyaf-does-not-exist", "Contoso.Nope", outputDirectory);

            var exception = (await action.Should().ThrowAsync<TemplateNotFoundException>()).Which;
            exception.ShortName.Should().Be("easyaf-does-not-exist");
            exception.SearchedPath.Should().Be(host.TemplateFolder);
            Directory.Exists(outputDirectory).Should().BeFalse();
        }

        [TestMethod]
        public async Task CreateProjectAsync_WithNonEmptyOutputDirectory_ShouldRefuseAndLeaveFilesAlone()
        {
            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), "net10.0");
            var outputDirectory = Path.Combine(_tempDir, "Contoso.Core");
            Directory.CreateDirectory(outputDirectory);
            var existing = Path.Combine(outputDirectory, "README.md");
            await File.WriteAllTextAsync(existing, "keep me");

            var action = () => host.CreateProjectAsync("classlib", "Contoso.Core", outputDirectory);

            (await action.Should().ThrowAsync<InvalidOperationException>())
                .Which.Message.Should().Contain(outputDirectory);
            (await File.ReadAllTextAsync(existing)).Should().Be("keep me");
            File.Exists(Path.Combine(outputDirectory, "Contoso.Core.csproj")).Should().BeFalse();
        }

        [TestMethod]
        public async Task CreateProjectAsync_WithEmptyExistingOutputDirectory_ShouldCreateProject()
        {
            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), "net10.0");
            var outputDirectory = Path.Combine(_tempDir, "Contoso.Core");
            Directory.CreateDirectory(outputDirectory);

            var projectFile = await host.CreateProjectAsync("classlib", "Contoso.Core", outputDirectory);

            File.Exists(projectFile).Should().BeTrue();
        }

        [TestMethod]
        public async Task CreateProjectAsync_WithOverwrite_ShouldReplaceConflictingFile()
        {
            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), "net10.0");
            var outputDirectory = Path.Combine(_tempDir, "Contoso.Core");
            Directory.CreateDirectory(outputDirectory);
            var existing = Path.Combine(outputDirectory, "Contoso.Core.csproj");
            await File.WriteAllTextAsync(existing, "<Project />");

            var projectFile = await host.CreateProjectAsync("classlib", "Contoso.Core", outputDirectory, overwrite: true);

            projectFile.Should().Be(existing);
            (await File.ReadAllTextAsync(existing)).Should().Contain("<TargetFramework>net10.0</TargetFramework>");
        }

        [TestMethod]
        public async Task CreateProjectAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
        {
            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), "net10.0");
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var action = () => host.CreateProjectAsync("classlib", "Contoso.Core", Path.Combine(_tempDir, "Contoso.Core"), cancellationToken: cts.Token);

            await action.Should().ThrowAsync<OperationCanceledException>();
        }

        #endregion

    }

}
