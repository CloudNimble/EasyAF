using Ben.Collections;
using CloudNimble.EasyAF.Core;
using CloudNimble.SimpleMessageBus.Publish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
#if EFCORE
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;
#endif


namespace CloudNimble.EasyAF.Business
{

    /// <summary>
    /// Provides a base class for entity-specific business logic managers with built-in CRUD operations,
    /// audit trail support, and lifecycle event hooks. Handles common entity operations and automatically
    /// manages audit fields for entities that implement auditing interfaces.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext used for database operations.</typeparam>
    /// <typeparam name="TEntity">The type of entity managed by this manager.</typeparam>
    /// <remarks>
    /// This manager provides comprehensive entity lifecycle management including:
    /// - Automatic audit trail creation for entities implementing <see cref="ICreatedAuditable"/>, <see cref="IUpdatedAuditable"/>
    /// - User tracking for entities implementing <see cref="ICreatorTrackable{T}"/>, <see cref="IUpdaterTrackable{T}"/>
    /// - Virtual hooks for custom business logic before and after CRUD operations
    /// - Batch operations support for improved performance
    /// - Thread-safe interface caching for performance optimization
    /// </remarks>
    /// <example>
    /// <code>
    /// public class UserManager : EntityManager&lt;MyDbContext, User&gt;
    /// {
    ///     public UserManager(MyDbContext context, IMessagePublisher publisher) 
    ///         : base(context, publisher) { }
    /// 
    ///     public override async Task OnInsertingAsync(User entity)
    ///     {
    ///         await base.OnInsertingAsync(entity); // Handles audit fields
    ///         entity.IsActive = true; // Custom business logic
    ///     }
    /// 
    ///     public override async Task&lt;bool&gt; OnInsertedAsync(User entity)
    ///     {
    ///         await MessagePublisher.PublishAsync(new UserCreatedEvent { UserId = entity.Id });
    ///         return await base.OnInsertedAsync(entity);
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class EntityManager<TContext, TEntity> : ManagerBase<TContext>
        where TContext : DbContext
        where TEntity : class
    {

        #region Private Static Members

        internal static readonly TypeDictionary<Type[]> InterfaceDictionary;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes static members of the <see cref="EntityManager{TContext, TEntity}"/> class.
        /// Sets up the interface cache for performance optimization of runtime interface checking.
        /// </summary>
        static EntityManager()
        {
            InterfaceDictionary = new TypeDictionary<Type[]>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityManager{TContext, TEntity}"/> class.
        /// </summary>
        /// <param name="dataContext">The database context instance for data operations. Should be injected by the DI container.</param>
        /// <param name="messagePublisher">The message publisher instance for publishing events. Should be injected by the DI container.</param>
        public EntityManager(TContext dataContext, IMessagePublisher messagePublisher) : base(dataContext, messagePublisher)
        {
            if (!InterfaceDictionary.ContainsKey(typeof(TEntity)))
            {
                InterfaceDictionary[typeof(TEntity)] = typeof(TEntity).GetInterfaces();
            }
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Called before inserting an entity into the database. Automatically handles audit field population
        /// and user tracking for entities implementing the appropriate interfaces.
        /// </summary>
        /// <param name="entity">The entity to be inserted.</param>
        /// <remarks>
        /// This method automatically sets:
        /// - CreatedById for entities implementing <see cref="ICreatorTrackable{T}"/>
        /// - DateCreated for entities implementing <see cref="ICreatedAuditable"/>
        /// Override this method to add custom business logic before insertion.
        /// </remarks>
        public virtual async Task OnInsertingAsync(TEntity entity)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));

            var entityType = entity.GetType();
            if (InterfaceDictionary[entityType].Any(c => c.Name == typeof(ICreatorTrackable<>).Name) && ClaimsPrincipal.Current is not null)
            {
                // TODO: RWM: This probably need to figure out how to check the type and make sure we don't just assume GUIDs.
                (entity as ICreatorTrackable<Guid>).CreatedById = ClaimsPrincipal.Current.GetIdClaim();
            }
            if (InterfaceDictionary[entityType].Any(c => c == typeof(ICreatedAuditable)))
            {
                (entity as ICreatedAuditable).DateCreated = DateTime.UtcNow;
            }
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Called after successfully inserting an entity into the database. Use this method for post-insertion
        /// business logic such as sending notifications, publishing events, or triggering external systems.
        /// </summary>
        /// <param name="entity">The entity that was inserted.</param>
        /// <returns>True if post-insertion processing was successful; otherwise, false.</returns>
        public virtual async Task<bool> OnInsertedAsync(TEntity entity)
        {
            return await Task.FromResult(true).ConfigureAwait(false);
        }

        /// <summary>
        /// Called before updating an entity in the database. Automatically handles audit field population
        /// and user tracking for entities implementing the appropriate interfaces.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <remarks>
        /// This method automatically sets:
        /// - UpdatedById for entities implementing <see cref="IUpdaterTrackable{T}"/>
        /// - DateUpdated for entities implementing <see cref="IUpdatedAuditable"/>
        /// Override this method to add custom business logic before updating.
        /// </remarks>
        public virtual async Task OnUpdatingAsync(TEntity entity)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));

            var entityType = entity.GetType();
            if (InterfaceDictionary[entityType].Any(c => c.Name == typeof(IUpdaterTrackable<>).Name) && ClaimsPrincipal.Current is not null)
            {
                // TODO: RWM: This probably need to figure out how to check the type and make sure we don't just assume GUIDs.
                (entity as IUpdaterTrackable<Guid>).UpdatedById = ClaimsPrincipal.Current.GetIdClaim();
            }
            if (InterfaceDictionary[entityType].Any(c => c == typeof(IUpdatedAuditable)))
            {
                (entity as IUpdatedAuditable).DateUpdated = DateTime.UtcNow;
            }
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Called after successfully updating an entity in the database. Use this method for post-update
        /// business logic such as sending notifications, publishing events, or triggering external systems.
        /// </summary>
        /// <param name="entity">The entity that was updated.</param>
        /// <returns>True if post-update processing was successful; otherwise, false.</returns>
        public virtual async Task<bool> OnUpdatedAsync(TEntity entity)
        {
            return await Task.FromResult(true).ConfigureAwait(false);
        }

        /// <summary>
        /// Called before deleting an entity from the database. Override this method to add
        /// custom business logic or validation before deletion.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        public virtual async Task OnDeletingAsync(TEntity entity)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Called after successfully deleting an entity from the database. Use this method for post-deletion
        /// business logic such as cleanup operations, sending notifications, or triggering external systems.
        /// </summary>
        /// <param name="entity">The entity that was deleted.</param>
        /// <returns>True if post-deletion processing was successful; otherwise, false.</returns>
        public virtual async Task<bool> OnDeletedAsync(TEntity entity)
        {
            return await Task.FromResult(true).ConfigureAwait(false);
        }

        #endregion

        #region Public Methods

        #region List Methods

        /// <summary>
        /// Called before inserting a collection of entities into the database.
        /// Applies OnInsertingAsync logic to each entity in the collection.
        /// </summary>
        /// <param name="entities">The collection of entities to be inserted.</param>
        public async Task OnInsertingAsync(List<TEntity> entities)
        {
            Ensure.ArgumentNotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                await OnInsertingAsync(entity).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Called after successfully inserting a collection of entities into the database.
        /// Applies OnInsertedAsync logic to each entity in the collection.
        /// </summary>
        /// <param name="entities">The collection of entities that were inserted.</param>
        public virtual async Task OnInsertedAsync(List<TEntity> entities)
        {
            Ensure.ArgumentNotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                await OnInsertedAsync(entity).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Called before updating a collection of entities in the database.
        /// Applies OnUpdatingAsync logic to each entity in the collection.
        /// </summary>
        /// <param name="entities">The collection of entities to be updated.</param>
        public virtual async Task OnUpdatingAsync(List<TEntity> entities)
        {
            Ensure.ArgumentNotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                await OnUpdatingAsync(entity).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Called after successfully updating a collection of entities in the database.
        /// Applies OnUpdatedAsync logic to each entity in the collection.
        /// </summary>
        /// <param name="entities">The collection of entities that were updated.</param>
        public virtual async Task OnUpdatedAsync(List<TEntity> entities)
        {
            Ensure.ArgumentNotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                await OnUpdatedAsync(entity).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Called before deleting a collection of entities from the database.
        /// Applies OnDeletingAsync logic to each entity in the collection.
        /// </summary>
        /// <param name="entities">The collection of entities to be deleted.</param>
        public virtual async Task OnDeletingAsync(List<TEntity> entities)
        {
            Ensure.ArgumentNotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                await OnDeletingAsync(entity).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Called after successfully deleting a collection of entities from the database.
        /// Applies OnDeletedAsync logic to each entity in the collection.
        /// </summary>
        /// <param name="entities">The collection of entities that were deleted.</param>
        public virtual async Task OnDeletedAsync(List<TEntity> entities)
        {
            Ensure.ArgumentNotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                await OnDeletedAsync(entity).ConfigureAwait(false);
            }
        }

        #endregion

        #region Insert Methods

        /// <summary>
        /// Inserts a single entity into the database with optional save operation.
        /// Executes the OnInsertingAsync and OnInsertedAsync lifecycle hooks.
        /// </summary>
        /// <param name="entity">The entity to be inserted.</param>
        /// <param name="save">Whether to immediately save changes to the database. Defaults to true.</param>
        /// <returns>True if the entity was successfully inserted; otherwise, false.</returns>
        /// <remarks>RWM: This will need to be updated to be generic if it's going to be in a NuGet package.</remarks>
        public async Task<bool> InsertAsync(TEntity entity, bool save = true)
        {
            return await InsertAsync(entity, DataContext, save).ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts a single entity into the database using a specified context with optional save operation.
        /// Executes the OnInsertingAsync and OnInsertedAsync lifecycle hooks.
        /// </summary>
        /// <param name="entity">The entity to be inserted.</param>
        /// <param name="context">The database context to use for the operation.</param>
        /// <param name="save">Whether to immediately save changes to the database. Defaults to true.</param>
        /// <returns>True if the entity was successfully inserted; otherwise, false.</returns>
        public async Task<bool> InsertAsync(TEntity entity, TContext context, bool save = true)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));
            Ensure.ArgumentNotNull(context, nameof(context));

            await OnInsertingAsync(entity).ConfigureAwait(false);
            context.Entry(entity).State = EntityState.Added;
            if (!save)
            {
                return true;
            }
            var changeCount = await context.SaveChangesAsync().ConfigureAwait(false);
            await OnInsertedAsync(entity).ConfigureAwait(false);
            return changeCount > 0;
        }

        /// <summary>
        /// Inserts a collection of entities into the database with optional save operation.
        /// Executes the OnInsertingAsync and OnInsertedAsync lifecycle hooks for each entity.
        /// </summary>
        /// <param name="entities">The collection of entities to be inserted.</param>
        /// <param name="save">Whether to immediately save changes to the database. Defaults to true.</param>
        /// <returns>True if the entities were successfully inserted; otherwise, false.</returns>
        /// <remarks>RWM: This will need to be updated to be generic if it's going to be in a NuGet package.</remarks>
        public async Task<bool> InsertAsync(List<TEntity> entities, bool save = true)
        {
            return await InsertAsync(entities, DataContext, save).ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts a collection of entities into the database using a specified context with optional save operation.
        /// Executes the OnInsertingAsync and OnInsertedAsync lifecycle hooks for each entity.
        /// </summary>
        /// <param name="entities">The collection of entities to be inserted.</param>
        /// <param name="context">The database context to use for the operation.</param>
        /// <param name="save">Whether to immediately save changes to the database. Defaults to true.</param>
        /// <returns>True if the entities were successfully inserted; otherwise, false.</returns>
        public async Task<bool> InsertAsync(List<TEntity> entities, TContext context, bool save = true)
        {
            Ensure.ArgumentNotNull(entities, nameof(entities));
            Ensure.ArgumentNotNull(context, nameof(context));

            var changeCount = 0;
            await OnInsertingAsync(entities).ConfigureAwait(false);
            entities.ForEach(c => context.Entry(c).State = EntityState.Added);
            if (!save)
            {
                return true;
            }
            changeCount = await context.SaveChangesAsync().ConfigureAwait(false);
            await OnInsertedAsync(entities).ConfigureAwait(false);
            return changeCount > 0;
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Updates a single entity in the database with optional save operation.
        /// Executes the OnUpdatingAsync and OnUpdatedAsync lifecycle hooks.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="save">Whether to immediately save changes to the database. Defaults to true.</param>
        /// <returns>True if the entity was successfully updated; otherwise, false.</returns>
        /// <remarks>RWM: This will need to be updated to be generic if it's going to be in a NuGet package.</remarks>
        public async Task<bool> UpdateAsync(TEntity entity, bool save = true)
        {
            return await UpdateAsync(entity, DataContext, save).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates a single entity in the database using a specified context with optional save operation.
        /// Executes the OnUpdatingAsync and OnUpdatedAsync lifecycle hooks.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="context">The database context to use for the operation.</param>
        /// <param name="save">Whether to immediately save changes to the database. Defaults to true.</param>
        /// <returns>True if the entity was successfully updated; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(TEntity entity, TContext context, bool save = true)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));
            Ensure.ArgumentNotNull(context, nameof(context));

            await OnUpdatingAsync(entity).ConfigureAwait(false);
            context.Entry(entity).State = EntityState.Modified;
            if (!save)
            {
                return true;
            }
            var changeCount = await context.SaveChangesAsync().ConfigureAwait(false);
            await OnUpdatedAsync(entity).ConfigureAwait(false);
            return changeCount > 0;
        }

        /// <summary>
        /// Updates a collection of entities in the database with optional save operation.
        /// Executes the OnUpdatingAsync and OnUpdatedAsync lifecycle hooks for each entity.
        /// </summary>
        /// <param name="entities">The collection of entities to be updated.</param>
        /// <param name="save">Whether to immediately save changes to the database. Defaults to true.</param>
        /// <returns>True if the entities were successfully updated; otherwise, false.</returns>
        /// <remarks>RWM: This will need to be updated to be generic if it's going to be in a NuGet package.</remarks>
        public async Task<bool> UpdateAsync(List<TEntity> entities, bool save = true)
        {
            return await UpdateAsync(entities, DataContext, save).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates a collection of entities in the database using a specified context with optional save operation.
        /// Executes the OnUpdatingAsync and OnUpdatedAsync lifecycle hooks for each entity.
        /// </summary>
        /// <param name="entities">The collection of entities to be updated.</param>
        /// <param name="context">The database context to use for the operation.</param>
        /// <param name="save">Whether to immediately save changes to the database. Defaults to true.</param>
        /// <returns>True if the entities were successfully updated; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(List<TEntity> entities, TContext context, bool save = true)
        {
            Ensure.ArgumentNotNull(entities, nameof(entities));
            Ensure.ArgumentNotNull(context, nameof(context));

            var changeCount = 0;
            await OnUpdatingAsync(entities).ConfigureAwait(false);
            entities.ForEach(c => context.Entry(c).State = EntityState.Modified);
            if (!save)
            {
                return true;
            }
            changeCount = await context.SaveChangesAsync().ConfigureAwait(false);
            await OnUpdatedAsync(entities).ConfigureAwait(false);
            return changeCount > 0;
        }

        /// <summary>
        /// Executes a direct UPDATE query on the database without returning objects or processing them through the interceptors.
        /// </summary>
        /// <param name="predicate">An <see cref="Expression{TDelegate}"/> to execute against the <see cref="DbSet{TEntity}"/></param> to return records that will be updated.
        /// <param name="updateExpression">An <see cref="Expression{TDelegate}"/> defining the updates to be performed on the records returned by the predicate.</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload will give you all of the performance of updating a set of data without loading entities in the context but none of
        /// the extra processing provided by OnUpdating / OnUpdated.
        /// </remarks>
        public async Task<int> DirectUpdateAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateExpression)
        {
            Ensure.ArgumentNotNull(predicate, nameof(predicate));

            return await DataContext.Set<TEntity>().Where(predicate).UpdateFromQueryAsync(updateExpression).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a direct UPDATE query on the database without returning objects or processing them through the interceptors.
        /// </summary>
        /// <param name="predicate">An <see cref="Expression{TDelegate}"/> to execute against the <see cref="DbSet{TEntity}"/></param> to return records that will be updated.
        /// <param name="updateExpression">An <see cref="Expression{TDelegate}"/> defining the updates to be performed on the records returned by the predicate.</param>
        /// <returns></returns>
        /// <remarks>
        /// This overload will give you all of the performance of updating a set of data without loading entities in the context but none of
        /// the extra processing provided by OnUpdating / OnUpdated.
        /// </remarks>
        public int DirectUpdate(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TEntity>> updateExpression)
        {
            Ensure.ArgumentNotNull(predicate, nameof(predicate));

            return DataContext.Set<TEntity>().Where(predicate).UpdateFromQuery(updateExpression);
        }

        #endregion

        #region Delete Methods

        /// <summary>
        /// Delete a specific <see cref="DbSet{TEntity}"/> with optional save operation.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(TEntity entity, bool save = true)
        {
            return await DeleteAsync(entity, DataContext, save).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a specific <see cref="DbSet{TEntity}"/> with optional save operation using a specified <see cref="DbContext"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="context"></param>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(TEntity entity, TContext context, bool save = true)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));
            Ensure.ArgumentNotNull(context, nameof(context));

            await OnDeletingAsync(entity).ConfigureAwait(false);
            context.Entry(entity).State = EntityState.Deleted;
            if (!save)
            {
                return true;
            }
            var changeCount = await context.SaveChangesAsync().ConfigureAwait(false);
            await OnDeletedAsync(entity).ConfigureAwait(false);
            return changeCount > 0;
        }

        /// <summary>
        /// Delete all <see cref="DbSet{TEntity}"/> from a list with optional save operation.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="save"></param>
        /// <returns></returns>
        /// <remarks>RWM: This will need to be Deleted to be generic if it's going to be in a NuGet package.</remarks>
        public async Task<bool> DeleteAsync(List<TEntity> entities, bool save = true)
        {
            return await DeleteAsync(entities, DataContext, save).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete all <see cref="DbSet{TEntity}"/> from a list with optional save operation using a specified <see cref="DbContext"/>.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="context"></param>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(List<TEntity> entities, TContext context, bool save = true)
        {
            Ensure.ArgumentNotNull(entities, nameof(entities));
            Ensure.ArgumentNotNull(context, nameof(context));

            var changeCount = 0;
            await OnDeletingAsync(entities).ConfigureAwait(false);
            entities.ForEach(c => context.Entry(c).State = EntityState.Deleted);
            if (!save)
            {
                return true;
            }
            changeCount = await context.SaveChangesAsync().ConfigureAwait(false);
            await OnDeletedAsync(entities).ConfigureAwait(false);
            return changeCount > 0;
        }

        /// <summary>
        /// Delete entities returned by the specified query without individual entity processing.
        /// </summary>
        /// <param name="predicate">An <see cref="Expression{TDelegate}"/> to execute against the <see cref="DbSet{TEntity}"/></param> to return records that will be deleted.
        /// <returns></returns>
        /// <remarks>
        /// This overload will give you all of the performance of deleting a set of data without loading entities in the context but none of
        /// the extra processing provided by OnDeleting / OnDeleted.
        /// </remarks>
        public async Task<int> DirectDeleteAsync(Expression<Func<TEntity,bool>> predicate)
        {
            Ensure.ArgumentNotNull(predicate, nameof(predicate));

            return await DataContext.Set<TEntity>().Where(predicate).DeleteFromQueryAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Delete entities returned by the specified query without individual entity processing.
        /// </summary>
        /// <param name="predicate">An <see cref="Expression{TDelegate}"/> to execute against the <see cref="DbSet{TEntity}"/></param> to return records that will be deleted.
        /// <returns></returns>
        /// <remarks>
        /// This overload will give you all of the performance of deleting a set of data without loading entities in the context but none of
        /// the extra processing provided by OnDeleting / OnDeleted.
        /// </remarks>
        public int DirectDelete(Expression<Func<TEntity,bool>> predicate)
        {
            Ensure.ArgumentNotNull(predicate, nameof(predicate));

            return DataContext.Set<TEntity>().Where(predicate).DeleteFromQuery();
        }

        #endregion

        #region Reset Methods

        /// <summary>
        /// Resets audit properties to an "Inserted" state by setting creation fields and clearing update fields.
        /// Sets CreatedById and DateCreated to current values, while clearing UpdatedById and DateUpdated.
        /// </summary>
        /// <typeparam name="TDbObservable">
        /// Any <see cref="DbObservableObject"/> in the object model. DOES NOT have to be the entity for this Manager.
        /// </typeparam>
        /// <param name="entity">The entity whose audit properties should be reset.</param>
        public void ResetAuditProperties<TDbObservable>(TDbObservable entity) where TDbObservable : DbObservableObject
        {
            var entityType = entity.GetType();
            if (!InterfaceDictionary.ContainsKey(entityType))
            {
                InterfaceDictionary[entityType] = entityType.GetInterfaces();
            }

            if (InterfaceDictionary[entityType].Any(c => c.Name == typeof(ICreatorTrackable<>).Name))
            {
                // TODO: RWM: This probably need to figure out how to check the type and make sure we don't just assume GUIDs.
                (entity as ICreatorTrackable<Guid>).CreatedById = ClaimsPrincipal.Current.GetIdClaim();
            }
            if (InterfaceDictionary[entityType].Any(c => c == typeof(ICreatedAuditable)))
            {
                (entity as ICreatedAuditable).DateCreated = DateTime.UtcNow;
            }
            if (InterfaceDictionary[entityType].Any(c => c.Name == typeof(IUpdaterTrackable<>).Name))
            {
                // TODO: RWM: This probably need to figure out how to check the type and make sure we don't just assume GUIDs.
                (entity as IUpdaterTrackable<Guid>).UpdatedById = null;
            }
            if (InterfaceDictionary[entityType].Any(c => c == typeof(IUpdatedAuditable)))
            {
                (entity as IUpdatedAuditable).DateUpdated = null;
            }
        }

        #endregion

        #endregion

    }

}
