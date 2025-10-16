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

    }

}
