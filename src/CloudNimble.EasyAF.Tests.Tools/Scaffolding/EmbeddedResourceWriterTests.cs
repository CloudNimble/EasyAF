using CloudNimble.EasyAF.Tools.Scaffolding;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.Tools.Scaffolding
{

    /// <summary>
    /// Unit tests for the <see cref="EmbeddedResourceWriter"/> class. These tests read the test-only resources embedded
    /// in this assembly under <c>TestResources/</c> (see the test project file).
    /// </summary>
    [TestClass]
    public class EmbeddedResourceWriterTests
    {

        #region Fields

        private const string TestRoot = "TestResources";

        private static readonly Assembly TestAssembly = typeof(EmbeddedResourceWriterTests).Assembly;

        private static readonly Dictionary<string, string> Tokens = new()
        {
            ["Name"] = "World",
            ["Namespace"] = "CloudNimble",
            ["Product"] = "Contoso",
        };

        private string _tempDir;

        #endregion

        #region Test Setup

        /// <summary>
        /// Creates a temporary output directory for each test.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"EasyAF_EmbeddedResourceWriter_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDir);
        }

        /// <summary>
        /// Removes the temporary output directory after each test.
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
        /// Creates a writer over this assembly's test resources.
        /// </summary>
        /// <param name="tokens">The tokens to use; defaults to <see cref="Tokens"/>.</param>
        /// <returns>The writer.</returns>
        private static EmbeddedResourceWriter CreateWriter(IReadOnlyDictionary<string, string> tokens = null)
        {
            return new EmbeddedResourceWriter(tokens ?? Tokens, TestAssembly, TestRoot);
        }

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullTokens_ShouldThrowArgumentNullException()
        {
            Action act = () => new EmbeddedResourceWriter(null, TestAssembly, TestRoot);

            act.Should().Throw<ArgumentNullException>().WithParameterName("tokens");
        }

        [TestMethod]
        public void Constructor_WithNullAssembly_ShouldThrowArgumentNullException()
        {
            Action act = () => new EmbeddedResourceWriter(Tokens, null, TestRoot);

            act.Should().Throw<ArgumentNullException>().WithParameterName("assembly");
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void Constructor_WithMissingRootFolder_ShouldThrowArgumentException(string rootFolder)
        {
            Action act = () => new EmbeddedResourceWriter(Tokens, TestAssembly, rootFolder);

            act.Should().Throw<ArgumentException>().WithParameterName("rootFolder");
        }

        [TestMethod]
        public void Constructor_WithDefaultOverload_ShouldReadTheToolsAssembly()
        {
            var writer = new EmbeddedResourceWriter(Tokens);

            writer.Assembly.Should().BeSameAs(typeof(EmbeddedResourceWriter).Assembly);
            writer.RootFolder.Should().Be("Resources");
        }

        [TestMethod]
        public void Constructor_ShouldCopyTokensSoLaterChangesDoNotLeakIn()
        {
            var tokens = new Dictionary<string, string> { ["Product"] = "Contoso" };
            var writer = CreateWriter(tokens);

            tokens["Product"] = "Changed";

            writer.Tokens["Product"].Should().Be("Contoso");
        }

        #endregion

        #region ResourceNames Tests

        [TestMethod]
        public void ResourceNames_ShouldListResourcesUnderRootWithForwardSlashes()
        {
            var writer = CreateWriter();

            writer.ResourceNames.Should().BeEquivalentTo(
                "MixedLineEndings.txt",
                "Nested/Folder.Dotted/Settings.json",
                "Tokens.txt",
                "UnknownToken.txt");
        }

        [TestMethod]
        public void ResourceNames_ShouldNotIncludeResourcesOutsideRoot()
        {
            var writer = CreateWriter();

            writer.ResourceNames.Should().NotContain(n => n.Contains("Ignored"));
        }

        [TestMethod]
        public void ResourceNames_WithRootThatHasNoResources_ShouldBeEmpty()
        {
            var writer = new EmbeddedResourceWriter(Tokens, TestAssembly, "NoSuchRoot");

            writer.ResourceNames.Should().BeEmpty();
        }

        [TestMethod]
        public void ResourceNames_WithRootSurroundedBySlashes_ShouldStillMatch()
        {
            var writer = new EmbeddedResourceWriter(Tokens, TestAssembly, "/TestResources/");

            writer.ResourceNames.Should().Contain("Tokens.txt");
        }

        #endregion

        #region Render Tests

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void Render_WithMissingResourceName_ShouldThrowArgumentException(string resourceName)
        {
            var writer = CreateWriter();

            Action act = () => writer.Render(resourceName);

            act.Should().Throw<ArgumentException>().WithParameterName("resourceName");
        }

        [TestMethod]
        public void Render_WithUnknownResource_ShouldThrowFileNotFoundExceptionNamingIt()
        {
            var writer = CreateWriter();

            Action act = () => writer.Render("Nope.txt");

            act.Should().Throw<FileNotFoundException>().WithMessage("*Nope.txt*");
        }

        [TestMethod]
        public void Render_ShouldReplaceEveryTokenOccurrence()
        {
            var writer = CreateWriter();

            var result = writer.Render("Tokens.txt");

            result.Should().Be($"Hello World from Contoso.{Environment.NewLine}Namespace: CloudNimble.Contoso{Environment.NewLine}");
        }

        [TestMethod]
        public void Render_ShouldLeaveSingleBracesAlone()
        {
            var writer = CreateWriter();

            var result = writer.Render("Nested/Folder.Dotted/Settings.json");

            result.Should().Contain("\"static\": { \"value\": 1 }").And.Contain("\"product\": \"Contoso\"");
        }

        [TestMethod]
        public void Render_WithBackslashesOrDifferentCase_ShouldFindTheResource()
        {
            var writer = CreateWriter();

            var result = writer.Render(@"nested\folder.dotted\SETTINGS.json");

            result.Should().Contain("Contoso");
        }

        [TestMethod]
        public void Render_WithTokenMissingFromDictionary_ShouldThrowNamingTokenAndResource()
        {
            var writer = CreateWriter();

            Action act = () => writer.Render("UnknownToken.txt");

            act.Should().Throw<InvalidOperationException>().WithMessage("*{{Missing}}*UnknownToken.txt*");
        }

        [TestMethod]
        public void Render_WithTokenValueThatLooksLikeAToken_ShouldNotExpandItAgain()
        {
            var writer = CreateWriter(new Dictionary<string, string>
            {
                ["Name"] = "{{Product}}",
                ["Namespace"] = "N",
                ["Product"] = "P",
            });

            var result = writer.Render("Tokens.txt");

            result.Should().StartWith("Hello {{Product}} from P.");
        }

        [TestMethod]
        public void Render_ShouldNormalizeLineEndingsToEnvironmentNewLine()
        {
            var writer = CreateWriter();

            var result = writer.Render("MixedLineEndings.txt");

            result.Split(Environment.NewLine).Should().Equal("line one", "line two", "line three", "line four", string.Empty);
            result.Replace(Environment.NewLine, string.Empty).Should().NotContainAny("\r", "\n");
        }

        #endregion

        #region WriteAsync Tests

        [TestMethod]
        public async Task WriteAsync_WithMissingDestination_ShouldThrowArgumentException()
        {
            var writer = CreateWriter();

            var act = () => writer.WriteAsync("Tokens.txt", null);

            await act.Should().ThrowAsync<ArgumentException>().WithParameterName("destinationPath");
        }

        [TestMethod]
        public async Task WriteAsync_ShouldCreateParentDirectoriesAndReturnFullPath()
        {
            var writer = CreateWriter();
            var destination = Path.Combine(_tempDir, "a", "b", "Tokens.txt");

            var path = await writer.WriteAsync("Tokens.txt", destination);

            path.Should().Be(destination);
            (await File.ReadAllTextAsync(path)).Should().StartWith("Hello World from Contoso.");
        }

        [TestMethod]
        public async Task WriteAsync_WhenFileExists_ShouldOverwriteIt()
        {
            var writer = CreateWriter();
            var destination = Path.Combine(_tempDir, "Program.cs");
            await File.WriteAllTextAsync(destination, "Console.WriteLine(\"Hello, World!\");");

            await writer.WriteAsync("Tokens.txt", destination);

            (await File.ReadAllTextAsync(destination)).Should().StartWith("Hello World from Contoso.");
        }

        [TestMethod]
        public async Task WriteAsync_ShouldWriteUtf8WithoutByteOrderMark()
        {
            var writer = CreateWriter();
            var destination = Path.Combine(_tempDir, "Tokens.txt");

            await writer.WriteAsync("Tokens.txt", destination);

            var bytes = await File.ReadAllBytesAsync(destination);
            bytes[0].Should().Be((byte)'H');
        }

        [TestMethod]
        public async Task WriteAsync_WithUnknownToken_ShouldNotCreateTheFile()
        {
            var writer = CreateWriter();
            var destination = Path.Combine(_tempDir, "UnknownToken.txt");

            var act = () => writer.WriteAsync("UnknownToken.txt", destination);

            await act.Should().ThrowAsync<InvalidOperationException>();
            File.Exists(destination).Should().BeFalse();
        }

        #endregion

        #region Tools Resources Tests

        /// <summary>
        /// The tokens <c>dotnet easyaf new</c> supplies. Every placeholder in the Tools resources must be one of these.
        /// </summary>
        private static readonly Dictionary<string, string> ScaffoldTokens = new()
        {
            ["Company"] = "CloudNimble",
            ["EasyAFPackageVersion"] = "5.*",
            ["Namespace"] = "CloudNimble.Contoso",
            ["Product"] = "Contoso",
            ["SdkVersion"] = "10.0.401",
            ["Year"] = "2026",
        };

        [TestMethod]
        public void ToolsResources_ShouldContainEveryScaffoldFile()
        {
            var writer = new EmbeddedResourceWriter(ScaffoldTokens);

            writer.ResourceNames.Should().BeEquivalentTo(
                "Api/appsettings.BETA.json",
                "Api/appsettings.DEV.json",
                "Api/appsettings.Development.json",
                "Api/appsettings.json",
                "Api/appsettings.PROD.json",
                "Api/Program.cs",
                "DEV.runsettings",
                "Directory.Build.props",
                "Directory.Build.targets",
                "global.json",
                "MessageBus.Runtime/appsettings.BETA.json",
                "MessageBus.Runtime/appsettings.DEV.json",
                "MessageBus.Runtime/appsettings.Development.json",
                "MessageBus.Runtime/appsettings.json",
                "MessageBus.Runtime/appsettings.PROD.json",
                "MessageBus.Runtime/Program.cs",
                "MessageBus.Runtime/Properties/launchSettings.json",
                "MessageBus.Runtime/Properties/webjobs-publish-settings.json",
                "nuget.config",
                "Tests.Business/BusinessTestBase.cs",
                "Tests.Core/PlaceholderTests.cs");
        }

        [TestMethod]
        public void ToolsResources_ShouldRenderWithOnlyTheScaffoldTokens()
        {
            var writer = new EmbeddedResourceWriter(ScaffoldTokens);

            foreach (var name in writer.ResourceNames)
            {
                Action act = () => writer.Render(name);

                act.Should().NotThrow(because: $"'{name}' should only use the documented scaffold tokens");
            }
        }

        [TestMethod]
        public void ToolsResources_JsonFiles_ShouldParseAfterRendering()
        {
            var writer = new EmbeddedResourceWriter(ScaffoldTokens);

            foreach (var name in writer.ResourceNames.Where(n => n.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
            {
                Action act = () => JsonDocument.Parse(writer.Render(name)).Dispose();

                act.Should().NotThrow(because: $"'{name}' should be valid JSON");
            }
        }

        [TestMethod]
        public void ToolsResources_XmlFiles_ShouldParseAfterRendering()
        {
            var writer = new EmbeddedResourceWriter(ScaffoldTokens);

            foreach (var name in writer.ResourceNames.Where(n => n.EndsWith(".props", StringComparison.OrdinalIgnoreCase) ||
                                                                 n.EndsWith(".config", StringComparison.OrdinalIgnoreCase) ||
                                                                 n.EndsWith(".runsettings", StringComparison.OrdinalIgnoreCase)))
            {
                Action act = () => XDocument.Parse(writer.Render(name));

                act.Should().NotThrow(because: $"'{name}' should be valid XML");
            }
        }

        [TestMethod]
        public void ToolsResources_DirectoryBuildProps_ShouldNotDeclareUserSecretsId()
        {
            var writer = new EmbeddedResourceWriter(ScaffoldTokens);

            writer.Render("Directory.Build.props").Should().NotContain("UserSecretsId");
        }

        [TestMethod]
        public void ToolsResources_AppSettingsJson_ShouldNotDeclareConnectionStrings()
        {
            var writer = new EmbeddedResourceWriter(ScaffoldTokens);

            writer.Render("Api/appsettings.json").Should().NotContain("ConnectionStrings");
            writer.Render("MessageBus.Runtime/appsettings.json").Should().NotContain("ConnectionStrings");
        }

        [TestMethod]
        public void ToolsResources_ApiProgram_ShouldMapMcpAfterRestier()
        {
            var writer = new EmbeddedResourceWriter(ScaffoldTokens);

            var program = writer.Render("Api/Program.cs");

            var addMcp = program.IndexOf("AddODataMcp()", StringComparison.Ordinal);
            var mapRestier = program.IndexOf("MapApiRoute<ContosoContextApi>", StringComparison.Ordinal);
            var useMcp = program.IndexOf("UseODataMcp()", StringComparison.Ordinal);
            addMcp.Should().BePositive();
            mapRestier.Should().BeGreaterThan(addMcp);
            useMcp.Should().BeGreaterThan(mapRestier);
            program.Should().NotContain("MapMcp(");
        }

        #endregion

    }

}
