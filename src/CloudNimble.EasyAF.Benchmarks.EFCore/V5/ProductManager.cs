using CloudNimble.EasyAF.Business;
using CloudNimble.SimpleMessageBus.Publish;
using EasyAFModel;
using System;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Benchmarks.V5
{

    /// <summary>
    /// The EasyAF 5.0 ProductManager, written in the shape the generator produces for EF6, over the EF Core context.
    /// </summary>
    public class ProductManager : StatusEntityManager<EasyAFEntities, Product, Guid, ProductStatusType>
    {

        #region Constructors

        public ProductManager(EasyAFEntities dataContext, IMessagePublisher messagePublisher) : base(dataContext, messagePublisher)
        {
        }

        #endregion

        #region Object Validation

        public override async Task OnInsertingAsync(Product entity)
        {
            Initialize();
            await base.OnInsertingAsync(entity);
        }

        public override async Task OnUpdatingAsync(Product entity)
        {
            Initialize();
            await base.OnUpdatingAsync(entity);
        }

        #endregion

    }

}
