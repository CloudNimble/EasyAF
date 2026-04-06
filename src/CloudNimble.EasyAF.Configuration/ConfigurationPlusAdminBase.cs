using System.Diagnostics.CodeAnalysis;

namespace CloudNimble.EasyAF.Configuration
{
    /// <summary>
    /// An extended configuration class that includes both public and administrative endpoint configuration.
    /// Inherits from <see cref="ConfigurationBase"/> and adds support for administrative APIs and applications.
    /// </summary>
    /// <remarks>
    /// This configuration class should be used for applications that need both customer-facing and 
    /// administrative functionality, such as multi-tenant applications with separate admin interfaces
    /// or applications that need to communicate with both public and private APIs.
    /// </remarks>
    /// <example>
    /// <code>
    /// // In Program.cs or Startup.cs
    /// builder.Services.AddConfigurationBase&lt;MyAdminConfiguration&gt;(builder.Configuration, "AppSettings");
    /// 
    /// // Example configuration in appsettings.json
    /// {
    ///   "AppSettings": {
    ///     "ApiRoot": "https://api.mycompany.com",
    ///     "AppRoot": "https://myapp.mycompany.com",
    ///     "AdminApiRoot": "https://admin-api.mycompany.com",
    ///     "AdminAppRoot": "https://admin.mycompany.com",
    ///     "HttpHandlerMode": "Add"
    ///   }
    /// }
    /// 
    /// // Usage in administrative components
    /// [Inject] public MyAdminConfiguration Config { get; set; }
    /// 
    /// private async Task CallAdminApi()
    /// {
    ///     var adminClient = HttpClientFactory.CreateClient(Config.AdminApiClientName);
    ///     var response = await adminClient.GetAsync($"{Config.AdminApiRoot}/admin/users");
    /// }
    /// </code>
    /// </example>
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
    public class ConfigurationPlusAdminBase : ConfigurationBase
    {

        /// <summary>
        /// The name of the HttpClient that will be used to hit the Admin Blazor Controllers.
        /// </summary>
        public string AdminAppClientName { get; set; } = "AdminAppClient";

        /// <summary>
        /// The website your Administrative Blazor app is being served from.
        /// </summary>
        /// <remarks>
        /// Sometimes you will need to get information about the app's deployment before it has been fully-initialized in Program.cs. This is the place to do it.
        /// </remarks>
        [HttpEndpoint(nameof(AdminAppClientName))]
        public string AdminAppRoot { get; set; }

        /// <summary>
        /// The name of the HttpClient that will be used to hit the Admin (Private) API.
        /// </summary>
        public string AdminApiClientName { get; set; } = "AdminApiClient";

        /// <summary>
        /// The root of the Admin (Private) API.
        /// </summary>
        /// <remarks>
        /// Most Blazor apps will call at least one API. If you need to call more than one, just inherit from ConfigurationBase and add your own properties.
        /// </remarks>
        [HttpEndpoint(nameof(AdminApiClientName))]
        public string AdminApiRoot { get; set; }

    }

}
