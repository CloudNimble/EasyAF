using CloudNimble.EasyAF.Configuration;
using CloudNimble.EasyAF.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{

    /// <summary>
    /// Provides extension methods for registering EasyAF HTTP clients in the dependency injection container.
    /// Automatically configures HttpClient instances based on configuration attributes.
    /// </summary>
    public static class EasyAF_Http_IServiceCollectionExtensions
    {

        static readonly CacheControlHeaderValue cacheControlHeaderValue = new() { NoCache = true };

        /// <summary>
        /// Adds HTTP clients to the service collection based on configuration properties marked with <see cref="HttpEndpointAttribute"/>.
        /// Uses the default HttpHandlerMode from the configuration.
        /// </summary>
        /// <typeparam name="TConfig">The configuration type that contains HTTP endpoint definitions.</typeparam>
        /// <typeparam name="TMessageHandler">The type of message handler to add to the HTTP clients.</typeparam>
        /// <param name="services">The service collection to add HTTP clients to.</param>
        /// <param name="config">The configuration instance containing endpoint definitions.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddHttpClients<TConfig, TMessageHandler>(this IServiceCollection services, TConfig config)
            where TMessageHandler : DelegatingHandler
            where TConfig : ConfigurationBase
        {
            return AddHttpClients<TConfig, TMessageHandler>(services, config, config.HttpHandlerMode);
        }

        /// <summary>
        /// Adds HTTP clients to the service collection based on configuration properties marked with <see cref="HttpEndpointAttribute"/>.
        /// Allows explicit specification of the HttpHandlerMode for message handler configuration.
        /// </summary>
        /// <typeparam name="TConfig">The configuration type that contains HTTP endpoint definitions.</typeparam>
        /// <typeparam name="TMessageHandler">The type of message handler to add to the HTTP clients.</typeparam>
        /// <param name="services">The service collection to add HTTP clients to.</param>
        /// <param name="config">The configuration instance containing endpoint definitions.</param>
        /// <param name="httpHandlerMode">Specifies how message handlers should be configured for the HTTP clients.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <example>
        /// <code>
        /// // Register HTTP clients with custom message handler
        /// services.AddHttpClients&lt;MyConfiguration, MyAuthHandler&gt;(config, HttpHandlerMode.Add);
        /// 
        /// // This will automatically register HttpClient instances for all properties
        /// // in MyConfiguration that are marked with [HttpEndpoint]
        /// </code>
        /// </example>
        public static IServiceCollection AddHttpClients<TConfig, TMessageHandler>(this IServiceCollection services, TConfig config, HttpHandlerMode httpHandlerMode)
            where TMessageHandler : DelegatingHandler
            where TConfig : ConfigurationBase
        {
            if (httpHandlerMode != HttpHandlerMode.None)
            {
                services.TryAddScoped<TMessageHandler>();
            }

            var properties = typeof(TConfig).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(c => c.GetCustomAttributes<HttpEndpointAttribute>().Any());

            foreach (var property in properties)
            {
                var url = property.GetValue(config) as string;
                if (string.IsNullOrWhiteSpace(url)) continue;

                var attribute = property.GetCustomAttribute<HttpEndpointAttribute>();

                services
                    .AddHttpClient(typeof(TConfig).GetProperty(attribute.ClientNameProperty).GetValue(config) as string, 
                        client =>
                        {
                            client.BaseAddress = new Uri(url);
                            // RWM: Addresses accidental serial requests in Chrome.
                            // https://github.com/dotnet/aspnetcore/issues/26795#issuecomment-707356648
                            client.DefaultRequestHeaders.CacheControl = cacheControlHeaderValue;
                        })
                    .AddHttpMessageHandler<TMessageHandler>(httpHandlerMode);
            }

            return services;
        }

    }

}
