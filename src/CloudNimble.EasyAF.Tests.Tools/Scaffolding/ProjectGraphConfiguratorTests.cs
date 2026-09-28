using CloudNimble.EasyAF.Tools.Scaffolding;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.Tools.Scaffolding
{

    /// <summary>
    /// Unit tests for the <see cref="ProjectGraphConfigurator"/> class. Project files are created from the installed
    /// SDK's real templates so the tests exercise exactly what <c>dotnet easyaf new</c> edits.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class ProjectGraphConfiguratorTests
    {

        #region Fields

        private const string Namespace = "CloudNimble.Contoso";

        private SdkTemplateHost _templateHost;
        private string _tempDir;

        #endregion

        #region Test Setup

        /// <summary>
        /// Creates a temporary solution directory and a template host for each test.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"EasyAF_ProjectGraphConfigurator_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDir);
            _templateHost = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), "net10.0");
        }

        /// <summary>
        /// Disposes the template host and removes the temporary directory after each test.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            _templateHost?.Dispose();
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
        /// Creates the project described by <paramref name="project"/> from its SDK template.
        /// </summary>
        /// <param name="project">The project description.</param>
        /// <returns>The project file path.</returns>
        private Task<string> CreateFromTemplateAsync(ScaffoldProject project)
        {
            var name = project.GetName(Namespace);
            return _templateHost.CreateProjectAsync(project.TemplateShortName, name, Path.Combine(_tempDir, name));
        }

        /// <summary>
        /// Gets a project from the default layout, optionally with WebJob metadata.
        /// </summary>
        /// <param name="suffix">The project suffix.</param>
        /// <param name="webJob">Whether to build the layout with <c>--webjob</c>.</param>
        /// <returns>The project description.</returns>
        private static ScaffoldProject LayoutProject(string suffix, bool webJob = false)
        {
            return ScaffoldLayout.Create(new ScaffoldOptions(Namespace, webJob: webJob)).Single(p => p.Suffix == suffix);
        }

        /// <summary>
        /// Configures a layout project that has already been created from its template and returns the saved XML.
        /// </summary>
        /// <param name="project">The project description.</param>
        /// <returns>The configured project file.</returns>
        private async Task<XDocument> CreateAndConfigureAsync(ScaffoldProject project)
        {
            var path = await CreateFromTemplateAsync(project);
            new ProjectGraphConfigurator(_tempDir, Namespace).Configure(project);
            return XDocument.Load(path);
        }

        /// <summary>
        /// Gets the values of a property element anywhere in the project.
        /// </summary>
        private static string[] PropertyValues(XDocument document, string name)
        {
            return [.. document.Root.Elements("PropertyGroup").Elements(name).Select(e => e.Value)];
        }

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithMissingSolutionDirectory_ShouldThrowArgumentException()
        {
            Action act = () => new ProjectGraphConfigurator(null, Namespace);

            act.Should().Throw<ArgumentException>().WithParameterName("solutionDirectory");
        }

        [TestMethod]
        public void Constructor_WithMissingNamespace_ShouldThrowArgumentException()
        {
            Action act = () => new ProjectGraphConfigurator(_tempDir, " ");

            act.Should().Throw<ArgumentException>().WithParameterName("namespace");
        }

        #endregion

        #region Configure Tests

        [TestMethod]
        public void Configure_WithNullProject_ShouldThrowArgumentNullException()
        {
            var configurator = new ProjectGraphConfigurator(_tempDir, Namespace);

            Action act = () => configurator.Configure(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("project");
        }

        [TestMethod]
        public void Configure_WithMissingProjectFile_ShouldThrowFileNotFoundExceptionNamingIt()
        {
            var configurator = new ProjectGraphConfigurator(_tempDir, Namespace);

            Action act = () => configurator.Configure(LayoutProject("Core"));

            act.Should().Throw<FileNotFoundException>().WithMessage("*CloudNimble.Contoso.Core.csproj*");
        }

        [TestMethod]
        [DataRow("Core")]
        [DataRow("Api")]
        [DataRow("MessageBus.Runtime")]
        public async Task Configure_ShouldStripTemplateImplicitUsingsAndNullable(string suffix)
        {
            var project = LayoutProject(suffix);
            var path = await CreateFromTemplateAsync(project);
            (await File.ReadAllTextAsync(path)).Should().Contain("<ImplicitUsings>").And.Contain("<Nullable>", because: "the template adds them, which is why we strip them");

            new ProjectGraphConfigurator(_tempDir, Namespace).Configure(project);

            var document = XDocument.Load(path);
            PropertyValues(document, "ImplicitUsings").Should().BeEmpty();
            PropertyValues(document, "Nullable").Should().BeEmpty();
            PropertyValues(document, "TargetFramework").Should().Equal("net10.0");
        }

        [TestMethod]
        public async Task Configure_ShouldStampEasyAFProjectType()
        {
            var document = await CreateAndConfigureAsync(LayoutProject("Data"));

            PropertyValues(document, "EasyAFProjectType").Should().Equal("Data");
        }

        [TestMethod]
        public async Task Configure_WithoutProjectType_ShouldNotStampOne()
        {
            var document = await CreateAndConfigureAsync(LayoutProject("MessageBus.Dispatch"));

            PropertyValues(document, "EasyAFProjectType").Should().BeEmpty();
        }

        [TestMethod]
        public async Task Configure_ShouldAddPackagesWithVersionsAlphabetically()
        {
            var document = await CreateAndConfigureAsync(LayoutProject("Data"));

            var packages = document.Root.Elements("ItemGroup").Elements("PackageReference").ToList();
            packages.Select(p => p.Attribute("Include").Value).Should().Equal("EasyAF.Data.EFCore", "Microsoft.EntityFrameworkCore.SqlServer");
            packages.Select(p => (string)p.Attribute("Version") ?? p.Element("Version")?.Value).Should().Equal("5.*", "10.*");
        }

        [TestMethod]
        public async Task Configure_ShouldAddRelativeProjectReferencesAlphabetically()
        {
            var document = await CreateAndConfigureAsync(LayoutProject("MessageBus.Runtime"));

            document.Root.Elements("ItemGroup").Elements("ProjectReference").Select(r => r.Attribute("Include").Value).Should().Equal(
                @"..\CloudNimble.Contoso.Business\CloudNimble.Contoso.Business.csproj",
                @"..\CloudNimble.Contoso.Data\CloudNimble.Contoso.Data.csproj",
                @"..\CloudNimble.Contoso.MessageBus.Core\CloudNimble.Contoso.MessageBus.Core.csproj",
                @"..\CloudNimble.Contoso.MessageBus.Dispatch\CloudNimble.Contoso.MessageBus.Dispatch.csproj");
        }

        [TestMethod]
        public async Task Configure_ShouldPlaceReferenceGroupsFirstInPackageThenProjectOrder()
        {
            var document = await CreateAndConfigureAsync(LayoutProject("MessageBus.Runtime"));

            var elements = document.Root.Elements().ToList();
            var groups = document.Root.Elements("ItemGroup").ToList();
            elements.IndexOf(groups[0]).Should().Be(elements.FindLastIndex(e => e.Name == "PropertyGroup") + 1,
                because: "the reference ItemGroups come immediately after the leading PropertyGroup");
            groups[0].Elements().Should().OnlyContain(e => e.Name == "PackageReference");
            groups[1].Elements().Should().OnlyContain(e => e.Name == "ProjectReference");
            groups.Skip(2).SelectMany(g => g.Elements()).Should().NotContain(e => e.Name == "PackageReference" || e.Name == "ProjectReference");
        }

        [TestMethod]
        public async Task Configure_WithNoReferences_ShouldNotAddAnEmptyProjectReferenceGroup()
        {
            var document = await CreateAndConfigureAsync(LayoutProject("Core"));

            document.Root.Elements("ItemGroup").Should().ContainSingle().Which.Elements().Should().OnlyContain(e => e.Name == "PackageReference");
        }

        [TestMethod]
        public async Task Configure_ForWebProject_ShouldKeepTheWebSdk()
        {
            var document = await CreateAndConfigureAsync(LayoutProject("Api"));

            document.Root.Attribute("Sdk").Value.Should().Be("Microsoft.NET.Sdk.Web");
            PropertyValues(document, "EasyAFProjectType").Should().Equal("Api");
        }

        [TestMethod]
        public async Task Configure_ForRuntime_ShouldStayAPlainSdkExeAndCopyAppSettings()
        {
            var document = await CreateAndConfigureAsync(LayoutProject("MessageBus.Runtime"));

            document.Root.Attribute("Sdk").Value.Should().Be("Microsoft.NET.Sdk");
            PropertyValues(document, "OutputType").Should().Equal("Exe");
            var updates = document.Root.Elements("ItemGroup").Elements("None").ToList();
            updates.Select(n => n.Attribute("Update")?.Value).Should().Equal("appsettings.json", "appsettings.*.json");
            updates.Should().OnlyContain(n => n.Attribute("Include") == null);
            updates[0].Element("CopyToOutputDirectory").Value.Should().Be("Always");
            updates[1].Element("CopyToOutputDirectory").Value.Should().Be("Always");
            updates[1].Element("DependentUpon").Value.Should().Be("appsettings.json");
        }

        [TestMethod]
        public async Task Configure_ForRuntimeWithoutWebJob_ShouldNotStampWebJobProperties()
        {
            var document = await CreateAndConfigureAsync(LayoutProject("MessageBus.Runtime"));

            PropertyValues(document, "IsWebJobProject").Should().BeEmpty();
            PropertyValues(document, "WebJobName").Should().BeEmpty();
        }

        [TestMethod]
        public async Task Configure_ForRuntimeWithWebJob_ShouldStampPublishMetadataOnly()
        {
            var document = await CreateAndConfigureAsync(LayoutProject("MessageBus.Runtime", webJob: true));

            PropertyValues(document, "IsWebJobProject").Should().Equal("true");
            PropertyValues(document, "WebJobName").Should().Equal("ContosoWebJobs");
            PropertyValues(document, "WebJobType").Should().Equal("Continuous");
            PropertyValues(document, "RuntimeIdentifier").Should().BeEmpty();
            PropertyValues(document, "SelfContained").Should().BeEmpty();
            document.Root.Attribute("Sdk").Value.Should().Be("Microsoft.NET.Sdk");
        }

        #endregion

    }

}
