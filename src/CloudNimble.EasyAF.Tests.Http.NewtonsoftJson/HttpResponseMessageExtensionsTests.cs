using CloudNimble.Breakdance.Assemblies.Http;
using CloudNimble.EasyAF.Http.OData;
using FluentAssertions;
using Microsoft.Restier.Tests.Shared.Scenarios.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

#if NEWTONSOFT
namespace CloudNimble.EasyAF.Tests.Http.NewtonsoftJson
#else
namespace CloudNimble.EasyAF.Tests.Http.SystemTextJson
#endif
{

    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class HttpResponseMessageExtensionsTests
    {

        private static readonly string baselines = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../../../CloudNimble.EasyAF.Tests.Http.NewtonsoftJson/Baselines"));


        [TestMethod]
        public async Task DeserializeResponseAsync_List()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://services.odata.org/TripPinRESTierService/People", TestContext.CancellationToken);
            var (Result, ErrorContent) = await response.DeserializeResponseAsync<ODataV4List<ExpandoObject>>();
            ErrorContent.Should().BeNullOrEmpty();
            Result.Should().NotBeNull();
        }

        [TestMethod]
        public async Task DeserializeResponseAsync_List2()
        {
            var client = new HttpClient(new ResponseSnapshotReplayHandler(baselines));
            var response = await client.GetAsync("https://localhost/api/tests/Books", TestContext.CancellationToken);
            var (Result, ErrorContent) = await response.DeserializeResponseAsync<ODataV4List<Book>>();
            ErrorContent.Should().BeNullOrEmpty();
            Result.Should().NotBeNull();
            Result.ODataContext.Should().NotBeNullOrWhiteSpace();
            Result.Items.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public async Task DeserializeResponseAsync_SystemTextJsonAnnotations()
        {
            var client = new HttpClient(new ResponseSnapshotReplayHandler(baselines));
            var response = await client.GetAsync("https://localhost/api/tests/People", TestContext.CancellationToken);
            var (Result, ErrorContent) = await response.DeserializeResponseAsync<ODataV4List<Person>>();
            ErrorContent.Should().BeNullOrEmpty();
            Result.Should().NotBeNull();
            Result.ODataContext.Should().NotBeNullOrWhiteSpace();
            Result.Items.Should().NotBeNullOrEmpty();
            Result.Items[0].FirstName.Should().Be("Robert");
        }

        [TestMethod]
        public async Task DeserializeResponseAsync_WrongUrl()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://services.odata.org/TripPinRESTierService/Robert");
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var (Result, ErrorContent) = await response.DeserializeResponseAsync<ExpandoObject>();

            Result.Should().BeNull();
            ErrorContent.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public async Task DeserializeResponseAsync_NoContent()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Options, "https://services.odata.org/TripPinRESTierService/People");
            var response = await client.SendAsync(request, TestContext.CancellationToken);
            response.IsSuccessStatusCode.Should().BeTrue();

            var (Result, ErrorContent) = await response.DeserializeResponseAsync<ExpandoObject>();
            Result.Should().BeNull();
            ErrorContent.Should().BeNullOrEmpty();
        }

        [TestMethod]
        public async Task DeserializeResponseAsync_BadDelete_NoContent()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Delete, "https://services.odata.org/TripPinRESTierService/People");
            var response = await client.SendAsync(request, TestContext.CancellationToken);

            var (Result, ErrorContent) = await response.DeserializeResponseAsync<ExpandoObject, ODataV4ErrorResponse>();

            Result.Should().BeNull();
            ErrorContent.Should().NotBeNull();
            ErrorContent.Error.Should().NotBeNull();
            ErrorContent.Error.Message.Should().Be("Element type cannot be found for 'Collection(Trippin.Person)'.");
        }

        public TestContext TestContext { get; set; }
    }

}
