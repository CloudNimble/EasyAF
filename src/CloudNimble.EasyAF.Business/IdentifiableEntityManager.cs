using CloudNimble.EasyAF.Core;
using CloudNimble.SimpleMessageBus.Publish;
using System;
using System.Linq;
using System.Threading.Tasks;
#if EFCORE
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif

namespace CloudNimble.EasyAF.Business
{

    /// <summary>
    /// Provides a specialized entity manager for entities that implement IIdentifiable&lt;TId&gt;.
    /// Automatically generates GUID identifiers for entities with empty IDs during insertion.
    /// </summary>
    /// <typeparam name="TContext">The <see cref="DbContext"/> type to use for this Manager.</typeparam>
    /// <typeparam name="TEntity">The entity type for this Manager.</typeparam>
    /// <typeparam name="TId">The data type of the Id column for this Entity.</typeparam>
    public abstract class IdentifiableEntityManager<TContext, TEntity, TId> : EntityManager<TContext, TEntity>
        where TContext : DbContext
        where TEntity : class, IIdentifiable<TId>
        where TId : struct
    {

        #region Constructors

        /// <summary>
        /// Create a new instance of the given Manager for a given <see cref="DbContext"/>.
        /// </summary>
        /// <param name="dataContext">The <see cref="DbContext"/> instance to use for the database connection. Should be injected by the DI container.</param>
        /// <param name="messagePublisher">The SimpleMessageBus <see cref="IMessagePublisher"/> instance to use to publish Messages to a Queue. Should be injected by the DI container.</param>
        public IdentifiableEntityManager(TContext dataContext, IMessagePublisher messagePublisher) : base(dataContext, messagePublisher)
        {
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Perform business logic (like setting the entity's Id) prior to saving the <typeparamref name="TEntity"/> to the <typeparamref name="TContext"/>.
        /// </summary>
        /// <param name="entity">The <typeparamref name="TEntity"/> to be inserted.</param>
        public override async Task OnInsertingAsync(TEntity entity)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));

            var entityType = entity.GetType();
            // RWM: We have to do this cast because we're only doing this update for GUIDs. Numeric values should be set at the database level.
            if (InterfaceDictionary[entityType].Any(c => c == typeof(IIdentifiable<Guid>)) && (entity as IIdentifiable<Guid>).Id == Guid.Empty)
            {
                (entity as IIdentifiable<Guid>).Id = Guid.NewGuid();
            }
            await base.OnInsertingAsync(entity).ConfigureAwait(false);
        }

        #endregion

    }

}
