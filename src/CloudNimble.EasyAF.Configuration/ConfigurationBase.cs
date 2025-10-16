using CloudNimble.EasyAF.Core;
using System.Diagnostics.CodeAnalysis;

namespace CloudNimble.EasyAF.Configuration
{
    /// <summary>
    /// A base class implementation of the configuration your Blazor app will pull from wwwroot/appsettings.json.
    /// Provides standard HttpClient configuration for API and application endpoints.
    /// </summary>
    /// <remarks>
    /// This configuration class is typically used for customer-facing applications that need to communicate
    /// with external APIs and handle application-level HTTP requests. For administrative applications,
    /// consider using <see cref="ConfigurationPlusAdminBase"/> instead.
    /// </remarks>
    /// <example>
    /// <code>
    /// // In Program.cs or Startup.cs
    /// builder.Services.AddConfigurationBase&lt;MyAppConfiguration&gt;(builder.Configuration, "AppSettings");
    /// 
    /// // Example configuration in appsettings.json
    /// {
    ///   "AppSettings": {
    ///     "ApiRoot": "https://api.mycompany.com",
    ///     "AppRoot": "https://myapp.mycompany.com",
    ///     "HttpHandlerMode": "Add"
    ///   }
    /// }
    /// 
    /// // Usage in components
    /// [Inject] public MyAppConfiguration Config { get; set; }
    /// 
    /// private async Task CallApi()
    /// {
    ///     var httpClient = HttpClientFactory.CreateClient(Config.ApiClientName);
    ///     var response = await httpClient.GetAsync($"{Config.ApiRoot}/api/data");
    /// }
    /// </code>
    /// </example>
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    public class ConfigurationBase
    {

        /// <summary>
        /// The name of the HttpClient that will be used to hit the app's Public API.
        /// </summary>
        public string ApiClientName { get; set; } = "ApiClient";

        /// <summary>
        /// The root of the API that your Blazor app will call.
        /// </summary>
        /// <remarks>
        /// Most Blazor apps will call at least one API. If you need to call more than one, just inherit from ConfigurationBase and add your own properties.
        /// </remarks>
        [HttpEndpoint(nameof(ApiClientName))]
        public string ApiRoot { get; set; }

        /// <summary>
        /// The name of the HttpClient that will be used to hit the Blazor App's Controllers.
        /// </summary>
        public string AppClientName { get; set; } = "AppClient";

        /// <summary>
        /// The website your Blazor app is being served from.
        /// </summary>
        /// <remarks>
        /// Sometimes you will need to get information about the app's deployment before it has been fully-initialized in Program.cs. This is the place to do it.
        /// </remarks>
        [HttpEndpoint(nameof(AppClientName))]
        public string AppRoot { get; set; }

        /// <summary>
        /// Determines how HttpClient message handlers are configured when registering HTTP clients.
        /// Controls whether handlers are added to existing handlers or replace them entirely.
        /// </summary>
        public HttpHandlerMode HttpHandlerMode { get; set; } = HttpHandlerMode.Add;

    }

}
