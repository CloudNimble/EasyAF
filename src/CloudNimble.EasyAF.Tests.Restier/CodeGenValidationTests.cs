using CloudNimble.Breakdance.Assemblies;
using FluentAssertions;
using Microsoft.Restier.Breakdance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CloudNimble.EasyAF.Tests.Restier
{

    ///// <summary>
    ///// 
    ///// </summary>
    //[TestClass]
    //public class CodeGenValidationTests : EasyAFContextApiTestBase
    //{

    //    private const string baselinesPath = "..//..//..//Baselines";


    //    /// <summary>
    //    /// Initializes the test environment with user credentials.
    //    /// </summary>
    //    [TestInitialize]
    //    public void TestInitialize() => TestSetup();

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    [TestMethod]
    //    public void EasyAFEntitiesApi_VisibilityMatrix()
    //    {
    //        var baseline = File.ReadAllText(Path.Combine(baselinesPath, "EasyAFEntitiesApi-ApiSurface.md"));
    //        baseline.Should().NotBeNullOrWhiteSpace();

    //        var matrix = GetApiInstance().GenerateVisibilityMatrix(true);
    //        matrix.Should().NotBeNullOrWhiteSpace();

    //        TestContext.WriteLine($"Old Report: {baseline}");
    //        TestContext.WriteLine($"New Report: {matrix}");

    //        matrix.Should().Be(baseline);

    //        matrix.Should().Contain("**OnInsertingProductAsync**                        |   **True**");

    //    }

    //    #region Manifest Generators

    //    //[DataRow(baselinesPath)]
    //    //[TestMethod]
    //    [BreakdanceManifestGenerator]
    //    public void EasyAFEntitiesApi_ApiSurface_WriteOutput(string projectPath)
    //    {
    //        GetApiInstance().WriteCurrentVisibilityMatrix(projectPath, markdown: true);
    //    }

    //    #endregion

    //}

}
