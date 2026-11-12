using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Benchmarks
{

    /// <summary>
    /// Measures what an app does on every request that creates a record: create a product, set its properties, and save it through the
    /// ProductManager to an EF Core in-memory database. Compares EasyAF 4.x (the V4 namespace) with EasyAF 5.0 (the V5 namespace).
    /// </summary>
    /// <remarks>
    /// Each operation uses a new context and manager, the way a scoped request does, so the change tracker doesn't grow between operations.
    /// </remarks>
    [MemoryDiagnoser]
    public class SaveBenchmarks
    {

        #region Fields

        private readonly string _databaseName = Guid.NewGuid().ToString();
        private DbContextOptions<V4.EasyAFEntities> _v4Options;
        private DbContextOptions<V5.EasyAFEntities> _v5Options;
        private Guid _statusTypeId;

        #endregion

        #region Setup

        /// <summary>
        /// Creates the EasyAF 4.x in-memory database and seeds the status type every product needs.
        /// </summary>
        [GlobalSetup(Target = nameof(CreateAndSaveProduct_V4))]
        public void SetupV4()
        {
            SignIn();
            _v4Options = new DbContextOptionsBuilder<V4.EasyAFEntities>().UseInMemoryDatabase(_databaseName).Options;
            using var context = new V4.EasyAFEntities(_v4Options);
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
            SignIn();
            _v5Options = new DbContextOptionsBuilder<V5.EasyAFEntities>().UseInMemoryDatabase(_databaseName).Options;
            using var context = new V5.EasyAFEntities(_v5Options);
            var statusType = new EasyAFModel.ProductStatusType { Id = Guid.NewGuid(), DisplayName = "Active", IsActive = true, SortOrder = 0, DateCreated = DateTimeOffset.UtcNow };
            context.ProductStatusTypes.Add(statusType);
            context.SaveChanges();
            _statusTypeId = statusType.Id;
        }

        /// <summary>
        /// Signs in a user with a Guid ID claim, so the managers can set CreatedById.
        /// </summary>
        private static void SignIn()
        {
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
            using var context = new V4.EasyAFEntities(_v4Options);
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
            using var context = new V5.EasyAFEntities(_v5Options);
            var manager = new V5.ProductManager(context, null);
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
