using BenchmarkDotNet.Attributes;
using CloudNimble.EasyAF.Core.Converters;
using EasyAFModel;
using System;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Benchmarks
{

    /// <summary>
    /// Measures the serialization helpers that run on API requests and responses, using the generated <see cref="Product"/> entity
    /// that Tests.Shared uses. The V4 benchmarks use the EasyAF 4.x entity and converter from the V4 namespace.
    /// </summary>
    [MemoryDiagnoser]
    public class SerializationBenchmarks
    {

        #region Fields

        private readonly JsonSerializerOptions _plainOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };

        private readonly JsonSerializerOptions _ignoreAuditOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        }.IgnoreAuditFields();

#pragma warning disable CS0618 // RWM: Measures the obsolete converter until it is removed.
        private readonly JsonSerializerOptions _obsoleteConverterOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters = { new IgnoreAuditFieldsJsonConverterFactory() },
        };
#pragma warning restore CS0618

        private readonly JsonSerializerOptions _easyAF4xOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters = { new V4.IgnoreAuditFieldsJsonConverterFactory() },
        };

        private Product _product;
        private V4.Product _v4Product;
        private string _productJson;

        #endregion

        #region Setup

        /// <summary>
        /// Creates a fully populated product and its JSON.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            _product = new Product
            {
                Id = Guid.NewGuid(),
                DisplayName = "Contoso Widget",
                StatusTypeId = Guid.NewGuid(),
                CreatedById = Guid.NewGuid(),
                DateCreated = DateTimeOffset.UtcNow,
                UpdatedById = Guid.NewGuid(),
                DateUpdated = DateTimeOffset.UtcNow,
            };
            _v4Product = new V4.Product
            {
                Id = _product.Id,
                DisplayName = _product.DisplayName,
                StatusTypeId = _product.StatusTypeId,
                CreatedById = _product.CreatedById,
                DateCreated = _product.DateCreated,
                UpdatedById = _product.UpdatedById,
                DateUpdated = _product.DateUpdated,
            };
            _productJson = JsonSerializer.Serialize(_product, _plainOptions);
        }

        #endregion

        #region System.Text.Json Converter Benchmarks

        [Benchmark(Baseline = true)]
        public string Serialize_Plain() => JsonSerializer.Serialize(_product, _plainOptions);

        [Benchmark]
        public string Serialize_V5_IgnoreAuditFields() => JsonSerializer.Serialize(_product, _ignoreAuditOptions);

        [Benchmark]
        public string Serialize_V5_ObsoleteConverter() => JsonSerializer.Serialize(_product, _obsoleteConverterOptions);

        [Benchmark]
        public string Serialize_V4() => JsonSerializer.Serialize(_v4Product, _easyAF4xOptions);

        [Benchmark]
        public Product Deserialize_V5_IgnoreAuditFields() => JsonSerializer.Deserialize<Product>(_productJson, _ignoreAuditOptions);

        [Benchmark]
        public Product Deserialize_V5_ObsoleteConverter() => JsonSerializer.Deserialize<Product>(_productJson, _obsoleteConverterOptions);

        [Benchmark]
        public V4.Product Deserialize_V4() => JsonSerializer.Deserialize<V4.Product>(_productJson, _easyAF4xOptions);

        #endregion

        #region DbObservableObject Benchmarks

        [Benchmark]
        public ExpandoObject ToDeltaPayload()
        {
            var product = new Product { Id = _product.Id, DisplayName = _product.DisplayName };
            product.TrackChanges();
            product.DisplayName = "Contoso Widget Pro";
            return product.ToDeltaPayload();
        }

        #endregion

        #region Http Benchmarks

        [Benchmark]
        public async Task<Product> Http_DeserializeResponse_SystemTextJson()
        {
            using var message = CreateResponse();
            var (response, _) = await EasyAF_Http_SystemTextJson_HttpResponseMessageExtensions.DeserializeResponseAsync<Product>(message);
            return response;
        }

        [Benchmark]
        public async Task<Product> Http_DeserializeResponse_NewtonsoftJson()
        {
            using var message = CreateResponse();
            var (response, _) = await EasyAF_Http_NewtonsoftJson_HttpResponseMessageExtensions.DeserializeResponseAsync<Product>(message);
            return response;
        }

        /// <summary>
        /// Creates a successful response carrying the product JSON. Both Http benchmarks pay this cost equally.
        /// </summary>
        /// <returns>The response.</returns>
        private HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(_productJson, Encoding.UTF8, "application/json"),
            };
        }

        #endregion

    }

}
