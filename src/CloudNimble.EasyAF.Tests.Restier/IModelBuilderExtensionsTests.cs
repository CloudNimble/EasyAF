using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.Restier
{

    ///// <summary>
    ///// 
    ///// </summary>
    //[TestClass]
    //public class IModelBuilderExtensionsTests : EasyAFContextApiTestBase
    //{

    //    /// <summary>
    //    /// Initializes the test environment with user credentials.
    //    /// </summary>
    //    [TestInitialize]
    //    public void TestInitialize() => TestSetup();

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    [TestMethod]
    //    public async Task ApiIsSecured()
    //    {
    //        var metadataDoc = await GetApiMetadataAsync();
    //        metadataDoc.Should().NotBeNull();

    //        var metadata = metadataDoc.ToString();
    //        metadata.Should().NotBeNullOrWhiteSpace();
    //        metadata.Should().Contain("CreatedById", Exactly.Once());
    //        metadata.Should().NotContain("DateCreated");
    //        metadata.Should().NotContain("UpdatedById");
    //        metadata.Should().NotContain("DateUpdated");
    //    }

    //}

}
