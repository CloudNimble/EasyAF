using BenchmarkDotNet.Attributes;
using CloudNimble.EasyAF.Data;
using Effort;
using EasyAFModel.Managers;
using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Benchmarks
{

    /// <summary>
    /// Measures what an app does on every request that creates a record: create a product, set its properties, and save it through the
    /// generated ProductManager to an in-memory EF6 database. Compares EasyAF 4.x (the V4 namespace) with EasyAF 5.0.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each operation uses a new context and manager, the way a scoped request does, so the change tracker doesn't grow between operations.
    /// </para>
    /// <para>
    /// EF6 matches entity classes to the EDMX by class name, so the V4 and V5 models can't be loaded into the same process. Each benchmark
    /// has its own setup, and BenchmarkDotNet runs every benchmark in its own process.
    /// </para>
    /// </remarks>
    [MemoryDiagnoser]
    public class SaveBenchmarks
    {

        #region Fields

        private const string ConnectionString = "metadata=res://*/EntityModel.csdl|res://*/EntityModel.ssdl|res://*/EntityModel.msl;provider=Microsoft.Data.SqlClient;provider connection string=\"\"";

        private readonly string _instanceId = Guid.NewGuid().ToString();
        private Guid _statusTypeId;

        #endregion

        #region Setup

        /// <summary>
        /// Creates the EasyAF 4.x in-memory database and seeds the status type every product needs.
        /// </summary>
        [GlobalSetup(Target = nameof(CreateAndSaveProduct_V4))]
        public void SetupV4()
        {
            // RWM: res://*/ only searches loaded assemblies, and nothing else in this process loads Tests.Shared, which holds the EDMX.
            _ = new EasyAFModel.Managers.ProductManager(null, null);
            Configure();
            using var context = new V4.EasyAFEntities(EntityConnectionFactory.CreatePersistent(_instanceId, ConnectionString), true);
            var statusType = new V4.ProductStatusType { Id = Guid.NewGuid(), DisplayName = "Active", IsActive = true, SortOrder = 0, DateCreated = DateTimeOffset.UtcNow };
            context.ProductStatusTypes.Add(statusType);
            context.SaveChanges();
            _statusTypeId = statusType.Id;
        }

        /// <summary>
        /// Creates the EasyAF 5.0 in-memory database and seeds the status type every product needs.
        /// </summary>
        [GlobalSetup(Target = nameof(CreateAndSaveProduct_V5))]
        public void SetupV5()
        {
            Configure();
            using var context = new EasyAFModel.EasyAFEntities(EntityConnectionFactory.CreatePersistent(_instanceId, ConnectionString), true);
            var statusType = new EasyAFModel.ProductStatusType { Id = Guid.NewGuid(), DisplayName = "Active", IsActive = true, SortOrder = 0, DateCreated = DateTimeOffset.UtcNow };
            context.ProductStatusTypes.Add(statusType);
            context.SaveChanges();
            _statusTypeId = statusType.Id;
        }

        /// <summary>
        /// Registers the EF6 SQL provider the way an EasyAF app does, and signs in a user with a Guid ID claim so the managers can set CreatedById.
        /// </summary>
        /// <remarks>
        /// Effort reads the EDMX storage model through its original provider, so that provider must be registered even though no SQL runs.
        /// </remarks>
        private static void Configure()
        {
            // RWM: Effort adds itself when EF6 loads its configuration, so it has to register before the configuration is set.
            Effort.Provider.EffortProviderConfiguration.RegisterProvider();
            DbConfiguration.SetConfiguration(new EasyAFSqlAzureConfiguration());

            EasyAF_ClaimsPrincipalExtensions.Initialize();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "robert@contoso.com"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(EasyAF_ClaimsPrincipalExtensions.NameClaimType, "731c7991-8714-4a6a-a98f-311f6e79f742"),
            }, "Bearer"));
            ClaimsPrincipal.ClaimsPrincipalSelector = () => user;
        }

        #endregion

        #region Benchmarks

        /// <summary>
        /// Creates a product, sets its properties, and saves it with EasyAF 4.x.
        /// </summary>
        [Benchmark(Baseline = true)]
        public async Task<bool> CreateAndSaveProduct_V4()
        {
            using var context = new V4.EasyAFEntities(EntityConnectionFactory.CreatePersistent(_instanceId, ConnectionString), true);
            var manager = new V4.ProductManager(context, null);
            var product = new V4.Product
            {
                DisplayName = "Contoso Widget",
                StatusTypeId = _statusTypeId,
            };
            return await manager.InsertAsync(product);
        }

        /// <summary>
        /// Creates a product, sets its properties, and saves it with EasyAF 5.0.
        /// </summary>
        [Benchmark]
        public async Task<bool> CreateAndSaveProduct_V5()
        {
            using var context = new EasyAFModel.EasyAFEntities(EntityConnectionFactory.CreatePersistent(_instanceId, ConnectionString), true);
            var manager = new ProductManager(context, null);
            var product = new EasyAFModel.Product
            {
                DisplayName = "Contoso Widget",
                StatusTypeId = _statusTypeId,
            };
            return await manager.InsertAsync(product);
        }

        #endregion

    }

}
