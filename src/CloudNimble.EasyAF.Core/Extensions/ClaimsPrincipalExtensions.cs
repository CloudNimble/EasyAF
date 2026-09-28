using CloudNimble.EasyAF.Core;
using System.Collections.Generic;
using System.Globalization;

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

        // RWM: The ID claim is read on every insert and update, so build these claim types once instead of on every lookup.
        private static string _idClaimType;
        private static string _schemaNameIdentifierClaimType;

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
            RefreshIdClaimTypes();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="idClaimName"></param>
        public static void SetIdClaimName(string idClaimName)
        {
            _idClaimName = idClaimName;
            RefreshIdClaimTypes();
        }

        /// <summary>
        ///
        /// </summary>
        public static void Initialize()
        {
            _schemaUri = "https://schemas.nimbleapps.cloud/identity/claims/";
            _idClaimName = "userid";
            RefreshIdClaimTypes();
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
            RefreshIdClaimTypes();
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
            var id = principal.GetIdClaimValue();
            return !string.IsNullOrWhiteSpace(id) ? new Guid(id) : Guid.Empty;
        }

        /// <summary>
        /// Gets the current User's ID claim as a <see cref="Guid"/>, without throwing when the claim is missing or malformed.
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal instance we're extending.</param>
        /// <param name="id">The ID, or <see cref="Guid.Empty"/> when the claim is missing or not a valid <see cref="Guid"/>.</param>
        /// <returns><see langword="true"/> if the claim exists and is a valid <see cref="Guid"/>; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetIdClaim(this ClaimsPrincipal principal, out Guid id)
        {
            Ensure.ArgumentNotNull(principal, nameof(principal));
            return Guid.TryParse(principal.GetIdClaimValue(), out id);
        }

        /// <summary>
        /// Gets the current User's ID claim as an <see cref="int"/>, for applications whose user IDs are integers.
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal instance we're extending.</param>
        /// <param name="id">The ID, or 0 when the claim is missing or not a valid <see cref="int"/>.</param>
        /// <returns><see langword="true"/> if the claim exists and is a valid <see cref="int"/>; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetIdClaim(this ClaimsPrincipal principal, out int id)
        {
            Ensure.ArgumentNotNull(principal, nameof(principal));
            return int.TryParse(principal.GetIdClaimValue(), NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
        }

        /// <summary>
        /// Gets the current User's ID claim as a <see cref="long"/>, for applications whose user IDs are 64-bit integers.
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal instance we're extending.</param>
        /// <param name="id">The ID, or 0 when the claim is missing or not a valid <see cref="long"/>.</param>
        /// <returns><see langword="true"/> if the claim exists and is a valid <see cref="long"/>; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetIdClaim(this ClaimsPrincipal principal, out long id)
        {
            Ensure.ArgumentNotNull(principal, nameof(principal));
            return long.TryParse(principal.GetIdClaimValue(), NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
        }

        /// <summary>
        /// Gets the raw value of the configured ID claim, falling back to <see cref="ClaimTypes.NameIdentifier"/>.
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal instance we're extending.</param>
        /// <returns>The claim value, or an empty string if neither claim exists.</returns>
        private static string GetIdClaimValue(this ClaimsPrincipal principal)
        {
            // RWM: Same order as before (configured ID claim, NameIdentifier, schema-prefixed NameIdentifier), without per-claim
            //      string building or closures. This runs on every insert and update.
            if (_idClaimType is null)
            {
                RefreshIdClaimTypes();
            }

            return principal.FindFirst(_idClaimType)?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst(_schemaNameIdentifierClaimType)?.Value
                ?? string.Empty;
        }

        /// <summary>
        /// Rebuilds the cached ID claim types after the schema URI or ID claim name changes.
        /// </summary>
        private static void RefreshIdClaimTypes()
        {
            _idClaimType = $"{_schemaUri}{_idClaimName}";
            _schemaNameIdentifierClaimType = $"{_schemaUri}{ClaimTypes.NameIdentifier}";
        }

    }

}
