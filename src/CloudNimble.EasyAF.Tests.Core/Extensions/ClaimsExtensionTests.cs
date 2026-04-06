using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Security.Claims;

namespace CloudNimble.EasyAF.Tests.Core
{

    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class ClaimsExtensionTests
    {

        [TestMethod]
        public void ClaimsExtensions_SingleRoleClaim_ProcessedCorrectly()
        {
            EasyAF_ClaimsPrincipalExtensions.SetSchemaUri("https://schemas.nimbleapps.cloud/identity/claims/");
            var claims = new List<Claim>
            {
                new Claim("https://schemas.nimbleapps.cloud/identity/claims/roles", "beta"),
                new Claim("https://schemas.nimbleapps.cloud/identity/claims/userid", "731c7991-8714-4a6a-a98f-311f6e79f742")
            };
            claims = claims.GetStandardizedClaims();
            claims[0].Type.Should().Be("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            claims[0].Value.Should().Be("beta");
            claims[1].Type.Should().Be("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        }

        [TestMethod]
        public void ClaimsExtensions_SingleRoleClaim_InArray_ProcessedCorrectly()
        {
            EasyAF_ClaimsPrincipalExtensions.SetSchemaUri("https://schemas.nimbleapps.cloud/identity/claims/");
            var claims = new List<Claim>
            {
                new Claim("https://schemas.nimbleapps.cloud/identity/claims/roles", "[\"beta\"]"),
                new Claim("https://schemas.nimbleapps.cloud/identity/claims/userid", "731c7991-8714-4a6a-a98f-311f6e79f742")
            };
            claims = claims.GetStandardizedClaims();
            claims[0].Type.Should().Be("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            claims[0].Value.Should().Be("beta");
            claims[1].Type.Should().Be("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        }

        [TestMethod]
        public void ClaimsExtensions_MultipleRoleClaims_InArray_ProcessedCorrectly()
        {
            EasyAF_ClaimsPrincipalExtensions.SetSchemaUri("https://schemas.nimbleapps.cloud/identity/claims/");
            var claims = new List<Claim>
            {
                new Claim("https://schemas.nimbleapps.cloud/identity/claims/roles", "[\"beta\", \"gamma\"]"),
                new Claim("https://schemas.nimbleapps.cloud/identity/claims/userid", "731c7991-8714-4a6a-a98f-311f6e79f742")
            };
            claims = claims.GetStandardizedClaims();
            claims[0].Type.Should().Be("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            claims[0].Value.Should().Be("beta");
            claims[1].Type.Should().Be("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            claims[1].Value.Should().Be("gamma");
            claims[2].Type.Should().Be("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        }

    }

}
