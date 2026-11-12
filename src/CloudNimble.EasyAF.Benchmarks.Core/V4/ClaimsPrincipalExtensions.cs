using System;
using System.Security.Claims;

namespace CloudNimble.EasyAF.Benchmarks.V4
{

    /// <summary>
    /// The EasyAF 4.x EasyAF_ClaimsPrincipalExtensions.GetIdClaim and GetClaimValue, copied verbatim. They rebuilt the claim type on every call.
    /// </summary>
    /// <remarks>
    /// These are not extension methods, so they don't collide with the EasyAF 5.0 extensions of the same name.
    /// </remarks>
    public static class ClaimsPrincipalExtensions
    {

        #region Private Static Members

        // RWM: Static fields rather than constants, so the interpolation happens at runtime like it did in 4.x.
        private static readonly string _schemaUri = "https://schemas.nimbleapps.cloud/identity/claims/";
        private static readonly string _idClaimName = "userid";

        #endregion

        #region Public Methods

        /// <summary>
        /// The EasyAF 4.x GetIdClaim.
        /// </summary>
        /// <param name="principal">The user.</param>
        /// <returns>The user's ID, or <see cref="Guid.Empty"/>.</returns>
        public static Guid GetIdClaim(ClaimsPrincipal principal)
        {
            var id = principal.HasClaim(c => c.Type == $"{_schemaUri}{_idClaimName}") ? GetClaimValue(principal, $"{_schemaUri}{_idClaimName}") : GetClaimValue(principal, ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(id) ? new Guid(id) : Guid.Empty;
        }

        /// <summary>
        /// The EasyAF 4.x GetClaimValue.
        /// </summary>
        /// <param name="claimsPrincipal">The user.</param>
        /// <param name="claimType">The claim type, with or without the schema URI.</param>
        /// <returns>The claim's value, or an empty string.</returns>
        public static string GetClaimValue(ClaimsPrincipal claimsPrincipal, string claimType)
        {
            if (claimsPrincipal.HasClaim(p => p.Type == claimType))
            {
                return claimsPrincipal.FindFirst(claimType)?.Value;
            }

            return claimsPrincipal.FindFirst($"{_schemaUri}{claimType}")?.Value ?? string.Empty;
        }

        #endregion

    }

}
