using CloudNimble.EasyAF.Business;
using CloudNimble.EasyAF.Core;
using CloudNimble.SimpleMessageBus.Publish;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Benchmarks.V4
{

    /// <summary>
    /// The EasyAF 4.x generated ProductManager and the Tests.Shared partial, frozen for benchmark comparisons.
    /// </summary>
    /// <remarks>
    /// The lifecycle methods call <see cref="EntityManagerHooks"/>, which holds the EasyAF 4.x base class code, instead of the
    /// EasyAF 5.0 base classes. InsertAsync, UpdateAsync, and everything else did not change between 4.x and 5.0.
    /// </remarks>
    public partial class ProductManager : StatusEntityManager<EasyAFEntities, Product, Guid, ProductStatusType>
    {

        #region Constructors

        public ProductManager(EasyAFEntities dataContext, IMessagePublisher messagePublisher) : base(dataContext, messagePublisher)
        {
            EntityManagerHooks.RegisterEntityType(typeof(Product));
        }

        #endregion

        #region Public Methods

        public IQueryable<Product> OnFilter(IQueryable<Product> entitySet)
        {
            OnFilterInternal(ref entitySet);
            return entitySet;
        }

        /// <summary>
        /// The EasyAF 4.x EntityManager.ResetAuditProperties.
        /// </summary>
        public new void ResetAuditProperties<TDbObservable>(TDbObservable entity) where TDbObservable : DbObservableObject
        {
            EntityManagerHooks.ResetAuditProperties(entity);
        }

        #region Object Validation

        public override async Task OnInsertingAsync(Product entity)
        {
            Initialize();
            await EntityManagerHooks.OnInsertingIdentifiableAsync(entity);
            OnInsertingInternal(entity);
        }

        public override async Task OnUpdatingAsync(Product entity)
        {
            Initialize();
            await EntityManagerHooks.OnUpdatingAsync(entity);
            OnUpdatingInternal(entity);
        }

        public override async Task OnDeletingAsync(Product entity)
        {
            Initialize();
            await base.OnDeletingAsync(entity);
            OnDeletingInternal(entity);
        }

        #endregion

        #endregion

        #region Partial Methods

        partial void OnFilterInternal(ref IQueryable<Product> entitySet, string clientAppId = null);

        partial void OnInsertingInternal(Product entity);

        partial void OnUpdatingInternal(Product entity);

        partial void OnDeletingInternal(Product entity);

        #endregion

        #region Tests.Shared Partial

        partial void OnInsertingInternal(Product entity)
        {
            if (entity.StatusType is null || entity.StatusTypeId == Guid.Empty)
            {
                entity.StatusTypeId = DataContext.ProductStatusTypes.Where(c => c.SortOrder == 0).FirstOrDefault()?.Id ?? Guid.Empty;
            }
        }

        #endregion

    }

}
