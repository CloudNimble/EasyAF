using CloudNimble.EasyAF.Tools.Scaffolding;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CloudNimble.EasyAF.Tests.Tools.Scaffolding
{

    /// <summary>
    /// Unit tests for the <see cref="ScaffoldOptions"/> class.
    /// </summary>
    [TestClass]
    public class ScaffoldOptionsTests
    {

        #region Fields

        private static readonly Version ToolVersion = new(5, 0, 0);

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullToolVersion_ShouldThrowArgumentNullException()
        {
            Action act = () => new ScaffoldOptions("Contoso", null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("toolVersion");
        }

        [TestMethod]
        public void Constructor_WithDefaults_ShouldIncludeEverythingOnNet11()
        {
            var options = new ScaffoldOptions("CloudNimble.Contoso", ToolVersion);

            options.DotNetVersion.Should().Be(11);
            options.TargetFramework.Should().Be("net11.0");
            options.IncludeApi.Should().BeTrue();
            options.IncludeMessageBus.Should().BeTrue();
            options.IncludeRuntime.Should().BeTrue();
            options.WebJob.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void Constructor_WithMissingNamespace_ShouldThrowArgumentException(string @namespace)
        {
            Action act = () => new ScaffoldOptions(@namespace, ToolVersion);

            act.Should().Throw<ArgumentException>().WithParameterName("namespace");
        }

        [TestMethod]
        [DataRow("Contoso..App")]
        [DataRow(".Contoso")]
        [DataRow("Contoso.")]
        [DataRow("my-app")]
        [DataRow("Contoso.1App")]
        [DataRow("Contoso.class")]
        [DataRow("Contoso App")]
        public void Constructor_WithInvalidNamespace_ShouldThrowNamingTheNamespace(string @namespace)
        {
            Action act = () => new ScaffoldOptions(@namespace, ToolVersion);

            act.Should().Throw<ArgumentException>().WithParameterName("namespace").WithMessage($"*'{@namespace}'*");
        }

        [TestMethod]
        [DataRow("Contoso.Tests")]
        [DataRow("Contoso.Testing.App")]
        [DataRow("TestCo.App")]
        public void Constructor_WithSegmentStartingWithTest_ShouldThrowBecauseProjectTypeDetectionWouldBreak(string @namespace)
        {
            Action act = () => new ScaffoldOptions(@namespace, ToolVersion);

            act.Should().Throw<ArgumentException>().WithParameterName("namespace").WithMessage("*Test*");
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(8)]
        [DataRow(9)]
        [DataRow(12)]
        public void Constructor_WithUnsupportedDotNetVersion_ShouldThrowArgumentException(int dotNetVersion)
        {
            Action act = () => new ScaffoldOptions("Contoso", ToolVersion, dotNetVersion);

            act.Should().Throw<ArgumentException>().WithParameterName("dotNetVersion").WithMessage($"*.NET {dotNetVersion}*");
        }

        [TestMethod]
        [DataRow(10, "net10.0")]
        [DataRow(11, "net11.0")]
        public void Constructor_ShouldBuildTheTargetFrameworkFromTheDotNetVersion(int dotNetVersion, string expected)
        {
            var options = new ScaffoldOptions("Contoso", ToolVersion, dotNetVersion);

            options.DotNetVersion.Should().Be(dotNetVersion);
            options.TargetFramework.Should().Be(expected);
        }

        [TestMethod]
        public void Constructor_WithNoMessageBus_ShouldAlsoExcludeRuntime()
        {
            var options = new ScaffoldOptions("Contoso", ToolVersion, includeMessageBus: false);

            options.IncludeMessageBus.Should().BeFalse();
            options.IncludeRuntime.Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_WithWebJobAndNoRuntime_ShouldThrowArgumentException()
        {
            Action act = () => new ScaffoldOptions("Contoso", ToolVersion, includeRuntime: false, webJob: true);

            act.Should().Throw<ArgumentException>().WithParameterName("webJob");
        }

        [TestMethod]
        public void Constructor_WithWebJobAndNoMessageBus_ShouldThrowArgumentException()
        {
            Action act = () => new ScaffoldOptions("Contoso", ToolVersion, includeMessageBus: false, webJob: true);

            act.Should().Throw<ArgumentException>().WithParameterName("webJob");
        }

        #endregion

        #region Product and Company Tests

        [TestMethod]
        [DataRow("CloudNimble.Contoso", "Contoso", "CloudNimble")]
        [DataRow("Contoso", "Contoso", "Contoso")]
        [DataRow("A.B.C", "C", "A")]
        public void Constructor_ShouldDeriveProductFromLastSegmentAndCompanyFromFirst(string @namespace, string product, string company)
        {
            var options = new ScaffoldOptions(@namespace, ToolVersion);

            options.Namespace.Should().Be(@namespace);
            options.Product.Should().Be(product);
            options.Company.Should().Be(company);
        }

        [TestMethod]
        public void Constructor_ShouldTrimSurroundingWhitespace()
        {
            var options = new ScaffoldOptions("  CloudNimble.Contoso  ", ToolVersion);

            options.Namespace.Should().Be("CloudNimble.Contoso");
        }

        #endregion

        #region MicrosoftPackageVersion Tests

        [TestMethod]
        [DataRow(10, "10.*")]
        [DataRow(11, "11.*-*")]
        public void MicrosoftPackageVersion_ShouldFloatByDotNetVersion(int dotNetVersion, string expected)
        {
            var options = new ScaffoldOptions("Contoso", ToolVersion, dotNetVersion);

            options.MicrosoftPackageVersion.Should().Be(expected);
        }

        #endregion

        #region EasyAFPackageVersion Tests

        [TestMethod]
        [DataRow("5.0.0", false, "5.*")]
        [DataRow("5.0.0", true, "5.*-*")]
        [DataRow("6.1.2", false, "6.*")]
        [DataRow("6.1.2", true, "6.*-*")]
        public void EasyAFPackageVersion_ShouldUseTheToolMajorAndFloatToPrereleasesOnlyForAPrereleaseTool(string toolVersion, bool toolIsPrerelease, string expected)
        {
            var options = new ScaffoldOptions("Contoso", Version.Parse(toolVersion), toolIsPrerelease: toolIsPrerelease);

            options.EasyAFPackageVersion.Should().Be(expected);
        }

        #endregion

        #region CreateTokens Tests

        [TestMethod]
        public void CreateTokens_ShouldSupplyEveryScaffoldToken()
        {
            var options = new ScaffoldOptions("CloudNimble.Contoso", ToolVersion);

            var tokens = options.CreateTokens("10.0.401", 2026);

            tokens.Should().BeEquivalentTo(new System.Collections.Generic.Dictionary<string, string>
            {
                ["Company"] = "CloudNimble",
                ["EasyAFPackageVersion"] = "5.*",
                ["Namespace"] = "CloudNimble.Contoso",
                ["Product"] = "Contoso",
                ["SdkVersion"] = "10.0.401",
                ["Year"] = "2026",
            });
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void CreateTokens_WithMissingSdkVersion_ShouldThrowArgumentException(string sdkVersion)
        {
            var options = new ScaffoldOptions("Contoso", ToolVersion);

            Action act = () => options.CreateTokens(sdkVersion, 2026);

            act.Should().Throw<ArgumentException>().WithParameterName("sdkVersion");
        }

        #endregion

    }

}
