using CloudNimble.EasyAF.Core;
using CloudNimble.EasyAF.NewtonsoftJson.Compatibility;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace System.Net.Http
{

    /// <summary>
    /// Provides extension methods for HttpResponseMessage to deserialize JSON responses using Newtonsoft.Json.
    /// Includes support for both success and error response handling with automatic contract resolver configuration.
    /// </summary>
    public static class EasyAF_Http_NewtonsoftJson_HttpResponseMessageExtensions
    {

        /// <summary>
        /// Deserializes the HTTP response message content to the specified type using Newtonsoft.Json with default settings.
        /// Returns either the deserialized response or error content as a string.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response content to.</typeparam>
        /// <param name="message">The HTTP response message to deserialize.</param>
        /// <returns>A tuple containing either the deserialized response object or error content string.</returns>
        public static async Task<(T Response, string ErrorContent)> DeserializeResponseAsync<T>(this HttpResponseMessage message)
        {
            Ensure.ArgumentNotNull(message, nameof(message));

            if (!message.IsSuccessStatusCode)
            {
                return (default, await message.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            var content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
            return !string.IsNullOrWhiteSpace(content) ? (JsonConvert.DeserializeObject<T>(content, SystemTextJsonContractResolver.GetJsonSerializerSettings()), null) : (default(T), null);
        }

        /// <summary>
        /// Deserializes the HTTP response message content to the specified type using Newtonsoft.Json with custom settings.
        /// Automatically configures SystemTextJsonContractResolver if not already set. Returns either the deserialized response or error content as a string.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response content to.</typeparam>
        /// <param name="message">The HTTP response message to deserialize.</param>
        /// <param name="settings">The JSON serializer settings to use for deserialization.</param>
        /// <returns>A tuple containing either the deserialized response object or error content string.</returns>
        public static async Task<(T Response, string ErrorContent)> DeserializeResponseAsync<T>(this HttpResponseMessage message, JsonSerializerSettings settings)
        {
            Ensure.ArgumentNotNull(message, nameof(message));

            if (!message.IsSuccessStatusCode)
            {
                return (default, await message.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            var content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (settings.ContractResolver is null || !settings.ContractResolver.GetType().IsAssignableFrom(typeof(SystemTextJsonContractResolver)))
            {
                settings.ContractResolver = new SystemTextJsonContractResolver();
            }
            return !string.IsNullOrWhiteSpace(content) ? (JsonConvert.DeserializeObject<T>(content, settings), null) : (default(T), null);
        }

        /// <summary>
        /// Deserializes the HTTP response message content to strongly-typed response and error objects using Newtonsoft.Json with default settings.
        /// Provides type-safe error handling by deserializing error responses to a specific error type.
        /// </summary>
        /// <typeparam name="TResponse">The type to deserialize successful response content to.</typeparam>
        /// <typeparam name="TError">The type to deserialize error response content to.</typeparam>
        /// <param name="message">The HTTP response message to deserialize.</param>
        /// <returns>A tuple containing either the deserialized response object or deserialized error object.</returns>
        public static async Task<(TResponse Response, TError ErrorContent)> DeserializeResponseAsync<TResponse, TError>(this HttpResponseMessage message)
        {
            Ensure.ArgumentNotNull(message, nameof(message));

            var content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!message.IsSuccessStatusCode)
            {
                return (default, JsonConvert.DeserializeObject<TError>(content));
            }
            return !string.IsNullOrWhiteSpace(content) ? (JsonConvert.DeserializeObject<TResponse>(content, SystemTextJsonContractResolver.GetJsonSerializerSettings()), default) : (default(TResponse), default(TError));
        }

        /// <summary>
        /// Deserializes the HTTP response message content to strongly-typed response and error objects using Newtonsoft.Json with custom settings.
        /// Automatically configures SystemTextJsonContractResolver if not already set. Provides type-safe error handling by deserializing error responses to a specific error type.
        /// </summary>
        /// <typeparam name="TResponse">The type to deserialize successful response content to.</typeparam>
        /// <typeparam name="TError">The type to deserialize error response content to.</typeparam>
        /// <param name="message">The HTTP response message to deserialize.</param>
        /// <param name="settings">The JSON serializer settings to use for deserialization.</param>
        /// <returns>A tuple containing either the deserialized response object or deserialized error object.</returns>
        public static async Task<(TResponse Response, TError ErrorContent)> DeserializeResponseAsync<TResponse, TError>(this HttpResponseMessage message, JsonSerializerSettings settings)
        {
            Ensure.ArgumentNotNull(message, nameof(message));

            var content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!message.IsSuccessStatusCode)
            {
                return (default, JsonConvert.DeserializeObject<TError>(content));
            }

            if (settings.ContractResolver is null || !settings.ContractResolver.GetType().IsAssignableFrom(typeof(SystemTextJsonContractResolver)))
            {
                settings.ContractResolver = new SystemTextJsonContractResolver();
            }
            return !string.IsNullOrWhiteSpace(content) ? (JsonConvert.DeserializeObject<TResponse>(content, settings), default) : (default(TResponse), default(TError));
        }

    }

}
