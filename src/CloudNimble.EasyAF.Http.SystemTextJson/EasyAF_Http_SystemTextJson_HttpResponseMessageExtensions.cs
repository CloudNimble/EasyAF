using CloudNimble.EasyAF.Core;
using System.Text.Json;
using System.Threading.Tasks;

namespace System.Net.Http
{

    /// <summary>
    /// Provides extension methods for HttpResponseMessage to deserialize JSON responses using System.Text.Json.
    /// Includes support for both success and error response handling with configurable serializer options.
    /// </summary>
    public static class EasyAF_Http_SystemTextJson_HttpResponseMessageExtensions
    {

        /// <summary>
        /// Deserializes the HTTP response message content to the specified type using System.Text.Json with default options.
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
            return !string.IsNullOrWhiteSpace(content) ? (JsonSerializer.Deserialize<T>(content), null) : (default(T), null);
        }

        /// <summary>
        /// Deserializes the HTTP response message content to the specified type using System.Text.Json with custom options.
        /// Returns either the deserialized response or error content as a string.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response content to.</typeparam>
        /// <param name="message">The HTTP response message to deserialize.</param>
        /// <param name="settings">The JSON serializer options to use for deserialization.</param>
        /// <returns>A tuple containing either the deserialized response object or error content string.</returns>
        public static async Task<(T Response, string ErrorContent)> DeserializeResponseAsync<T>(this HttpResponseMessage message, JsonSerializerOptions settings)
        {
            Ensure.ArgumentNotNull(message, nameof(message));

            if (!message.IsSuccessStatusCode)
            {
                return (default, await message.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            var content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
            return !string.IsNullOrWhiteSpace(content) ? (JsonSerializer.Deserialize<T>(content, settings), null) : (default(T), null);
        }

        /// <summary>
        /// Deserializes the HTTP response message content to strongly-typed response and error objects using System.Text.Json with default options.
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
                return (default, JsonSerializer.Deserialize<TError>(content));
            }
            return !string.IsNullOrWhiteSpace(content) ? (JsonSerializer.Deserialize<TResponse>(content), default) : (default(TResponse), default(TError));
        }

        /// <summary>
        /// Deserializes the HTTP response message content to strongly-typed response and error objects using System.Text.Json with custom options.
        /// Provides type-safe error handling by deserializing error responses to a specific error type.
        /// </summary>
        /// <typeparam name="TResponse">The type to deserialize successful response content to.</typeparam>
        /// <typeparam name="TError">The type to deserialize error response content to.</typeparam>
        /// <param name="message">The HTTP response message to deserialize.</param>
        /// <param name="settings">The JSON serializer options to use for deserialization.</param>
        /// <returns>A tuple containing either the deserialized response object or deserialized error object.</returns>
        public static async Task<(TResponse Response, TError ErrorContent)> DeserializeResponseAsync<TResponse, TError>(this HttpResponseMessage message, JsonSerializerOptions settings)
        {
            Ensure.ArgumentNotNull(message, nameof(message));

            var content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!message.IsSuccessStatusCode)
            {
                return (default, JsonSerializer.Deserialize<TError>(content));
            }
            return !string.IsNullOrWhiteSpace(content) ? (JsonSerializer.Deserialize<TResponse>(content, settings), default) : (default(TResponse), default(TError));
        }

    }

}
