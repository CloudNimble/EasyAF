using CloudNimble.EasyAF.Tools.Commands.Root;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CloudNimble.EasyAF.Tests.Tools
{

    /// <summary>
    /// Tests for <see cref="EasyAFRootCommand"/>.
    /// </summary>
    [TestClass]
    public class EasyAFRootCommandTests
    {

        #region ParseVersion Tests

        [TestMethod]
        [DataRow("5.0.0", "5.0.0", false)]
        [DataRow("5.0.0+29855be8d0ca0ce04521815e8ead2f0b477f991e", "5.0.0", false)]
        [DataRow("5.0.0-CI-20261003-233509+29855be8d0ca0ce04521815e8ead2f0b477f991e", "5.0.0", true)]
        [DataRow("5.0.0-preview.1", "5.0.0", true)]
        [DataRow("5.1.2-rc.1+abc123", "5.1.2", true)]
        public void ParseVersion_ShouldSplitTheVersionFromThePrereleaseLabelAndBuildMetadata(string informationalVersion, string expectedVersion, bool expectedPrerelease)
        {
            var (version, isPrerelease) = EasyAFRootCommand.ParseVersion(informationalVersion);

            version.Should().Be(Version.Parse(expectedVersion));
            isPrerelease.Should().Be(expectedPrerelease);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("garbage")]
        public void ParseVersion_WithAnUnparseableVersion_ShouldReturnNoVersion(string informationalVersion)
        {
            var (version, isPrerelease) = EasyAFRootCommand.ParseVersion(informationalVersion);

            version.Should().BeNull();
            isPrerelease.Should().BeFalse();
        }

        #endregion

        #region Startup Tests

        [TestMethod]
        public void Description_ShouldShowTheMajorAndMinorVersion()
        {
            EasyAFRootCommand.Version.Should().NotBeNull();
            EasyAFRootCommand.Description.Should().StartWith($"EasyAF {EasyAFRootCommand.Version.Major}.{EasyAFRootCommand.Version.Minor} CLI Tools.");
        }

        #endregion

    }

}
