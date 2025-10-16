using CloudNimble.EasyAF.Configuration;
using CloudNimble.EasyAF.Core;
using Simple.OData.Client;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;

namespace CloudNimble.EasyAF.OData
{

    /// <summary>
    /// Provides custom configuration for ODataClient and ODataBatchClients using Simple.OData.V4.Client
    /// </summary>
    internal static class ODataClientSettingsHelper
    {

        /// <summary>
        /// Provides defaults for some important settings and improves how the client returns errors that come back from service.
        /// Configures the OData client with sensible defaults including ignore settings and custom User-Agent header.
        /// </summary>
        /// <param name="httpClientFactory">An <see cref="IHttpClientFactory"/> instance, provided by DI.</param>
        /// <param name="configurationBase">A <see cref="ConfigurationBase"/> instance, containing the name identifier for the <see cref="HttpClient"/>.</param>
        /// <param name="apiClientName">The name of the HTTP client to create from the factory.</param>
        /// <returns>A configured ODataClientSettings instance with default behaviors and custom User-Agent.</returns>
        internal static ODataClientSettings GetODataSettings<T>(IHttpClientFactory httpClientFactory, ConfigurationBase configurationBase, string apiClientName)
            where T: class
        {
            Ensure.ArgumentNotNull(httpClientFactory, nameof(httpClientFactory));
            Ensure.ArgumentNotNull(configurationBase, nameof(configurationBase));

            var caller = new StackTrace().GetFrames().Where(c =>
            {
                var method = c.GetMethod();
                return method.DeclaringType?.FullName?.StartsWith("System") is not true && method.DeclaringType?.FullName?.StartsWith("Microsoft") is not true;
            })
            .LastOrDefault();
            var assemblyName = caller?.GetMethod().DeclaringType.Assembly.GetName();
            var callerName = assemblyName?.Name ?? "";
            var version = assemblyName?.Version.ToString() ?? ""; 
            
            //RWM: Report the Name & Version of the client, as well as the Name, Environment, and Version of the app using it.
            var userAgent = string.Format("{0}/{1} ({2}/{3})",
                typeof(T).FullName,
                typeof(T).Assembly.GetName().Version,
                callerName,
                version);

            return new ODataClientSettings(httpClientFactory.CreateClient(apiClientName))
            {
                IgnoreUnmappedProperties = true,
                IgnoreResourceNotFoundException = true,
                IgnoreNavigationPropertiesOnUpdate = true,
                BeforeRequest = (request) =>
                {
                    request.Headers.Add("User-Agent", userAgent);
                },
                //AfterResponseAsync = async (response) =>
                //{
                //}
            };
        }

    }

}
