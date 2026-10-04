using CloudNimble.EasyAF.Business;
using CloudNimble.EasyAF.Core;
using CloudNimble.SimpleMessageBus.Publish;
using System;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Benchmarks.V4
{

    /// <summary>
    /// The EasyAF 4.x ProductManager, written in the shape the generator produces for EF6, over the EF Core context.
    /// </summary>
    /// <remarks>
    /// The lifecycle methods call <see cref="EntityManagerHooks"/>, which holds the EasyAF 4.x base class code, instead of the
    /// EasyAF 5.0 base classes. InsertAsync, UpdateAsync, and everything else did not change between 4.x and 5.0.
    /// </remarks>
    public class ProductManager : StatusEntityManager<EasyAFEntities, Product, Guid, ProductStatusType>
    {

        #region Constructors

        public ProductManager(EasyAFEntities dataContext, IMessagePublisher messagePublisher) : base(dataContext, messagePublisher)
        {
            EntityManagerHooks.RegisterEntityType(typeof(Product));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The EasyAF 4.x EntityManager.ResetAuditProperties.
        /// </summary>
        public new void ResetAuditProperties<TDbObservable>(TDbObservable entity) where TDbObservable : DbObservableObject
        {
            EntityManagerHooks.ResetAuditProperties(entity);
        }

        #endregion

        #region Object Validation

        public override async Task OnInsertingAsync(Product entity)
        {
            Initialize();
            await EntityManagerHooks.OnInsertingIdentifiableAsync(entity);
        }

        public override async Task OnUpdatingAsync(Product entity)
        {
            Initialize();
            await EntityManagerHooks.OnUpdatingAsync(entity);
        }

        #endregion

    }

}
