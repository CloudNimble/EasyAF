using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Benchmarks
{

    /// <summary>
    /// Measures each Business manager lifecycle method that runs on every save, on EasyAF 4.x (the V4 namespace) and EasyAF 5.0
    /// (the V5 namespace). This file is shared by Benchmarks.EF6 and Benchmarks.EFCore so both Entity Framework flavors run identical scenarios.
    /// </summary>
    /// <remarks>
    /// The managers skip the status type query and have no database, so only the manager code is measured.
    /// </remarks>
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class ManagerBenchmarks
    {

        #region Fields

        private V4.BenchmarkProductManager _v4Manager;
        private V4.Product _v4Product;
        private V4.ProductStatusType _v4StatusType;
        private V5.BenchmarkProductManager _v5Manager;
        private EasyAFModel.Product _v5Product;
        private EasyAFModel.ProductStatusType _v5StatusType;

        #endregion

        #region Setup

        /// <summary>
        /// Signs in a user with a Guid ID claim, and creates the managers and a saved product for each version.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            EasyAF_ClaimsPrincipalExtensions.Initialize();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "robert@contoso.com"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(EasyAF_ClaimsPrincipalExtensions.NameClaimType, "731c7991-8714-4a6a-a98f-311f6e79f742"),
            }, "Bearer"));
            ClaimsPrincipal.ClaimsPrincipalSelector = () => user;

            _v4Manager = new V4.BenchmarkProductManager();
            _v4StatusType = new V4.ProductStatusType { Id = Guid.NewGuid(), DisplayName = "Active", IsActive = true };
            _v4Product = NewV4Product();
            _v4Product.Id = Guid.NewGuid();

            _v5Manager = new V5.BenchmarkProductManager();
            _v5StatusType = new EasyAFModel.ProductStatusType { Id = Guid.NewGuid(), DisplayName = "Active", IsActive = true };
            _v5Product = NewV5Product();
            _v5Product.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Creates an unsaved EasyAF 4.x product with its status already set, so the manager doesn't need to look one up.
        /// </summary>
        /// <returns>The product.</returns>
        private V4.Product NewV4Product()
        {
            return new V4.Product { DisplayName = "Contoso Widget", StatusType = _v4StatusType, StatusTypeId = _v4StatusType.Id };
        }

        /// <summary>
        /// Creates an unsaved EasyAF 5.0 product with its status already set, so the manager doesn't need to look one up.
        /// </summary>
        /// <returns>The product.</returns>
        private EasyAFModel.Product NewV5Product()
        {
            return new EasyAFModel.Product { DisplayName = "Contoso Widget", StatusType = _v5StatusType, StatusTypeId = _v5StatusType.Id };
        }

        #endregion

        #region OnInsertingAsync

        /// <summary>
        /// A new product: generates the Guid key, sets CreatedById from the claim, and sets DateCreated.
        /// </summary>
        [Benchmark(Baseline = true), BenchmarkCategory("OnInsertingAsync")]
        public Task OnInsertingAsync_V4() => _v4Manager.OnInsertingAsync(NewV4Product());

        /// <inheritdoc cref="OnInsertingAsync_V4" />
        [Benchmark, BenchmarkCategory("OnInsertingAsync")]
        public Task OnInsertingAsync_V5() => _v5Manager.OnInsertingAsync(NewV5Product());

        #endregion

        #region OnUpdatingAsync

        /// <summary>
        /// An existing product: sets UpdatedById from the claim and DateUpdated.
        /// </summary>
        [Benchmark(Baseline = true), BenchmarkCategory("OnUpdatingAsync")]
        public Task OnUpdatingAsync_V4() => _v4Manager.OnUpdatingAsync(_v4Product);

        /// <inheritdoc cref="OnUpdatingAsync_V4" />
        [Benchmark, BenchmarkCategory("OnUpdatingAsync")]
        public Task OnUpdatingAsync_V5() => _v5Manager.OnUpdatingAsync(_v5Product);

        #endregion

        #region ResetAuditProperties

        /// <summary>
        /// Resets the audit fields to an "inserted" state.
        /// </summary>
        [Benchmark(Baseline = true), BenchmarkCategory("ResetAuditProperties")]
        public void ResetAuditProperties_V4() => _v4Manager.ResetAuditProperties(_v4Product);

        /// <inheritdoc cref="ResetAuditProperties_V4" />
        [Benchmark, BenchmarkCategory("ResetAuditProperties")]
        public void ResetAuditProperties_V5() => _v5Manager.ResetAuditProperties(_v5Product);

        #endregion

    }

}
