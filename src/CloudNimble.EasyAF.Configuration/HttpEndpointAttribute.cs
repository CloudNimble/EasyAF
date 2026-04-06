using System;

namespace CloudNimble.EasyAF.Configuration
{

    /// <summary>
    /// Specifies that a configuration property represents an HTTP endpoint URL for an HttpClient. 
    /// Used by the EasyAF configuration system to automatically register HttpClients with their base addresses.
    /// </summary>
    /// <remarks>
    /// This attribute enables automatic HttpClient registration by linking configuration properties
    /// that contain URLs to the corresponding HttpClient name properties. The configuration system
    /// uses this information to set up named HttpClient instances with appropriate base addresses.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class MyConfiguration : ConfigurationBase
    /// {
    ///     public string MyApiClientName { get; set; } = "MyApiClient";
    ///     
    ///     [HttpEndpoint(nameof(MyApiClientName))]
    ///     public string MyApiRoot { get; set; } = "https://api.example.com";
    /// }
    /// 
    /// // This will automatically register an HttpClient named "MyApiClient" 
    /// // with base address "https://api.example.com"
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property)]
    public class HttpEndpointAttribute : Attribute
    {

        /// <summary>
        /// Gets or sets the name of the property that contains the HttpClient name to be registered.
        /// This property should contain the string value that will be used as the named HttpClient identifier.
        /// </summary>
        public string ClientNameProperty { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpEndpointAttribute"/> class.
        /// </summary>
        /// <param name="clientNameProperty">The name of the property that contains the HttpClient name for registration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="clientNameProperty"/> is null.</exception>
        public HttpEndpointAttribute(string clientNameProperty)
        {
            ClientNameProperty = clientNameProperty;
        }

    }

}
