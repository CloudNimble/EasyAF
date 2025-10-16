using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Threading.Tasks;
using System.Net.Http;
using System.Dynamic;
using CloudNimble.EasyAF.Http.OData;
using System.Text.Json;

namespace CloudNimble.EasyAF.Tests.Http
{

    [TestClass]
    public class ODataV4PrimitiveResultTests
    {

        #region Private Members

        string booleanPayload = " {\"@odata.context\":\"http://localhost/api/tests/$metadata#Edm.Boolean\",\"value\":true}";

        #endregion

        [TestMethod]
        public void Boolean_CanDeserialize()
        {
            var result = JsonSerializer.Deserialize<ODataV4PrimitiveResult<bool>>(booleanPayload);
            result.Should().NotBeNull();
            result.ODataContext.Should().NotBeNullOrWhiteSpace();
            result.Value.Should().BeTrue();
        }

        [TestMethod]
        public async Task SingleEntity_DeserializesProperly()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://services.odata.org/TripPinRESTierService/People('russellwhyte')");
            var (Result, ErrorContent) = await response.DeserializeResponseAsync<ODataV4PrimitiveResult<ExpandoObject>>();
            ErrorContent.Should().BeNullOrEmpty();
            Result.Should().NotBeNull();
            Result.ODataContext.Should().NotBeNullOrWhiteSpace();
        }

    }

}
