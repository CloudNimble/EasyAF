using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CloudNimble.EasyAF.Http.OData;

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
            var random = new Random();

            for (var attempt = 1; attempt <= 3; attempt++)
            {
                var response = await client.GetAsync("https://services.odata.org/TripPinRESTierService/People('russellwhyte')");
                var (Result, ErrorContent) = await response.DeserializeResponseAsync<ODataV4PrimitiveResult<ExpandoObject>>();

                if (string.IsNullOrWhiteSpace(ErrorContent))
                {
                    Result.Should().NotBeNull();
                    Result.ODataContext.Should().NotBeNullOrWhiteSpace();
                    return;
                }

                if (attempt < 3)
                {
                    await Task.Delay(random.Next(500, 2000));
                }
            }

            Assert.Inconclusive("OData TripPin service returned errors on all attempts. The service may be temporarily unavailable.");
        }

    }

}
