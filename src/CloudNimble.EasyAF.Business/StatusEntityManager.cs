using CloudNimble.EasyAF.Core;
using CloudNimble.SimpleMessageBus.Publish;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// A Manager inheriting from <see cref="IdentifiableEntityManager{TContext, TEntity, TId}"/> that contains reusable logic for updating a <typeparamref name="TEntity"/>'s current Status.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TStatusType"></typeparam>
    public abstract class StatusEntityManager<TContext, TEntity, TId, TStatusType> : IdentifiableEntityManager<TContext, TEntity, TId>
        where TContext : DbContext
        where TEntity : class, IIdentifiable<TId>, IHasStatus<TStatusType>
        where TId : struct
        where TStatusType : class, IDbStatusEnum
    {

        #region Public Members

        /// <summary>
        /// Gets the collection of active status types available for entities managed by this manager.
        /// This collection is populated during initialization from the database.
        /// </summary>
        public List<TStatusType> StatusTypes { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the StatusEntityManager class.
        /// </summary>
        /// <param name="dataContext">The <see cref="DbContext"/> instance to use for the database connection. Should be injected by the DI container.</param>
        /// <param name="messagePublisher">The SimpleMessageBus <see cref="IMessagePublisher"/> instance to use to publish Messages to a Queue. Should be injected by the DI container.</param>
        protected StatusEntityManager(TContext dataContext, IMessagePublisher messagePublisher) : base(dataContext, messagePublisher)
        {
            StatusTypes = new List<TStatusType>();
        }

        #region Initialization 

        /// <summary>
        /// Initializes the StatusTypes collection by loading active status types from the database.
        /// This method is called automatically by status update methods if the collection is empty.
        /// </summary>
        public virtual void Initialize()
        {
            if (StatusTypes is null || StatusTypes.Count == 0)
            {
                StatusTypes = DataContext.Set<TStatusType>()
                    .AsNoTracking()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ToList();
            }
        }

        #endregion

        #region Status Updates

        /// <summary>
        /// Updates the entity's status to the status type with the specified sort order.
        /// Logs the status transition for tracking purposes.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="sortOrder">The sort order of the target status type.</param>
        /// <returns>True if the status was successfully updated; otherwise, false.</returns>
        /// <exception cref="Exception">Thrown when no status type is found with the specified sort order.</exception>
        public async Task<bool> UpdateStatusAsync(TEntity entity, int sortOrder)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));

            Initialize();
            var statusType = StatusTypes.FirstOrDefault(c => c.SortOrder == sortOrder)
                ?? throw new Exception($"Could not find the StatusType for SortOrder = '{sortOrder}'");

            Trace.TraceInformation($"{entity.GetType().Name} {(entity as IIdentifiable<TId>).Id} StatusType is being updated to {statusType.DisplayName}.");
            entity.StatusType = null;
            entity.StatusTypeId = statusType.Id;
            var result =  await UpdateAsync(entity, DataContext).ConfigureAwait(false);
            Trace.TraceInformation($"{entity.GetType().Name} {(entity as IIdentifiable<TId>).Id} StatusType{(result ? "" : " NOT")} updated.");
            return result;
        }

        #endregion

    }

}
