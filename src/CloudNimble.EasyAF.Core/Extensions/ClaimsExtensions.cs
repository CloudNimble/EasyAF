using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace System.Collections.Generic
{

    /// <summary>
    /// 
    /// </summary>
    public static class EasyAF_ClaimsExtensions
    {

        private static readonly List<string> ClaimTypesForUserId = new List<string> { "userid" };
        private static readonly List<string> ClaimTypesForRoles = new List<string> { "roles", "role" };
        private static readonly string[] ClaimTypesForEmail = { "emails", "email" };
        private static readonly string[] ClaimTypesForGivenName = { "givenname", "firstname" };
        private static readonly string[] ClaimTypesForFamilyName = { "familyname", "lastname", "surname" };
        private static readonly string[] ClaimTypesForPostalCode = { "postalcode" };
        //private static readonly string[] ClaimsToExclude = { "iss", "sub", "aud", "iat", "identities" };

        /// <summary>
        /// Translates a set of generic Claims (like the ones returned from Auth0) to a set of Claims from the
        /// <see cref="ClaimTypes"/> constants wherever possible.
        /// </summary>
        /// <param name="claims"></param>
        public static List<Claim> GetStandardizedClaims(this IEnumerable<Claim> claims)
        {
            if (claims is null)
            {
                return new List<Claim>();
            }

            var newClaims = new List<Claim>();
            foreach (var claim in claims)
            {
                var newClaimType = GetClaimType(claim.Type);
                if (newClaimType == ClaimTypes.Role && claim.Value.Contains("["))
                {
                    var roles = JsonSerializer.Deserialize<List<string>>(claim.Value);
                    roles.ForEach(c => newClaims.Add(new Claim(newClaimType, c, claim.ValueType, claim.Issuer)));
                    continue;
                }

                if (newClaimType == ClaimTypes.Role || !newClaims.Any(c => c.Type == newClaimType))
                {
                    newClaims.Add(new Claim(newClaimType, claim.Value, claim.ValueType, claim.Issuer));
                }
            }
            return newClaims;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetClaimType(string name)
        {
            var newName = name.Replace("_", "").ToLower();
            return true switch
            {
                true when newName == "name" => ClaimTypes.Name,
                true when ClaimTypesForUserId.Any(c => newName.EndsWith(c)) => ClaimTypes.NameIdentifier,
                true when ClaimTypesForRoles.Any(c => newName.EndsWith(c)) => ClaimTypes.Role,
                true when ClaimTypesForEmail.Contains(newName) => ClaimTypes.Email,
                true when ClaimTypesForGivenName.Contains(newName) => ClaimTypes.GivenName,
                true when ClaimTypesForFamilyName.Contains(newName) => ClaimTypes.Surname,
                true when ClaimTypesForPostalCode.Contains(newName) => ClaimTypes.PostalCode,
                true when name == "gender" => ClaimTypes.Gender,
                true when name == "exp" => ClaimTypes.Expiration,
                true when name == "actor" => ClaimTypes.Actor,
                _ => name,
            };
        }

    }

}
