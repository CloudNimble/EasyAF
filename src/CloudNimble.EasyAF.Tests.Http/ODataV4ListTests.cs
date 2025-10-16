using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Threading.Tasks;
using System.Net.Http;
using System.Dynamic;
using CloudNimble.EasyAF.Http.OData;

namespace CloudNimble.EasyAF.Tests.Http
{

    [TestClass]
    public class ODataV4ListTests
    {

        [TestMethod]
        public async Task ListWithCount_DeserializesProperly()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://services.odata.org/TripPinRESTierService/People?$count=true");
            var (Result, ErrorContent) = await response.DeserializeResponseAsync<ODataV4List<ExpandoObject>>();
            ErrorContent.Should().BeNullOrEmpty();
            Result.Should().NotBeNull();
            Result.ODataCount.Should().NotBe(0);
            Result.ODataCount.Should().Be(Result.Items.Count);
            Result.ODataContext.Should().NotBeNullOrWhiteSpace();
        }

    }

}
