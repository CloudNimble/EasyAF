using CloudNimble.EasyAF.Tools.Scaffolding;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.Tools.Scaffolding
{

    /// <summary>
    /// Unit tests for the <see cref="ScaffoldLayout"/> class, which encodes the project graph from the plan.
    /// </summary>
    [TestClass]
    public class ScaffoldLayoutTests
    {

        #region Test Helpers

        /// <summary>
        /// Creates the layout for the given options and indexes it by project suffix.
        /// </summary>
        /// <param name="options">The scaffold options; defaults to everything on net10.0.</param>
        /// <returns>The projects keyed by suffix.</returns>
        private static Dictionary<string, ScaffoldProject> Layout(ScaffoldOptions options = null)
        {
            return ScaffoldLayout.Create(options ?? new ScaffoldOptions("CloudNimble.Contoso")).ToDictionary(p => p.Suffix);
        }

        /// <summary>
        /// Gets the version a project declares for a package, or <see langword="null"/> if it does not reference it.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="packageId">The package id.</param>
        /// <returns>The version, or <see langword="null"/>.</returns>
        private static string PackageVersion(ScaffoldProject project, string packageId)
        {
            return project.Packages.SingleOrDefault(p => p.Id == packageId)?.Version;
        }

        #endregion

        #region Create Tests

        [TestMethod]
        public void Create_WithNullOptions_ShouldThrowArgumentNullException()
        {
            Action act = () => ScaffoldLayout.Create(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("options");
        }

        [TestMethod]
        public void Create_WithDefaults_ShouldProduceTheFullProjectSetInSolutionOrder()
        {
            var projects = ScaffoldLayout.Create(new ScaffoldOptions("CloudNimble.Contoso"));

            projects.Select(p => p.Suffix).Should().Equal(
                "Core", "Data", "Business",
                "Api",
                "MessageBus.Core", "MessageBus.Dispatch", "MessageBus.Runtime",
                "Tests.Core", "Tests.Business", "Tests.Api");
        }

        [TestMethod]
        public void Create_ShouldAssignSolutionFolders()
        {
            var layout = Layout();

            layout["Core"].SolutionFolder.Should().Be("Core");
            layout["Data"].SolutionFolder.Should().Be("Core");
            layout["Business"].SolutionFolder.Should().Be("Core");
            layout["Api"].SolutionFolder.Should().Be("Web");
            layout["MessageBus.Core"].SolutionFolder.Should().Be("MessageBus");
            layout["MessageBus.Dispatch"].SolutionFolder.Should().Be("MessageBus");
            layout["MessageBus.Runtime"].SolutionFolder.Should().Be("MessageBus");
            layout["Tests.Core"].SolutionFolder.Should().Be("Tests");
            layout["Tests.Business"].SolutionFolder.Should().Be("Tests");
            layout["Tests.Api"].SolutionFolder.Should().Be("Tests");
        }

        [TestMethod]
        public void Create_ShouldUseWebForApiConsoleForRuntimeAndClasslibForEverythingElse()
        {
            var layout = Layout();

            layout["Api"].TemplateShortName.Should().Be("web");
            layout["MessageBus.Runtime"].TemplateShortName.Should().Be("console");
            layout.Values.Where(p => p.Suffix is not "Api" and not "MessageBus.Runtime")
                .Should().OnlyContain(p => p.TemplateShortName == "classlib");
            layout.Values.Should().NotContain(p => p.TemplateShortName == "worker");
        }

        [TestMethod]
        public void Create_ShouldSetEasyAFProjectTypesOnlyOnCoreDataBusinessApiAndMessageBusCore()
        {
            var layout = Layout();

            layout["Core"].EasyAFProjectType.Should().Be("Core");
            layout["Data"].EasyAFProjectType.Should().Be("Data");
            layout["Business"].EasyAFProjectType.Should().Be("Business");
            layout["Api"].EasyAFProjectType.Should().Be("Api");
            layout["MessageBus.Core"].EasyAFProjectType.Should().Be("SimpleMessageBus");
            layout["MessageBus.Dispatch"].EasyAFProjectType.Should().BeNull();
            layout["MessageBus.Runtime"].EasyAFProjectType.Should().BeNull();
            layout.Values.Where(p => p.Suffix.StartsWith("Tests.")).Should().OnlyContain(p => p.EasyAFProjectType == null);
        }

        [TestMethod]
        public void Create_ShouldReferenceProjectsPerTheGraph()
        {
            var layout = Layout();

            layout["Core"].ProjectReferences.Should().BeEmpty();
            layout["Data"].ProjectReferences.Should().Equal("Core");
            layout["Business"].ProjectReferences.Should().Equal("Core", "Data");
            layout["Api"].ProjectReferences.Should().Equal("Business", "Data", "MessageBus.Core");
            layout["MessageBus.Core"].ProjectReferences.Should().Equal("Core");
            layout["MessageBus.Dispatch"].ProjectReferences.Should().Equal("Business", "MessageBus.Core");
            layout["MessageBus.Runtime"].ProjectReferences.Should().Equal("Business", "Data", "MessageBus.Core", "MessageBus.Dispatch");
            layout["Tests.Core"].ProjectReferences.Should().Equal("Core");
            layout["Tests.Business"].ProjectReferences.Should().Equal("Business", "Data");
            layout["Tests.Api"].ProjectReferences.Should().Equal("Api");
        }

        [TestMethod]
        public void Create_ShouldOnlyReferenceProjectsThatExist()
        {
            foreach (var options in new[]
            {
                new ScaffoldOptions("Contoso"),
                new ScaffoldOptions("Contoso", includeApi: false),
                new ScaffoldOptions("Contoso", includeMessageBus: false),
                new ScaffoldOptions("Contoso", includeRuntime: false),
            })
            {
                var layout = Layout(options);

                layout.Values.SelectMany(p => p.ProjectReferences).Should().OnlyContain(r => layout.ContainsKey(r));
            }
        }

        [TestMethod]
        public void Create_ShouldListPackagesAndReferencesAlphabetically()
        {
            foreach (var project in Layout().Values)
            {
                project.Packages.Select(p => p.Id).Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase, because: project.Suffix);
                project.ProjectReferences.Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase, because: project.Suffix);
            }
        }

        [TestMethod]
        public void Create_ShouldPinEasyAFAndSimpleMessageBusMajors()
        {
            var layout = Layout();

            PackageVersion(layout["Core"], "EasyAF.Core").Should().Be("5.*");
            PackageVersion(layout["Data"], "EasyAF.Data.EFCore").Should().Be("5.*");
            PackageVersion(layout["Business"], "EasyAF.Business.EFCore").Should().Be("5.*");
            PackageVersion(layout["Api"], "EasyAF.Restier.EFCore").Should().Be("5.*");
            PackageVersion(layout["MessageBus.Core"], "SimpleMessageBus.Core").Should().Be("6.*");
            PackageVersion(layout["MessageBus.Dispatch"], "SimpleMessageBus.Dispatch").Should().Be("6.*");
        }

        [TestMethod]
        [DataRow("net10.0", "10.*")]
        [DataRow("net11.0", "11.*-*")]
        public void Create_ShouldFloatMicrosoftPackagesByTargetFramework(string targetFramework, string expected)
        {
            var layout = Layout(new ScaffoldOptions("Contoso", targetFramework));

            PackageVersion(layout["Data"], "Microsoft.EntityFrameworkCore.SqlServer").Should().Be(expected);
            PackageVersion(layout["Api"], "Microsoft.EntityFrameworkCore.SqlServer").Should().Be(expected);
        }

        [TestMethod]
        public void Create_ShouldGiveTheApiRestierMcpAndBothPublishers()
        {
            var api = Layout()["Api"];

            api.Packages.Select(p => p.Id).Should().Contain(
                "EasyAF.Restier.EFCore",
                "Microsoft.EntityFrameworkCore.SqlServer",
                "Microsoft.OData.Mcp.AspNetCore",
                "Microsoft.Restier.AspNetCore",
                "SimpleMessageBus.Publish",
                "SimpleMessageBus.Publish.Azure");
            PackageVersion(api, "Microsoft.OData.Mcp.AspNetCore").Should().Be("1.*-*");
        }

        [TestMethod]
        public void Create_ShouldGiveTheRuntimeHostingAndBothQueueProcessors()
        {
            var runtime = Layout()["MessageBus.Runtime"];

            runtime.Packages.Select(p => p.Id).Should().BeEquivalentTo(
                "SimpleMessageBus.Dispatch.Azure",
                "SimpleMessageBus.Dispatch.FileSystem",
                "SimpleMessageBus.Hosting");
            runtime.Packages.Select(p => p.Id).Should().NotContain(id => id.StartsWith("Microsoft.Azure.WebJobs") || id.Contains("ApplicationInsights"));
            runtime.CopyAppSettingsToOutput.Should().BeTrue();
        }

        [TestMethod]
        public void Create_ShouldGiveTestProjectsBreakdance()
        {
            foreach (var test in Layout().Values.Where(p => p.Suffix.StartsWith("Tests.")))
            {
                PackageVersion(test, "Breakdance.Assemblies").Should().Be("8.*-*", because: test.Suffix);
                PackageVersion(test, "Breakdance.Extensions.MSTest2").Should().Be("8.*-*", because: test.Suffix);
            }
        }

        [TestMethod]
        public void Create_ShouldOnlyCopyAppSettingsForTheRuntime()
        {
            Layout().Values.Where(p => p.CopyAppSettingsToOutput).Select(p => p.Suffix).Should().Equal("MessageBus.Runtime");
        }

        [TestMethod]
        public void Create_WithoutWebJob_ShouldNotStampWebJobProperties()
        {
            Layout().Values.Should().OnlyContain(p => p.Properties.Count == 0);
        }

        [TestMethod]
        public void Create_WithWebJob_ShouldStampAlexisWebJobPropertiesOnRuntimeOnly()
        {
            var layout = Layout(new ScaffoldOptions("CloudNimble.Contoso", webJob: true));

            layout["MessageBus.Runtime"].Properties.Should().BeEquivalentTo(new Dictionary<string, string>
            {
                ["IsWebJobProject"] = "true",
                ["WebJobName"] = "ContosoWebJobs",
                ["WebJobType"] = "Continuous",
            });
            layout["MessageBus.Runtime"].Properties.Keys.Should().NotContain(new[] { "RuntimeIdentifier", "SelfContained" });
            layout.Values.Where(p => p.Suffix != "MessageBus.Runtime").Should().OnlyContain(p => p.Properties.Count == 0);
        }

        [TestMethod]
        public void Create_WithNoApi_ShouldOmitApiAndItsTests()
        {
            var layout = Layout(new ScaffoldOptions("Contoso", includeApi: false));

            layout.Keys.Should().NotContain(new[] { "Api", "Tests.Api" });
            layout.Keys.Should().Contain("MessageBus.Runtime");
        }

        [TestMethod]
        public void Create_WithNoMessageBus_ShouldOmitAllThreeAndDropTheApiReference()
        {
            var layout = Layout(new ScaffoldOptions("Contoso", includeMessageBus: false));

            layout.Keys.Should().NotContain(k => k.StartsWith("MessageBus."));
            layout["Api"].ProjectReferences.Should().Equal("Business", "Data");
            layout["Api"].Packages.Select(p => p.Id).Should().Contain("SimpleMessageBus.Publish.Azure", because: "EasyAF managers always need an IMessagePublisher");
        }

        [TestMethod]
        public void Create_WithNoRuntime_ShouldOmitOnlyTheRuntime()
        {
            var layout = Layout(new ScaffoldOptions("Contoso", includeRuntime: false));

            layout.Keys.Should().NotContain("MessageBus.Runtime");
            layout.Keys.Should().Contain(new[] { "MessageBus.Core", "MessageBus.Dispatch" });
        }

        [TestMethod]
        public void Create_WithNoApiAndNoMessageBus_ShouldLeaveCoreAndTests()
        {
            var layout = Layout(new ScaffoldOptions("Contoso", includeApi: false, includeMessageBus: false));

            layout.Keys.Should().BeEquivalentTo("Core", "Data", "Business", "Tests.Core", "Tests.Business");
        }

        #endregion

        #region ScaffoldProject Tests

        [TestMethod]
        public void GetName_ShouldPrefixTheNamespace()
        {
            var api = Layout()["Api"];

            api.GetName("CloudNimble.Contoso").Should().Be("CloudNimble.Contoso.Api");
        }

        #endregion

    }

}
