using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace CloudNimble.EasyAF.Data
{

    /// <summary>
    /// Provides Entity Framework 6 configuration optimized for SQL Azure connections.
    /// Configures Microsoft.Data.SqlClient provider and Azure-specific execution strategy for improved reliability.
    /// </summary>
    public class EasyAFSqlAzureConfiguration : DbConfiguration
    {

        /// <summary>
        /// Initializes a new instance of the EasyAFSqlAzureConfiguration class.
        /// Configures the SQL provider factory, services, and execution strategy for SQL Azure.
        /// </summary>
        public EasyAFSqlAzureConfiguration()
        {
            SetProviderFactory(MicrosoftSqlProviderServices.ProviderInvariantName, Microsoft.Data.SqlClient.SqlClientFactory.Instance);
            SetProviderServices(MicrosoftSqlProviderServices.ProviderInvariantName, MicrosoftSqlProviderServices.Instance);
            SetExecutionStrategy(MicrosoftSqlProviderServices.ProviderInvariantName, () => new MicrosoftSqlAzureExecutionStrategy());
        }

    }

}
