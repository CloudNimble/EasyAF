using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using CloudNimble.EasyAF.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudNimble.EasyAF.Tests.Http
{

    /// <summary>
    /// <see cref="InnerHttpContext"/> copies origin and headers from the outer request.
    /// </summary>
    [TestClass]
    public class InnerHttpContextTests
    {

        #region Public Methods

        /// <summary>
        /// Method, path, query, and body are applied; a path without a leading slash is prefixed.
        /// </summary>
        [TestMethod]
        public void ConfigureRequest_SetsMethodPathQueryAndBody()
        {
            var inner = new DefaultHttpContext();
            var bytes = Encoding.UTF8.GetBytes("{\"name\":\"Contoso\"}");

            InnerHttpContext.ConfigureRequest(inner, "POST", "odata/Customers", new QueryString("?$select=Name"), new MemoryStream(bytes), "application/json");

            inner.Request.Method.Should().Be("POST");
            inner.Request.Path.Value.Should().Be("/odata/Customers");
            inner.Request.QueryString.Value.Should().Be("?$select=Name");
            inner.Request.ContentType.Should().Be("application/json");
            inner.Request.ContentLength.Should().Be(bytes.Length);
        }

        /// <summary>
        /// Authorization is copied; Host, Content-Type, and If-Match are not.
        /// </summary>
        [TestMethod]
        public void CopyFromOuter_CopiesAuthorization_SkipsHopByHopAndConditionals()
        {
            var outer = new DefaultHttpContext();
            outer.Request.Scheme = "https";
            outer.Request.Host = new HostString("api.contoso.com");
            outer.Request.PathBase = "/app";
            outer.Request.Headers["Authorization"] = "Bearer token";
            outer.Request.Headers["Host"] = "api.contoso.com";
            outer.Request.Headers["Content-Type"] = "application/json";
            outer.Request.Headers["If-Match"] = "\"etag\"";
            outer.User = new ClaimsPrincipal(new ClaimsIdentity("test"));
            var inner = new DefaultHttpContext();

            InnerHttpContext.CopyFromOuter(inner, outer);

            inner.Request.Scheme.Should().Be("https");
            inner.Request.Host.Host.Should().Be("api.contoso.com");
            inner.Request.PathBase.Value.Should().Be("/app");
            inner.Request.Headers["Authorization"].ToString().Should().Be("Bearer token");
            inner.Request.Headers.ContainsKey("Content-Type").Should().BeFalse();
            inner.Request.Headers.ContainsKey("If-Match").Should().BeFalse();
            inner.User.Identity.AuthenticationType.Should().Be("test");
        }

        /// <summary>
        /// With no outer request, the inner call is <c>http://localhost</c> and an empty principal.
        /// </summary>
        [TestMethod]
        public void CopyFromOuter_NullOuter_UsesHttpLocalhost()
        {
            var inner = new DefaultHttpContext();

            InnerHttpContext.CopyFromOuter(inner, null);

            inner.Request.Scheme.Should().Be("http");
            inner.Request.Host.ToString().Should().Be("localhost");
            inner.Request.PathBase.Value.Should().BeEmpty();
            inner.User.Should().NotBeNull();
        }

        /// <summary>
        /// A null inner context is rejected.
        /// </summary>
        [TestMethod]
        public void CopyFromOuter_NullInner_Throws()
        {
            var act = () => InnerHttpContext.CopyFromOuter(null, new DefaultHttpContext());

            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

    }

}
