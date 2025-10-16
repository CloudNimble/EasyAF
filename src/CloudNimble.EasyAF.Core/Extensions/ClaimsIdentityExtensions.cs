using System.Collections.Generic;
using System.Linq;

namespace System.Security.Claims
{

    /// <summary>
    /// 
    /// </summary>
    public static class EasyAF_ClaimsIdentityExtensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identity"></param>
        public static void StandardizeClaims(this ClaimsIdentity identity)
        {
            if (identity is null)
            {
                throw new ArgumentNullException(nameof(identity), "The ClaimsIdentity instance cannot be null.");
            }

            var standardizedClaims = identity.Claims.GetStandardizedClaims();
            foreach (var claim in identity.Claims.ToList())
            {
                if (!string.IsNullOrWhiteSpace(EasyAF_ClaimsPrincipalExtensions._schemaUri) && claim.Type.StartsWith(EasyAF_ClaimsPrincipalExtensions._schemaUri))
                {
                    continue;
                }
                identity.RemoveClaim(claim);
            }
            identity.AddClaims(standardizedClaims);
        }

    }

}
