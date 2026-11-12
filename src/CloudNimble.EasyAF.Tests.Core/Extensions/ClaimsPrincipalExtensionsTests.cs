using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace CloudNimble.EasyAF.Tests.Core
{

    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class ClaimsPrincipalExtensionsTests
    {

        [TestMethod]
        public void GetIdClaim_ReturnsNameIdentifier()
        {
            const string schemaUri = "https://schemas.nimbleapps.cloud/identity/claims/";
            EasyAF_ClaimsPrincipalExtensions.SetSchemaUri(schemaUri);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "731c7991-8714-4a6a-a98f-311f6e79f742"),
            };

            var identity = new ClaimsIdentity(claims, "", ClaimTypes.Name, ClaimTypes.Role);
            identity.StandardizeClaims();
            var principal = new ClaimsPrincipal(identity);
            principal.GetIdClaim().Should().Be(new Guid("731c7991-8714-4a6a-a98f-311f6e79f742"));
        }

        [TestMethod]
        public void GetIdClaim_ReturnsCloudNimbleId()
        {
            const string schemaUri = "https://schemas.nimbleapps.cloud/identity/claims/";
            EasyAF_ClaimsPrincipalExtensions.SetSchemaUri(schemaUri);
            var claims = new List<Claim>
            {
                new Claim("https://schemas.nimbleapps.cloud/identity/claims/userid", "731c7991-8714-4a6a-a98f-311f6e79f742"),
            };

            var identity = new ClaimsIdentity(claims, "", ClaimTypes.Name, ClaimTypes.Role);
            identity.StandardizeClaims();
            var principal = new ClaimsPrincipal(identity);
            principal.GetIdClaim().Should().Be(new Guid("731c7991-8714-4a6a-a98f-311f6e79f742"));
        }

        [TestMethod]
        public void GetIdClaim_WithDifferentClaimName_ReturnsCloudNimbleId()
        {
            const string schemaUri = "https://schemas.nimbleapps.cloud/identity/claims/";
            EasyAF_ClaimsPrincipalExtensions.SetSchemaUri(schemaUri);
            EasyAF_ClaimsPrincipalExtensions.SetIdClaimName("UserId");
            var claims = new List<Claim>
            {
                new Claim("https://schemas.nimbleapps.cloud/identity/claims/UserId", "731c7991-8714-4a6a-a98f-311f6e79f742"),
            };

            var identity = new ClaimsIdentity(claims, "", ClaimTypes.Name, ClaimTypes.Role);
            identity.StandardizeClaims();
            var principal = new ClaimsPrincipal(identity);
            principal.GetIdClaim().Should().Be(new Guid("731c7991-8714-4a6a-a98f-311f6e79f742"));
        }

        [TestMethod]
        public void GetIdClaim_Initialize_ReturnsCloudNimbleId()
        {
            const string schemaUri = "https://schemas.nimbleapps.cloud/identity/claims/";
            EasyAF_ClaimsPrincipalExtensions.Initialize(schemaUri, "UserId");
            var claims = new List<Claim>
            {
                new Claim("https://schemas.nimbleapps.cloud/identity/claims/UserId", "731c7991-8714-4a6a-a98f-311f6e79f742"),
            };

            var identity = new ClaimsIdentity(claims, "", ClaimTypes.Name, ClaimTypes.Role);
            identity.StandardizeClaims();
            var principal = new ClaimsPrincipal(identity);
            principal.GetIdClaim().Should().Be(new Guid("731c7991-8714-4a6a-a98f-311f6e79f742"));
        }

        [TestMethod]
        public void GetIdClaim_WithSchemaPrefixedNameIdentifier_ReturnsId()
        {
            const string schemaUri = "https://schemas.nimbleapps.cloud/identity/claims/";
            EasyAF_ClaimsPrincipalExtensions.Initialize(schemaUri, "userid");
            var claims = new List<Claim>
            {
                new Claim($"{schemaUri}{ClaimTypes.NameIdentifier}", "731c7991-8714-4a6a-a98f-311f6e79f742"),
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "", ClaimTypes.Name, ClaimTypes.Role));
            principal.GetIdClaim().Should().Be(new Guid("731c7991-8714-4a6a-a98f-311f6e79f742"));
        }

        [TestMethod]
        public void GetIdClaim_WithNoIdClaim_ReturnsEmpty()
        {
            EasyAF_ClaimsPrincipalExtensions.Initialize();
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.Email, "robert@contoso.com") }, ""));

            principal.GetIdClaim().Should().Be(Guid.Empty);
        }

        #region TryGetIdClaim Tests

        /// <summary>
        /// Creates a principal whose only claim is a NameIdentifier, which GetIdClaim falls back to regardless of the configured claim name.
        /// </summary>
        /// <param name="value">The NameIdentifier value, or <see langword="null"/> for a principal with no claims.</param>
        /// <returns>The principal.</returns>
        private static ClaimsPrincipal PrincipalWithNameIdentifier(string value)
        {
            var claims = value is null ? new List<Claim>() : new List<Claim> { new Claim(ClaimTypes.NameIdentifier, value) };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "", ClaimTypes.Name, ClaimTypes.Role));
        }

        [TestMethod]
        public void TryGetIdClaim_WithGuidClaim_ShouldReturnTheGuid()
        {
            var principal = PrincipalWithNameIdentifier("731c7991-8714-4a6a-a98f-311f6e79f742");

            principal.TryGetIdClaim(out Guid id).Should().BeTrue();
            id.Should().Be(new Guid("731c7991-8714-4a6a-a98f-311f6e79f742"));
        }

        [TestMethod]
        public void TryGetIdClaim_WithIntClaim_ShouldReturnTheInt()
        {
            var principal = PrincipalWithNameIdentifier("42");

            principal.TryGetIdClaim(out int id).Should().BeTrue();
            id.Should().Be(42);
        }

        [TestMethod]
        public void TryGetIdClaim_WithLongClaim_ShouldReturnTheLong()
        {
            var principal = PrincipalWithNameIdentifier("9000000000");

            principal.TryGetIdClaim(out long id).Should().BeTrue();
            id.Should().Be(9000000000L);
        }

        [TestMethod]
        public void TryGetIdClaim_WithNoClaim_ShouldReturnFalse()
        {
            var principal = PrincipalWithNameIdentifier(null);

            principal.TryGetIdClaim(out Guid id).Should().BeFalse();
            id.Should().Be(Guid.Empty);
        }

        [TestMethod]
        public void TryGetIdClaim_WithValueOfTheWrongType_ShouldReturnFalse()
        {
            var principal = PrincipalWithNameIdentifier("731c7991-8714-4a6a-a98f-311f6e79f742");

            principal.TryGetIdClaim(out int id).Should().BeFalse();
            id.Should().Be(0);
        }

        [TestMethod]
        public void TryGetIdClaim_WithNullPrincipal_ShouldThrowArgumentNullException()
        {
            ClaimsPrincipal principal = null;

            Action act = () => principal.TryGetIdClaim(out Guid _);

            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

    }

}
