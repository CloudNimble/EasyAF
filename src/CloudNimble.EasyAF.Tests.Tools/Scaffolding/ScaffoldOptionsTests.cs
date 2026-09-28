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

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithDefaults_ShouldIncludeEverythingOnNet10()
        {
            var options = new ScaffoldOptions("CloudNimble.Contoso");

            options.TargetFramework.Should().Be("net10.0");
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
            Action act = () => new ScaffoldOptions(@namespace);

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
            Action act = () => new ScaffoldOptions(@namespace);

            act.Should().Throw<ArgumentException>().WithParameterName("namespace").WithMessage($"*'{@namespace}'*");
        }

        [TestMethod]
        [DataRow("Contoso.Tests")]
        [DataRow("Contoso.Testing.App")]
        [DataRow("TestCo.App")]
        public void Constructor_WithSegmentStartingWithTest_ShouldThrowBecauseProjectTypeDetectionWouldBreak(string @namespace)
        {
            Action act = () => new ScaffoldOptions(@namespace);

            act.Should().Throw<ArgumentException>().WithParameterName("namespace").WithMessage("*Test*");
        }

        [TestMethod]
        [DataRow("net472")]
        [DataRow("netstandard2.0")]
        [DataRow("garbage")]
        [DataRow("net8.0")]
        [DataRow("net9.0")]
        [DataRow("net12.0")]
        public void Constructor_WithUnsupportedTargetFramework_ShouldThrowArgumentException(string targetFramework)
        {
            Action act = () => new ScaffoldOptions("Contoso", targetFramework);

            act.Should().Throw<ArgumentException>().WithParameterName("targetFramework");
        }

        [TestMethod]
        public void Constructor_WithNoMessageBus_ShouldAlsoExcludeRuntime()
        {
            var options = new ScaffoldOptions("Contoso", includeMessageBus: false);

            options.IncludeMessageBus.Should().BeFalse();
            options.IncludeRuntime.Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_WithWebJobAndNoRuntime_ShouldThrowArgumentException()
        {
            Action act = () => new ScaffoldOptions("Contoso", includeRuntime: false, webJob: true);

            act.Should().Throw<ArgumentException>().WithParameterName("webJob");
        }

        [TestMethod]
        public void Constructor_WithWebJobAndNoMessageBus_ShouldThrowArgumentException()
        {
            Action act = () => new ScaffoldOptions("Contoso", includeMessageBus: false, webJob: true);

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
            var options = new ScaffoldOptions(@namespace);

            options.Namespace.Should().Be(@namespace);
            options.Product.Should().Be(product);
            options.Company.Should().Be(company);
        }

        [TestMethod]
        public void Constructor_ShouldTrimSurroundingWhitespace()
        {
            var options = new ScaffoldOptions("  CloudNimble.Contoso  ");

            options.Namespace.Should().Be("CloudNimble.Contoso");
        }

        #endregion

        #region MicrosoftPackageVersion Tests

        [TestMethod]
        [DataRow("net10.0", "10.*")]
        [DataRow("net11.0", "11.*-*")]
        public void MicrosoftPackageVersion_ShouldFloatByTargetFramework(string targetFramework, string expected)
        {
            var options = new ScaffoldOptions("Contoso", targetFramework);

            options.MicrosoftPackageVersion.Should().Be(expected);
        }

        #endregion

        #region CreateTokens Tests

        [TestMethod]
        public void CreateTokens_ShouldSupplyEveryScaffoldToken()
        {
            var options = new ScaffoldOptions("CloudNimble.Contoso");

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
            var options = new ScaffoldOptions("Contoso");

            Action act = () => options.CreateTokens(sdkVersion, 2026);

            act.Should().Throw<ArgumentException>().WithParameterName("sdkVersion");
        }

        #endregion

    }

}
