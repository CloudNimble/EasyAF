using CloudNimble.EasyAF.Configuration;
using Simple.OData.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.OData
{

    /// <summary>
    /// Provides a pre-configured Simple.OData.V4 <see cref="ODataBatch"/> Client.
    /// </summary>
    public class ApiBatch : ODataBatch
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Simple.OData.Client.ODataClient class with custom configuration
        /// </summary>
        /// <param name="httpClientFactory">An <see cref="IHttpClientFactory"/> instance, provided by DI.</param>
        /// <param name="configurationBase">A <see cref="ConfigurationBase"/> instance, containing the name identifier for the <see cref="HttpClient"/>.</param>
        /// <param name="apiClientName">Optional name for the API client. If not provided, uses the ApiClientName from the configuration.</param>
        public ApiBatch(IHttpClientFactory httpClientFactory, ConfigurationBase configurationBase, string apiClientName = null)
            : base(ODataClientSettingsHelper.GetODataSettings<ApiBatch>(httpClientFactory, configurationBase, apiClientName ?? configurationBase.ApiClientName))
        {
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Overloads the "+" operator used to add <see cref="IODataClient"/> operations to the <see cref="ODataBatch"/>.
        /// </summary>
        /// <param name="batch">The ApiBatch instance to add the operation to.</param>
        /// <param name="action">The async operation to add to the batch.</param>
        /// <returns>The ApiBatch instance for method chaining.</returns>
        public static ApiBatch operator +(ApiBatch batch, Func<IODataClient, Task> action)
        {
            var odata = batch as ODataBatch;
            // RWM: Do not let Roslyn lightbulb actions optimize out the next line. It prevents a recursive infinite loop.
            odata += action;
            return batch;
        }

        /// <summary>
        /// Overloads the Add operator used to add <see cref="IODataClient"/> operations to the <see cref="ODataBatch"/>.
        /// Provides an alternative method-based syntax for adding operations to the batch.
        /// </summary>
        /// <param name="batch">The ApiBatch instance to add the operation to.</param>
        /// <param name="action">The async operation to add to the batch.</param>
        /// <returns>The ApiBatch instance for method chaining.</returns>
        public static ApiBatch Add(ApiBatch batch, Func<IODataClient, Task> action)
        {
            batch += action;
            return batch;
        }

        #endregion

    }

}
