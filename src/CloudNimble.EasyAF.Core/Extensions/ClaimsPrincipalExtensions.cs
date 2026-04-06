using CloudNimble.EasyAF.Core;
using System.Collections.Generic;

namespace System.Security.Claims
{

    /// <summary>
    /// 
    /// </summary>
    public static class EasyAF_ClaimsPrincipalExtensions
    {

        #region Private Static Members

        internal static string _schemaUri;
        internal static string _idClaimName;

        #endregion

        /// <summary>
        /// Sets the SchemaUrl used <see langword="async"/>the basis for all custom claims.
        /// </summary>
        /// <param name="schemaUri"></param>
#pragma warning disable CA1054 // Uri parameters should not be strings
        public static void SetSchemaUri(string schemaUri)
#pragma warning restore CA1054 // Uri parameters should not be strings
        {
            _schemaUri = schemaUri;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idClaimName"></param>
        public static void SetIdClaimName(string idClaimName)
        {
            _idClaimName = idClaimName;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Initialize()
        {
            _schemaUri = "https://schemas.nimbleapps.cloud/identity/claims/";
            _idClaimName = "userid";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaUri"></param>
        /// <param name="idClaimName"></param>
        public static void Initialize(string schemaUri, string idClaimName)
        {
            _schemaUri = schemaUri;
            _idClaimName = idClaimName;
        }

        /// <summary>
        /// 
        /// </summary>
        public static string NameClaimType => $"{_schemaUri}{_idClaimName}";

        /// <summary>
        /// 
        /// </summary>
        public static string RoleClaimType => $"{_schemaUri}roles";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="claimsPrincipal">The ClaimsPrincipal instance to check for Claims. Should be <see cref="ClaimsPrincipal.Current"/>, except in unit testing.</param>
        /// <param name="claimType"></param>
        /// <returns></returns>
        public static IEnumerable<Claim> GetAllClaims(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            Ensure.ArgumentNotNull(claimsPrincipal, nameof(claimsPrincipal));

            // try to find the claim first
            if (claimsPrincipal.HasClaim(p => p.Type == claimType))
            {
                return claimsPrincipal.FindAll(claimType);
            }

            // try again to get the claim value
            return claimsPrincipal.FindAll($"{_schemaUri}{claimType}") ?? new List<Claim>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <param name="claimType"></param>
        /// <returns></returns>
        /// <exception cref="FormatException">
        /// If the <paramref name="claimType"/> is not formatted like a Guid (32 characters with 4 dashes), this exception will be thrown.
        /// </exception>
        public static Guid GetClaimGuid(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            // https://stackoverflow.com/questions/6915966/guid-parse-or-new-guid-whats-the-difference
            return new Guid(claimsPrincipal.GetClaimValue(claimType));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="claimsPrincipal">The ClaimsPrincipal instance to check for Claims. Should be <see cref="ClaimsPrincipal.Current"/>, except in unit testing.</param>
        /// <param name="claimType"></param>
        /// <returns></returns>
        public static string GetClaimValue(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            Ensure.ArgumentNotNull(claimsPrincipal, nameof(claimsPrincipal));

            // try to find the claim first
            if (claimsPrincipal.HasClaim(p => p.Type == claimType))
            {
                return claimsPrincipal.FindFirst(claimType)?.Value;
            }

            // try again to get the claim value
            return claimsPrincipal.FindFirst($"{_schemaUri}{claimType}")?.Value ?? string.Empty;
        }

        /// <summary>
        /// A shortcut for returning the AppUserProfileId for the current User.
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal instance we're extending.</param>
        /// <returns></returns>
        public static Guid GetIdClaim(this ClaimsPrincipal principal)
        {
            Ensure.ArgumentNotNull(principal, nameof(principal));
            var id = principal.HasClaim(c => c.Type == $"{_schemaUri}{_idClaimName}") ? principal.GetClaimValue($"{_schemaUri}{_idClaimName}") : principal.GetClaimValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(id) ? new Guid(id) : Guid.Empty;
        }

    }

}
