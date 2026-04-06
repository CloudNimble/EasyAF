using CloudNimble.EasyAF.Core;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{

    /// <summary>
    /// Provides extension methods for IHttpClientBuilder to configure message handlers based on HttpHandlerMode.
    /// Enables flexible configuration of HTTP message handler pipelines for different scenarios.
    /// </summary>
    public static class EasyAF_Http_IHttpClientBuilderExtensions
    {

        /// <summary>
        /// Given the <see cref="HttpHandlerMode"/>, adds the specified <typeparamref name="THandler"/> to the beginning or end of the pipeline.
        /// </summary>
        /// <typeparam name="THandler">The <see cref="DelegatingHandler"/> type to pull from the scoped <see cref="ServiceProvider"/>.</typeparam>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/> instance to extend.</param>
        /// <param name="mode">A <see cref="HttpHandlerMode"/> specifying whether we are making this handler the first one in the pipeline, or the last.</param>
        /// <returns>The IHttpClientBuilder instance for method chaining.</returns>
        public static IHttpClientBuilder AddHttpMessageHandler<THandler>(this IHttpClientBuilder builder, HttpHandlerMode mode)
            where THandler : DelegatingHandler

        {
            if (mode == HttpHandlerMode.None) return builder;
            return mode == HttpHandlerMode.Add ? builder.AddHttpMessageHandler<THandler>() : builder.ConfigurePrimaryHttpMessageHandler<THandler>();
        }

    }

}
