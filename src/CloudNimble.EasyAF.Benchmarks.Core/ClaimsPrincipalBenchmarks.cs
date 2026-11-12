using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace CloudNimble.EasyAF.Benchmarks
{

    /// <summary>
    /// Measures reading the current user's ID claim, which the Business managers do on every insert and update.
    /// </summary>
    [MemoryDiagnoser]
    public class ClaimsPrincipalBenchmarks
    {

        #region Fields

        private ClaimsPrincipal _guidUser;
        private ClaimsPrincipal _intUser;

        #endregion

        #region Setup

        /// <summary>
        /// Builds realistic principals: several standard claims, with the EasyAF user ID claim last.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            EasyAF_ClaimsPrincipalExtensions.Initialize();
            _guidUser = CreateUser("731c7991-8714-4a6a-a98f-311f6e79f742");
            _intUser = CreateUser("42");
        }

        /// <summary>
        /// Creates a principal with typical token claims and the given EasyAF user ID.
        /// </summary>
        /// <param name="userId">The value of the configured EasyAF ID claim.</param>
        /// <returns>The principal.</returns>
        private static ClaimsPrincipal CreateUser(string userId)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "robert@contoso.com"),
                new(ClaimTypes.Email, "robert@contoso.com"),
                new(ClaimTypes.GivenName, "Robert"),
                new(ClaimTypes.Surname, "McLaws"),
                new(ClaimTypes.Role, "Admin"),
                new(ClaimTypes.Role, "User"),
                new("iss", "https://login.contoso.com"),
                new("aud", "api"),
                new(ClaimTypes.NameIdentifier, "auth0|123"),
                new(EasyAF_ClaimsPrincipalExtensions.NameClaimType, userId),
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        }

        #endregion

        #region Benchmarks

        [Benchmark(Baseline = true)]
        public Guid GetIdClaim_V4() => V4.ClaimsPrincipalExtensions.GetIdClaim(_guidUser);

        [Benchmark]
        public Guid GetIdClaim_V5() => _guidUser.GetIdClaim();

        [Benchmark]
        public Guid TryGetIdClaim_Guid_V5() => _guidUser.TryGetIdClaim(out Guid id) ? id : Guid.Empty;

        [Benchmark]
        public int TryGetIdClaim_Int_V5() => _intUser.TryGetIdClaim(out int id) ? id : 0;

        #endregion

    }

}
