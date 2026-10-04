using CloudNimble.EasyAF.Tools.Commands;
using CloudNimble.EasyAF.Tools.Commands.Root;
using CloudNimble.EasyAF.Tools.Scaffolding;
using FluentAssertions;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.Tools
{

    /// <summary>
    /// Tests for <see cref="NewCommand"/>. These scaffold real solutions from the SDK templates installed on the machine, and are
    /// inconclusive when the templates for a target framework aren't installed. They check the files; they never restore or build.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class NewCommandTests
    {

        #region Fields

        private const string Namespace = "CloudNimble.Contoso";

        /// <summary>
        /// The .NET version <c>new</c> targets when <c>-f</c> isn't given.
        /// </summary>
        private const int DefaultDotNetVersion = 11;

        private static string _defaultRoot;
        private static int _defaultExitCode;
        private static string _defaultOutput;
        private static string _defaultConsole;

        private string _tempDir;

        #endregion

        #region Properties

        private static string DefaultSolution => Path.Combine(_defaultRoot, Namespace);

        #endregion

        #region Test Setup

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            if (!TemplatesInstalled(DefaultDotNetVersion))
            {
                return;
            }

            _defaultRoot = Path.Combine(Path.GetTempPath(), $"EasyAF_New_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_defaultRoot);
            _defaultOutput = Path.Combine(_defaultRoot, Namespace);
            (_defaultExitCode, _defaultConsole) = await RunAsync(new NewCommand { Name = Namespace, OutputDirectory = _defaultOutput });
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            DeleteDirectory(_defaultRoot);
        }

        [TestInitialize]
        public void Initialize()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"EasyAF_New_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DeleteDirectory(_tempDir);
        }

        #endregion

        #region Default Scaffold Tests

        [TestMethod]
        public void Default_ShouldSucceed()
        {
            RequireDefaultScaffold();

            _defaultExitCode.Should().Be(0, because: _defaultConsole);
        }

        [TestMethod]
        [DataRow("Core")]
        [DataRow("Data")]
        [DataRow("Business")]
        [DataRow("Api")]
        [DataRow("MessageBus.Core")]
        [DataRow("MessageBus.Dispatch")]
        [DataRow("MessageBus.Runtime")]
        [DataRow("Tests.Core")]
        [DataRow("Tests.Business")]
        [DataRow("Tests.Api")]
        public void Default_ShouldCreateEveryProject(string suffix)
        {
            RequireDefaultScaffold();

            File.Exists(ProjectPath(DefaultSolution, suffix)).Should().BeTrue();
        }

        [TestMethod]
        public async Task Default_Slnx_ShouldHaveTheFoldersBuildTypesAndProjects()
        {
            RequireDefaultScaffold();

            var model = await SolutionSerializers.SlnXml.OpenAsync(Path.Combine(DefaultSolution, $"{Namespace}.slnx"), default);

            model.SolutionFolders.Select(f => f.Path).Should().BeEquivalentTo("/Core/", "/Web/", "/MessageBus/", "/Tests/", "/Solution Items/");
            model.BuildTypes.Should().BeEquivalentTo("Debug", "Release", "DEV", "BETA", "PROD");
            model.SolutionProjects.Should().HaveCount(10);
            FindProject(model, "Api").Parent.Path.Should().Be("/Web/");
            FindProject(model, "MessageBus.Runtime").Parent.Path.Should().Be("/MessageBus/");
            FindProject(model, "Tests.Api").Parent.Path.Should().Be("/Tests/");
        }

        [TestMethod]
        [DataRow("Directory.Build.props")]
        [DataRow("Directory.Build.targets")]
        [DataRow("global.json")]
        [DataRow("DEV.runsettings")]
        public async Task Default_ShouldWriteSolutionItems(string fileName)
        {
            RequireDefaultScaffold();

            File.Exists(Path.Combine(DefaultSolution, fileName)).Should().BeTrue();
            var model = await SolutionSerializers.SlnXml.OpenAsync(Path.Combine(DefaultSolution, $"{Namespace}.slnx"), default);
            model.SolutionFolders.Single(f => f.Path == "/Solution Items/").Files.Should().Contain(fileName);
        }

        [TestMethod]
        [DataRow("Core", new string[0])]
        [DataRow("Data", new[] { "Core" })]
        [DataRow("Business", new[] { "Core", "Data" })]
        [DataRow("Api", new[] { "Business", "Data", "MessageBus.Core" })]
        [DataRow("MessageBus.Core", new[] { "Core" })]
        [DataRow("MessageBus.Dispatch", new[] { "Business", "MessageBus.Core" })]
        [DataRow("MessageBus.Runtime", new[] { "Business", "Data", "MessageBus.Core", "MessageBus.Dispatch" })]
        [DataRow("Tests.Core", new[] { "Core" })]
        [DataRow("Tests.Business", new[] { "Business", "Data" })]
        [DataRow("Tests.Api", new[] { "Api" })]
        public void Default_ProjectReferences_ShouldMatchTheGraph(string suffix, string[] expected)
        {
            RequireDefaultScaffold();

            ProjectReferences(DefaultSolution, suffix).Should().BeEquivalentTo(expected.Select(e => $@"..\{Namespace}.{e}\{Namespace}.{e}.csproj"));
        }

        [TestMethod]
        [DataRow("Core", "Core")]
        [DataRow("Data", "Data")]
        [DataRow("Business", "Business")]
        [DataRow("Api", "Api")]
        [DataRow("MessageBus.Core", "SimpleMessageBus")]
        [DataRow("MessageBus.Dispatch", null)]
        [DataRow("MessageBus.Runtime", null)]
        [DataRow("Tests.Core", null)]
        [DataRow("Tests.Business", null)]
        [DataRow("Tests.Api", null)]
        public void Default_EasyAFProjectType_ShouldOnlyBeSetOnLayerProjects(string suffix, string expected)
        {
            RequireDefaultScaffold();

            Property(DefaultSolution, suffix, "EasyAFProjectType").Should().Be(expected);
        }

        [TestMethod]
        public void Default_Api_ShouldReferenceODataMcpAndCallItInOrder()
        {
            RequireDefaultScaffold();

            PackageReferences(DefaultSolution, "Api").Should().Contain("Microsoft.OData.Mcp.AspNetCore");
            var program = File.ReadAllText(Path.Combine(DefaultSolution, $"{Namespace}.Api", "Program.cs"));
            var addMcp = program.IndexOf("//builder.Services.AddODataMcp();", StringComparison.Ordinal);
            var mapRoute = program.IndexOf("MapApiRoute<ContosoContextApi>", StringComparison.Ordinal);
            var useMcp = program.IndexOf("//app.UseODataMcp();", StringComparison.Ordinal);
            addMcp.Should().BePositive();
            mapRoute.Should().BeGreaterThan(addMcp);
            useMcp.Should().BeGreaterThan(mapRoute);
        }

        [TestMethod]
        public void Default_Runtime_ShouldBeAConsoleExeWithoutWebJobSettings()
        {
            RequireDefaultScaffold();

            var project = LoadProject(DefaultSolution, "MessageBus.Runtime");
            project.Root.Attribute("Sdk").Value.Should().Be("Microsoft.NET.Sdk");
            Property(DefaultSolution, "MessageBus.Runtime", "OutputType").Should().Be("Exe");
            Property(DefaultSolution, "MessageBus.Runtime", "IsWebJobProject").Should().BeNull();
            var runtimeFolder = Path.Combine(DefaultSolution, $"{Namespace}.MessageBus.Runtime");
            File.Exists(Path.Combine(runtimeFolder, "Worker.cs")).Should().BeFalse();
            File.Exists(Path.Combine(runtimeFolder, "Properties", "webjobs-publish-settings.json")).Should().BeFalse();
            File.ReadAllText(Path.Combine(runtimeFolder, "Properties", "launchSettings.json")).Should().Contain("\"DOTNET_ENVIRONMENT\": \"Development\"");
        }

        [TestMethod]
        public void Default_DirectoryBuildProps_ShouldHaveTheNamespaceAndAnalyzerGlobWithoutUserSecrets()
        {
            RequireDefaultScaffold();

            var props = File.ReadAllText(Path.Combine(DefaultSolution, "Directory.Build.props"));
            props.Should().Contain($"<EasyAFNamespace>{Namespace}</EasyAFNamespace>")
                .And.Contain($@"..\{Namespace}.Data\*.edmx")
                .And.NotContain("UserSecretsId")
                .And.NotContain("<DocumentationFile>", because: "AssemblyName isn't set yet when Directory.Build.props is evaluated");
            File.ReadAllText(ProjectPath(DefaultSolution, "Data")).Should().NotContain("UserSecretsId");
        }

        [TestMethod]
        [DataRow("Core", @"bin\Debug\net11.0\CloudNimble.Contoso.Core.xml")]
        [DataRow("MessageBus.Runtime", @"bin\Debug\net11.0\CloudNimble.Contoso.MessageBus.Runtime.xml")]
        [DataRow("Api", "")]
        [DataRow("Tests.Core", "")]
        public void Default_DocumentationFile_ShouldBeNamedAfterTheAssemblyByDirectoryBuildTargets(string suffix, string expected)
        {
            RequireDefaultScaffold();

            // RWM: Evaluate the real project, so this proves what MSBuild computes, not just what the file says.
            using var collection = new ProjectCollection();
            var project = collection.LoadProject(ProjectPath(DefaultSolution, suffix));

            project.GetPropertyValue("DocumentationFile").Should().Be(expected);
        }

        [TestMethod]
        public void Default_DocumentationFile_ShouldFollowAnAssemblyNameOverride()
        {
            RequireDefaultScaffold();

            using var collection = new ProjectCollection(new Dictionary<string, string> { ["AssemblyName"] = "Contoso.Renamed" });
            var project = collection.LoadProject(ProjectPath(DefaultSolution, "Core"));

            project.GetPropertyValue("DocumentationFile").Should().Be(@"bin\Debug\net11.0\Contoso.Renamed.xml");
        }

        [TestMethod]
        public void Default_GlobalJson_ShouldPinTheSdkThatSuppliedTheTemplates()
        {
            RequireDefaultScaffold();

            using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), $"net{DefaultDotNetVersion}.0");
            File.ReadAllText(Path.Combine(DefaultSolution, "global.json")).Should().Contain($"\"version\": \"{host.SdkVersion}\"");
        }

        [TestMethod]
        public void Default_Packages_ShouldFloatByFramework()
        {
            RequireDefaultScaffold();

            var versions = LoadProject(DefaultSolution, "Data").Descendants("PackageReference")
                .ToDictionary(p => (string)p.Attribute("Include"), p => (string)p.Attribute("Version"));
            // RWM: The range follows the tool running the test: "5.*" for a release build, "5.*-*" for a prerelease such as CI's.
            versions["EasyAF.Data.EFCore"].Should().Be($"{EasyAFRootCommand.Version.Major}.*{(EasyAFRootCommand.IsPrerelease ? "-*" : string.Empty)}");
            versions["Microsoft.EntityFrameworkCore.SqlServer"].Should().Be("11.*-*");
            Property(DefaultSolution, "Data", "TargetFramework").Should().Be("net11.0");
        }

        [TestMethod]
        public void Default_Api_ShouldHaveTheLocalDbConnectionStringInDevelopmentSettings()
        {
            RequireDefaultScaffold();

            File.ReadAllText(Path.Combine(DefaultSolution, $"{Namespace}.Api", "appsettings.Development.json"))
                .Should().Contain("\"Contoso\": \"Server=(localdb)");
        }

        [TestMethod]
        [DataRow("Api", "Program.cs", "public static class Program")]
        [DataRow("MessageBus.Runtime", "Program.cs", "UseSimpleMessageBusLifetime")]
        [DataRow("Tests.Core", "PlaceholderTests.cs", "public class PlaceholderTests")]
        [DataRow("Tests.Business", "BusinessTestBase.cs", "public class BusinessTestBase")]
        public void Default_ShouldWriteEasyAFFilesOverTheTemplates(string suffix, string file, string expected)
        {
            RequireDefaultScaffold();

            File.ReadAllText(Path.Combine(DefaultSolution, $"{Namespace}.{suffix}", file)).Should().Contain(expected);
        }

        [TestMethod]
        [DataRow("Tests.Core")]
        [DataRow("Tests.Business")]
        public void Default_ShouldRemoveClass1WhereEasyAFAddsCode(string suffix)
        {
            RequireDefaultScaffold();

            File.Exists(Path.Combine(DefaultSolution, $"{Namespace}.{suffix}", "Class1.cs")).Should().BeFalse();
        }

        [TestMethod]
        public void Default_ShouldNotLeaveAnyPlaceholders()
        {
            RequireDefaultScaffold();

            // RWM: The web template's .http file uses {{variables}} of its own, so only check the kinds of files EasyAF writes.
            string[] extensions = [".cs", ".csproj", ".json", ".props", ".targets", ".config", ".runsettings", ".slnx"];
            var withPlaceholders = Directory.EnumerateFiles(DefaultSolution, "*", SearchOption.AllDirectories)
                .Where(f => extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                .Where(f => File.ReadAllText(f).Contains("{{", StringComparison.Ordinal));
            withPlaceholders.Should().BeEmpty();
        }

        [TestMethod]
        public void Default_Output_ShouldPrintTheNextStepsWithoutAConnectionStringSource()
        {
            RequireDefaultScaffold();

            _defaultConsole.Should().Contain($"cd \"{_defaultOutput}\"")
                .And.Contain("dotnet easyaf init")
                .And.NotContain("init -c");
        }

        #endregion

        #region Option Tests

        [TestMethod]
        public async Task NoApi_ShouldOmitTheApiItsTestsAndTheWebFolder()
        {
            var output = await ScaffoldAsync(c => c.NoApi = true);

            Directory.Exists(Path.Combine(output, $"{Namespace}.Api")).Should().BeFalse();
            Directory.Exists(Path.Combine(output, $"{Namespace}.Tests.Api")).Should().BeFalse();
            var model = await SolutionSerializers.SlnXml.OpenAsync(Path.Combine(output, $"{Namespace}.slnx"), default);
            model.SolutionFolders.Select(f => f.Path).Should().NotContain("/Web/");
            model.SolutionProjects.Should().HaveCount(8);
        }

        [TestMethod]
        public async Task NoMessageBus_ShouldOmitTheMessageBusProjectsAndTheApiReference()
        {
            var output = await ScaffoldAsync(c => c.NoMessageBus = true);

            Directory.EnumerateDirectories(output, $"{Namespace}.MessageBus.*").Should().BeEmpty();
            ProjectReferences(output, "Api").Should().NotContain(r => r.Contains("MessageBus", StringComparison.Ordinal));
            var model = await SolutionSerializers.SlnXml.OpenAsync(Path.Combine(output, $"{Namespace}.slnx"), default);
            model.SolutionFolders.Select(f => f.Path).Should().NotContain("/MessageBus/");
        }

        [TestMethod]
        public async Task NoRuntime_ShouldOmitOnlyTheRuntime()
        {
            var output = await ScaffoldAsync(c => c.NoRuntime = true);

            Directory.Exists(Path.Combine(output, $"{Namespace}.MessageBus.Runtime")).Should().BeFalse();
            File.Exists(ProjectPath(output, "MessageBus.Core")).Should().BeTrue();
            File.Exists(ProjectPath(output, "MessageBus.Dispatch")).Should().BeTrue();
        }

        [TestMethod]
        public async Task WebJob_ShouldStampPublishMetadataAndWriteTheSettingsFile()
        {
            var output = await ScaffoldAsync(c => c.WebJob = true);

            Property(output, "MessageBus.Runtime", "IsWebJobProject").Should().Be("true");
            Property(output, "MessageBus.Runtime", "WebJobName").Should().Be("ContosoWebJobs");
            Property(output, "MessageBus.Runtime", "WebJobType").Should().Be("Continuous");
            Property(output, "MessageBus.Runtime", "RuntimeIdentifier").Should().BeNull();
            Property(output, "MessageBus.Runtime", "SelfContained").Should().BeNull();
            File.ReadAllText(Path.Combine(output, $"{Namespace}.MessageBus.Runtime", "Properties", "webjobs-publish-settings.json"))
                .Should().Contain("ContosoWebJobs").And.Contain("Continuous");
        }

        [TestMethod]
        public async Task Namespace_ShouldNameTheProjectsWhileNameNamesTheFolder()
        {
            RequireTemplates(DefaultDotNetVersion);
            var output = Path.Combine(_tempDir, "Contoso");

            var (exitCode, console) = await RunAsync(new NewCommand { Name = "Contoso", Namespace = Namespace, OutputDirectory = output });

            exitCode.Should().Be(0, because: console);
            File.Exists(Path.Combine(output, $"{Namespace}.slnx")).Should().BeTrue();
            File.Exists(ProjectPath(output, "Core")).Should().BeTrue();
        }

        [TestMethod]
        public async Task Framework_10_ShouldTargetNet10AndFloatMicrosoftPackagesTo10()
        {
            var output = await ScaffoldAsync(c => c.Framework = 10);

            Property(output, "Core", "TargetFramework").Should().Be("net10.0");
            LoadProject(output, "Data").Descendants("PackageReference")
                .Single(p => (string)p.Attribute("Include") == "Microsoft.EntityFrameworkCore.SqlServer")
                .Attribute("Version").Value.Should().Be("10.*");
        }

        [TestMethod]
        public async Task OutputDirectory_WhenOmitted_ShouldBeTheCurrentDirectory()
        {
            RequireTemplates(DefaultDotNetVersion);
            var originalDirectory = Environment.CurrentDirectory;
            try
            {
                Environment.CurrentDirectory = _tempDir;

                var (exitCode, console) = await RunAsync(new NewCommand { Name = Namespace });

                exitCode.Should().Be(0, because: console);
                File.Exists(Path.Combine(_tempDir, $"{Namespace}.slnx")).Should().BeTrue();
                File.Exists(ProjectPath(_tempDir, "Core")).Should().BeTrue();
                Directory.Exists(Path.Combine(_tempDir, Namespace)).Should().BeFalse();
                console.Should().NotContain("cd \"", because: "the solution is already in the current folder");
            }
            finally
            {
                Environment.CurrentDirectory = originalDirectory;
            }
        }

        [TestMethod]
        public async Task ExistingProjectDirectory_WhenAccepted_ShouldScaffoldOverIt()
        {
            RequireTemplates(DefaultDotNetVersion);
            var output = Path.Combine(_tempDir, Namespace);
            var existing = Path.Combine(output, $"{Namespace}.Core");
            Directory.CreateDirectory(existing);
            File.WriteAllText(Path.Combine(existing, "keep.txt"), "keep");
            string asked = null;

            var (exitCode, console) = await RunAsync(new NewCommand
            {
                Name = Namespace,
                OutputDirectory = output,
                ConfirmOverwrite = directory => { asked = directory; return true; },
            });

            exitCode.Should().Be(0, because: console);
            asked.Should().Be(existing);
            File.Exists(ProjectPath(output, "Core")).Should().BeTrue();
        }

        #endregion

        #region Failure Tests

        [TestMethod]
        [DataRow(true, false)]
        [DataRow(false, true)]
        public async Task WebJob_WithoutTheRuntime_ShouldFailWithoutWritingAnything(bool noRuntime, bool noMessageBus)
        {
            var output = Path.Combine(_tempDir, Namespace);

            var (exitCode, console) = await RunAsync(new NewCommand
            {
                Name = Namespace,
                OutputDirectory = output,
                WebJob = true,
                NoRuntime = noRuntime,
                NoMessageBus = noMessageBus,
            });

            exitCode.Should().Be(1);
            console.Should().Contain("--webjob");
            Directory.Exists(output).Should().BeFalse();
        }

        [TestMethod]
        [DataRow("Contoso.class")]
        [DataRow("Contoso.Tests")]
        [DataRow("1Contoso")]
        public async Task InvalidNamespace_ShouldFailWithoutWritingAnything(string name)
        {
            var output = Path.Combine(_tempDir, "out");

            var (exitCode, console) = await RunAsync(new NewCommand { Name = name, OutputDirectory = output });

            exitCode.Should().Be(1);
            console.Should().Contain(name);
            Directory.Exists(output).Should().BeFalse();
        }

        [TestMethod]
        [DataRow(8)]
        [DataRow(9)]
        [DataRow(12)]
        public async Task UnsupportedFramework_ShouldFailWithoutWritingAnything(int framework)
        {
            var output = Path.Combine(_tempDir, Namespace);

            var (exitCode, console) = await RunAsync(new NewCommand { Name = Namespace, OutputDirectory = output, Framework = framework });

            exitCode.Should().Be(1);
            console.Should().Contain($".NET {framework}");
            Directory.Exists(output).Should().BeFalse();
        }

        [TestMethod]
        [DataRow("Existing.slnx")]
        [DataRow("Existing.sln")]
        public async Task ExistingSolution_ShouldFailWithoutWritingAnything(string solutionFile)
        {
            var output = Path.Combine(_tempDir, Namespace);
            Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(output, solutionFile), string.Empty);

            var (exitCode, console) = await RunAsync(new NewCommand { Name = Namespace, OutputDirectory = output });

            exitCode.Should().Be(1);
            console.Should().Contain(solutionFile);
            Directory.EnumerateFileSystemEntries(output).Should().ContainSingle();
        }

        [TestMethod]
        public async Task MissingTemplates_ShouldFailNamingTheFolderWithoutWritingAnything()
        {
            var templatesRoot = Path.Combine(_tempDir, "templates");
            Directory.CreateDirectory(templatesRoot);
            var output = Path.Combine(_tempDir, Namespace);

            var (exitCode, console) = await RunAsync(new NewCommand { Name = Namespace, OutputDirectory = output, TemplatesRoot = templatesRoot });

            exitCode.Should().Be(1);
            console.Should().Contain(templatesRoot);
            Directory.Exists(output).Should().BeFalse();
        }

        [TestMethod]
        public async Task ExistingProjectDirectory_WhenDeclined_ShouldFailAndLeaveItAlone()
        {
            RequireTemplates(DefaultDotNetVersion);
            var output = Path.Combine(_tempDir, Namespace);
            var existing = Path.Combine(output, $"{Namespace}.Core");
            Directory.CreateDirectory(existing);
            File.WriteAllText(Path.Combine(existing, "keep.txt"), "keep");

            var (exitCode, _) = await RunAsync(new NewCommand { Name = Namespace, OutputDirectory = output, ConfirmOverwrite = _ => false });

            exitCode.Should().Be(1);
            Directory.EnumerateFileSystemEntries(output).Should().ContainSingle();
            Directory.EnumerateFileSystemEntries(existing).Should().ContainSingle();
        }

        [TestMethod]
        public async Task MissingName_ShouldFail()
        {
            var (exitCode, _) = await RunAsync(new NewCommand { Name = " ", OutputDirectory = Path.Combine(_tempDir, "out") });

            exitCode.Should().Be(1);
        }

        #endregion

        #region Helpers

        private static bool TemplatesInstalled(int dotNetVersion)
        {
            try
            {
                using var host = new SdkTemplateHost(SdkTemplateHost.FindTemplatesRoot(), $"net{dotNetVersion}.0");
                return host.TemplateFolder is not null;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
        }

        private static void RequireTemplates(int dotNetVersion)
        {
            if (!TemplatesInstalled(dotNetVersion))
            {
                Assert.Inconclusive($"No SDK templates for .NET {dotNetVersion} are installed on this machine.");
            }
        }

        private static void RequireDefaultScaffold()
        {
            RequireTemplates(DefaultDotNetVersion);
        }

        private async Task<string> ScaffoldAsync(Action<NewCommand> configure)
        {
            var output = Path.Combine(_tempDir, Namespace);
            var command = new NewCommand { Name = Namespace, OutputDirectory = output };
            configure(command);
            RequireTemplates(command.Framework);

            var (exitCode, console) = await RunAsync(command);

            exitCode.Should().Be(0, because: console);
            return output;
        }

        private static async Task<(int ExitCode, string Output)> RunAsync(NewCommand command)
        {
            var originalOut = Console.Out;
            var originalError = Console.Error;
            using var writer = new StringWriter();
            Console.SetOut(writer);
            Console.SetError(writer);
            try
            {
                var exitCode = await command.OnExecuteAsync();
                return (exitCode, writer.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }

        private static string ProjectPath(string solutionFolder, string suffix)
        {
            return Path.Combine(solutionFolder, $"{Namespace}.{suffix}", $"{Namespace}.{suffix}.csproj");
        }

        private static XDocument LoadProject(string solutionFolder, string suffix)
        {
            return XDocument.Load(ProjectPath(solutionFolder, suffix));
        }

        private static string Property(string solutionFolder, string suffix, string name)
        {
            return LoadProject(solutionFolder, suffix).Descendants(name).SingleOrDefault()?.Value;
        }

        private static string[] ProjectReferences(string solutionFolder, string suffix)
        {
            return [.. LoadProject(solutionFolder, suffix).Descendants("ProjectReference").Select(r => (string)r.Attribute("Include"))];
        }

        private static string[] PackageReferences(string solutionFolder, string suffix)
        {
            return [.. LoadProject(solutionFolder, suffix).Descendants("PackageReference").Select(r => (string)r.Attribute("Include"))];
        }

        private static SolutionProjectModel FindProject(SolutionModel model, string suffix)
        {
            return model.SolutionProjects.Single(p => p.FilePath.Replace('\\', '/') == $"{Namespace}.{suffix}/{Namespace}.{suffix}.csproj");
        }

        private static void DeleteDirectory(string path)
        {
            if (path is null || !Directory.Exists(path))
            {
                return;
            }

            try
            {
                Directory.Delete(path, recursive: true);
            }
            catch
            {
                // Best effort.
            }
        }

        #endregion

    }

}
