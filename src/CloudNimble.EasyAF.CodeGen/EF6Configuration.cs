using CloudNimble.EasyAF.Edmx.InMemoryDb.Provider;
using System.Data.Entity;

namespace CloudNimble.EasyAF.CodeGen
{

    /// <summary>
    /// 
    /// </summary>
    public class EF6Configuration : DbConfiguration
    {

        /// <summary>
        /// 
        /// </summary>
        public EF6Configuration()
        {
            //SetProviderFactory(ProviderConstants.MicrosoftDataClient, SqlClientFactory.Instance);
            SetProviderFactory(ProviderConstants.SystemDataClient, EffortProviderFactory.Instance);
            SetProviderServices(ProviderConstants.SystemDataClient, EffortProviderServices.Instance);
        }

    }

}
