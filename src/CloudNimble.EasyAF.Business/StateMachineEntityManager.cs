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
    /// A Manager inheriting from <see cref="IdentifiableEntityManager{TContext, TEntity, TId}"/> that contains reusable logic for updating a <typeparamref name="TEntity"/>'s current State.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <typeparam name="TStateType"></typeparam>
    public abstract class StateMachineEntityManager<TContext, TEntity, TId, TStateType> : IdentifiableEntityManager<TContext, TEntity, TId>
        where TContext : DbContext
        where TEntity : class, IIdentifiable<TId>, IHasState<TStateType>
        where TId : struct
        where TStateType : class, IDbStateEnum
    {

        #region Public Members

        /// <summary>
        /// Gets the collection of active state types available for entities managed by this manager.
        /// This collection is populated during initialization from the database.
        /// </summary>
        public List<TStateType> StateTypes { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the StateMachineEntityManager class.
        /// </summary>
        /// <param name="dataContext">The <see cref="DbContext"/> instance to use for the database connection. Should be injected by the DI container.</param>
        /// <param name="messagePublisher">The SimpleMessageBus <see cref="IMessagePublisher"/> instance to use to publish Messages to a Queue. Should be injected by the DI container.</param>
        protected StateMachineEntityManager(TContext dataContext, IMessagePublisher messagePublisher) : base(dataContext, messagePublisher)
        {
            StateTypes = new List<TStateType>();
        }

        #region Initialization 

        /// <summary>
        /// Initializes the StateTypes collection by loading active state types from the database.
        /// This method is called automatically by state update methods if the collection is empty.
        /// </summary>
        public virtual void Initialize()
        {
            if (StateTypes is null || StateTypes.Count == 0)
            {
                StateTypes = DataContext.Set<TStateType>()
                    .AsNoTracking()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ToList();
            }
        }

        #endregion

        #region State Updates

        /// <summary>
        /// Sets the entity's state to "Created" (sort order 0).
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>True if the state was successfully updated; otherwise, false.</returns>
        public async Task<bool> SetCreatedAsync(TEntity entity)
        {
            return await UpdateStateAsync(entity, 0).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the entity's state to "Cancelled" (sort order 98).
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>True if the state was successfully updated; otherwise, false.</returns>
        public virtual async Task<bool> SetCancelledAsync(TEntity entity)
        {
            return await UpdateStateAsync(entity, 98).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the entity's state to "Completed" (sort order 100).
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>True if the state was successfully updated; otherwise, false.</returns>
        public virtual async Task<bool> SetCompletedAsync(TEntity entity)
        {
            return await UpdateStateAsync(entity, 100).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the entity's state to "Failed" (sort order 99).
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="errorMessage">Optional error message (currently not used in implementation).</param>
        /// <param name="errorDetail">Optional error detail (currently not used in implementation).</param>
        /// <returns>True if the state was successfully updated; otherwise, false.</returns>
        public virtual async Task<bool> SetFailedAsync(TEntity entity, string errorMessage = "", string errorDetail = "")
        {
            return await UpdateStateAsync(entity, 99).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the entity's state to the state type with the specified sort order.
        /// Logs the state transition for tracking purposes.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="sortOrder">The sort order of the target state type.</param>
        /// <returns>True if the state was successfully updated; otherwise, false.</returns>
        /// <exception cref="Exception">Thrown when no state type is found with the specified sort order.</exception>
        public async Task<bool> UpdateStateAsync(TEntity entity, int sortOrder)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));

            Initialize();
            var StateType = StateTypes.FirstOrDefault(c => c.SortOrder == sortOrder);
            if (StateType is null)
            {
                throw new Exception($"Could not find the StateType for SortOrder = '{sortOrder}'");
            }
            Trace.TraceInformation($"{entity.GetType().Name} {(entity as IIdentifiable<TId>).Id} StateType is being updated to {StateType.DisplayName}.");
            entity.StateType = null;
            entity.StateTypeId = StateType.Id;
            var result =  await UpdateAsync(entity, DataContext).ConfigureAwait(false);
            Trace.TraceInformation($"{entity.GetType().Name} {(entity as IIdentifiable<TId>).Id} StateType{(result ? "" : " NOT")} updated.");
            return result;
        }

        #endregion

    }

}
