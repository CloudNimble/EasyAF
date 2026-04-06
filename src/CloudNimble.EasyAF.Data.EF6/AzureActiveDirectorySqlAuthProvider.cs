using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Data
{

    /// <summary>
    /// Provides a custom authentication method that gets a <see cref="SqlAuthenticationToken"/> from Azure Identity for the executing context.
    /// </summary>
    public class AzureActiveDirectorySqlAuthProvider : SqlAuthenticationProvider
    {

        private static readonly string[] _azureSqlScopes = new[]
        {
            "https://database.windows.net//.default"
        };

        private static readonly TokenCredential _credential = new DefaultAzureCredential();

        /// <summary>
        /// Request token from the provider using the specified <see cref="SqlAuthenticationParameters"/>.
        /// Uses DefaultAzureCredential to obtain an access token for SQL Database authentication.
        /// </summary>
        /// <param name="parameters">The authentication parameters from SQL Client.</param>
        /// <returns>A SqlAuthenticationToken containing the access token and expiration time.</returns>
        public override async Task<SqlAuthenticationToken> AcquireTokenAsync(SqlAuthenticationParameters parameters)
        {
            var tokenRequestContext = new TokenRequestContext(_azureSqlScopes);
            var tokenResult = await _credential.GetTokenAsync(tokenRequestContext, default);
            return new SqlAuthenticationToken(tokenResult.Token, tokenResult.ExpiresOn);
        }

        /// <summary>
        /// Returns a flag indicating if the requested <see cref="SqlAuthenticationMethod"/> is supported by this custom <see cref="SqlAuthenticationProvider"/>.
        /// This provider supports ActiveDirectoryDeviceCodeFlow authentication method.
        /// </summary>
        /// <param name="authenticationMethod">The authentication method to check for support.</param>
        /// <returns>True if the authentication method is ActiveDirectoryDeviceCodeFlow; otherwise, false.</returns>
        public override bool IsSupported(SqlAuthenticationMethod authenticationMethod) => authenticationMethod.Equals(SqlAuthenticationMethod.ActiveDirectoryDeviceCodeFlow);

    }

}
