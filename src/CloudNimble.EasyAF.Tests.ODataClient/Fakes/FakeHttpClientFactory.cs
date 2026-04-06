using System;
using System.Net.Http;

namespace CloudNimble.EasyAF.Tests.OData.Fakes
{

    /// <summary>
    /// A fake <see cref="IHttpClientFactory"/> to support testing dependency injection.
    /// </summary>
    public class FakeHttpClientFactory : IHttpClientFactory
    {

        /// <summary>
        /// Delegate function to invoke when creating an <see cref="HttpClient"/>.
        /// </summary>
        Func<HttpClient> _httpClientFunctionDelegate;

        /// <summary>
        /// Constructor overload to accept delegate <see cref="Func{T, TResult}"/>.
        /// </summary>
        /// <param name="functionDelegate"></param>
        public FakeHttpClientFactory(Func<HttpClient> functionDelegate)
        {
            _httpClientFunctionDelegate = functionDelegate;
        }

        /// <summary>
        /// Invokes the delegate function to create an <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public HttpClient CreateClient(string name)
        {
            return _httpClientFunctionDelegate.Invoke();
        }

    }

}
