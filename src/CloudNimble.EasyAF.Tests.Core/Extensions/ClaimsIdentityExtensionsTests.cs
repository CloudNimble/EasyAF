using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace CloudNimble.EasyAF.Tests.Core
{

    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class ClaimsIdentityExtensionsTests
    {

        [TestMethod]
        public void RoleClaimsProcessedCorrectly()
        {
            const string schemaUri = "https://schemas.nimbleapps.cloud/identity/claims/";
            EasyAF_ClaimsPrincipalExtensions.SetSchemaUri(schemaUri);
            var claims = new List<Claim>
            {
                new Claim("https://schemas.nimbleapps.cloud/identity/claims/roles", "beta"),
                new Claim("https://schemas.nimbleapps.cloud/identity/claims/userid", "731c7991-8714-4a6a-a98f-311f6e79f742"),
                new Claim("https://schemas.nimbleapps.cloud/identity/claims/userid", "test"),
                new Claim("role", "admin"),
                new Claim(ClaimTypes.Email, "test"),
                new Claim(ClaimTypes.Email, "test2"),
            };

            var identity = new ClaimsIdentity(claims, "", ClaimTypes.Name, ClaimTypes.Role);
            identity.StandardizeClaims();
            identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier).Should().BeTrue();
            identity.HasClaim(c => c.Type == $"{schemaUri}userid").Should().BeTrue();
            identity.FindAll($"{schemaUri}userid").Should().NotBeEmpty().And.HaveCount(2);
            identity.FindAll(ClaimTypes.Email).Should().NotBeEmpty().And.HaveCount(1);
            var principal = new ClaimsPrincipal(identity);
            principal.IsInRole("admin").Should().BeTrue();
        }

        [TestMethod]
        public void MultipleRolesInSameClaimProcessedCorrectly()
        {
            const string schemaUri = "https://schemas.nimbleapps.cloud/identity/claims/";
            EasyAF_ClaimsPrincipalExtensions.SetSchemaUri(schemaUri);
            var claims = new List<Claim>
            {
                new Claim("https://schemas.nimbleapps.cloud/identity/claims/roles", "[\"beta\",\"admin\"]"),
            };

            var identity = new ClaimsIdentity(claims, "", ClaimTypes.Name, ClaimTypes.Role);
            identity.StandardizeClaims();
            identity.Claims.ToList().Should().HaveCount(3);
            var principal = new ClaimsPrincipal(identity);
            principal.IsInRole("admin").Should().BeTrue();
        }

    }

}
