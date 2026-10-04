using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CloudNimble.EasyAF.Http
{

    /// <summary>
    /// Copies origin, identity, and headers from an outer <see cref="HttpContext"/> onto an inner
    /// in-process request so the inner pipeline sees the public URL and the same user.
    /// </summary>
    /// <remarks>
    /// The caller constructs the inner <see cref="HttpContext"/> (typically <c>new DefaultHttpContext()</c> in
    /// an ASP.NET Core app, or <see cref="IHttpContextFactory.Create"/>). This type stays on
    /// <c>Microsoft.AspNetCore.Http.Abstractions</c> and does not construct a context.
    /// </remarks>
    /// <example>
    /// <code>
    /// var inner = new DefaultHttpContext
    /// {
    ///     RequestServices = scope.ServiceProvider,
    ///     RequestAborted = cancellationToken
    /// };
    /// InnerHttpContext.CopyFromOuter(inner, outer);
    /// InnerHttpContext.ConfigureRequest(inner, "GET", "/odata/Customers", QueryString.Empty);
    /// inner.Request.Headers["Accept"] = "application/json";
    /// inner.Response.Body = new MemoryStream();
    /// </code>
    /// </example>
    public static class InnerHttpContext
    {

        #region Fields

        /// <summary>
        /// Request headers that must not be replayed onto the inner call.
        /// Hop-by-hop, entity-body, and conditionals that describe the outer request rather than the inner resource.
        /// </summary>
        public static readonly HashSet<string> SkippedRequestHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Accept",
            "Accept-Encoding",
            "Connection",
            "Content-Encoding",
            "Content-Length",
            "Content-Type",
            "Expect",
            "Host",
            "If-Match",
            "If-Modified-Since",
            "If-None-Match",
            "If-Range",
            "If-Unmodified-Since",
            "Keep-Alive",
            "Proxy-Authenticate",
            "Proxy-Authorization",
            "Range",
            "TE",
            "Trailer",
            "Transfer-Encoding",
            "Upgrade"
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets method, path, and query on <paramref name="inner"/>, and optionally replaces the request body.
        /// </summary>
        /// <param name="inner">The inner context to configure.</param>
        /// <param name="method">The HTTP method, for example <c>GET</c>.</param>
        /// <param name="path">The request path, including a leading slash.</param>
        /// <param name="query">The query string, or <see cref="QueryString.Empty"/>.</param>
        /// <param name="body">An optional replacement request body. When <see langword="null"/>, the body is left unchanged.</param>
        /// <param name="contentType">The content type to set when <paramref name="body"/> is provided.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="method"/> or <paramref name="path"/> is blank.</exception>
        public static void ConfigureRequest(HttpContext inner, string method, string path, QueryString query, Stream body = null, string contentType = null)
        {
            ArgumentNullException.ThrowIfNull(inner);
            ArgumentException.ThrowIfNullOrWhiteSpace(method);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            inner.Request.Method = method;
            inner.Request.Path = path.StartsWith('/') ? path : "/" + path;
            inner.Request.QueryString = query;

            if (body is null)
            {
                return;
            }

            inner.Request.Body = body;
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                inner.Request.ContentType = contentType;
            }

            if (body.CanSeek)
            {
                inner.Request.ContentLength = body.Length;
            }
        }

        /// <summary>
        /// Copies scheme, host, path base, user, and non-skipped headers from <paramref name="outer"/> onto
        /// <paramref name="inner"/>. When <paramref name="outer"/> is <see langword="null"/>, uses
        /// <c>http</c> / <c>localhost</c> and an empty principal.
        /// </summary>
        /// <param name="inner">The inner context to fill.</param>
        /// <param name="outer">The incoming context, or <see langword="null"/> when there is no outer request.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// Scheme, host, and path base come from the outer request so generated links match the public URL.
        /// Other request headers are copied except hop-by-hop, entity-body, and HTTP conditionals
        /// (see <see cref="SkippedRequestHeaders"/>).
        /// </remarks>
        public static void CopyFromOuter(HttpContext inner, HttpContext outer)
        {
            ArgumentNullException.ThrowIfNull(inner);

            if (outer is null)
            {
                inner.Request.Scheme = "http";
                inner.Request.Host = new HostString("localhost");
                inner.Request.PathBase = PathString.Empty;
                inner.User = new ClaimsPrincipal();

                return;
            }

            inner.Request.Scheme = string.IsNullOrWhiteSpace(outer.Request.Scheme) ? "http" : outer.Request.Scheme;
            inner.Request.Host = outer.Request.Host.HasValue ? outer.Request.Host : new HostString("localhost");
            inner.Request.PathBase = outer.Request.PathBase;
            inner.User = outer.User ?? new ClaimsPrincipal();
            CopyIncomingHeaders(inner, outer);
        }

        /// <summary>
        /// Copies outer request headers onto the inner call, skipping <see cref="SkippedRequestHeaders"/>.
        /// </summary>
        /// <param name="inner">The inner context.</param>
        /// <param name="outer">The incoming context.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> or <paramref name="outer"/> is <see langword="null"/>.</exception>
        public static void CopyIncomingHeaders(HttpContext inner, HttpContext outer)
        {
            ArgumentNullException.ThrowIfNull(inner);
            ArgumentNullException.ThrowIfNull(outer);

            foreach (var header in outer.Request.Headers)
            {
                if (SkippedRequestHeaders.Contains(header.Key))
                {
                    continue;
                }

                inner.Request.Headers[header.Key] = header.Value;
            }
        }

        #endregion

    }

}
