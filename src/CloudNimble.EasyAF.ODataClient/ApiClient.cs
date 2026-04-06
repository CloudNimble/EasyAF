using CloudNimble.EasyAF.Configuration;
using Simple.OData.Client;
using System.Net.Http;

namespace CloudNimble.EasyAF.OData
{

    /// <summary>
    /// Provides a pre-configured Simple.OData.V4 <see cref="ODataClient"/>.
    /// </summary>
    public class ApiClient : ODataClient
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Simple.OData.Client.ODataClient class with custom configuration
        /// </summary>
        /// <param name="httpClientFactory">An <see cref="IHttpClientFactory"/> instance, provided by DI.</param>
        /// <param name="configurationBase">A <see cref="ConfigurationBase"/> instance, containing the name identifier for the <see cref="HttpClient"/>.</param>
        /// <param name="apiClientName">Optional name for the API client. If not provided, uses the ApiClientName from the configuration.</param>
        public ApiClient(IHttpClientFactory httpClientFactory, ConfigurationBase configurationBase, string apiClientName = null)
            : base(ODataClientSettingsHelper.GetODataSettings<ApiClient>(httpClientFactory, configurationBase, apiClientName ?? configurationBase.ApiClientName))
        {
        }

        #endregion

    }

}
