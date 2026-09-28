using Ben.Collections;
using CloudNimble.EasyAF.Core;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Benchmarks.V4
{

    /// <summary>
    /// The EasyAF 4.x EntityManager and IdentifiableEntityManager lifecycle code, copied verbatim so the V4 managers behave exactly like 4.x.
    /// </summary>
    /// <remarks>
    /// The V4 managers inherit the EasyAF 5.0 base classes, because InsertAsync, UpdateAsync, and everything else outside these methods
    /// did not change between 4.x and 5.0. They override the lifecycle methods to call these instead.
    /// </remarks>
    public static class EntityManagerHooks
    {

        #region Private Static Members

        internal static readonly TypeDictionary<Type[]> InterfaceDictionary = new();

        #endregion

        #region Public Methods

        /// <summary>
        /// The EasyAF 4.x EntityManager constructor body, which cached the entity type's interfaces.
        /// </summary>
        /// <param name="entityType">The manager's entity type.</param>
        public static void RegisterEntityType(Type entityType)
        {
            if (!InterfaceDictionary.ContainsKey(entityType))
            {
                InterfaceDictionary[entityType] = entityType.GetInterfaces();
            }
        }

        /// <summary>
        /// The EasyAF 4.x IdentifiableEntityManager.OnInsertingAsync, which then calls the 4.x EntityManager.OnInsertingAsync.
        /// </summary>
        /// <param name="entity">The entity being inserted.</param>
        public static async Task OnInsertingIdentifiableAsync(object entity)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));

            var entityType = entity.GetType();
            // RWM: We have to do this cast because we're only doing this update for GUIDs. Numeric values should be set at the database level.
            if (InterfaceDictionary[entityType].Any(c => c == typeof(IIdentifiable<Guid>)) && (entity as IIdentifiable<Guid>).Id == Guid.Empty)
            {
                (entity as IIdentifiable<Guid>).Id = Guid.NewGuid();
            }
            await OnInsertingAsync(entity).ConfigureAwait(false);
        }

        /// <summary>
        /// The EasyAF 4.x EntityManager.OnInsertingAsync.
        /// </summary>
        /// <param name="entity">The entity being inserted.</param>
        public static async Task OnInsertingAsync(object entity)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));

            var entityType = entity.GetType();
            if (InterfaceDictionary[entityType].Any(c => c.Name == typeof(ICreatorTrackable<>).Name) && ClaimsPrincipal.Current is not null)
            {
                (entity as ICreatorTrackable<Guid>).CreatedById = ClaimsPrincipalExtensions.GetIdClaim(ClaimsPrincipal.Current);
            }
            if (InterfaceDictionary[entityType].Any(c => c == typeof(ICreatedAuditable)))
            {
                (entity as ICreatedAuditable).DateCreated = DateTime.UtcNow;
            }
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// The EasyAF 4.x EntityManager.OnUpdatingAsync.
        /// </summary>
        /// <param name="entity">The entity being updated.</param>
        public static async Task OnUpdatingAsync(object entity)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));

            var entityType = entity.GetType();
            if (InterfaceDictionary[entityType].Any(c => c.Name == typeof(IUpdaterTrackable<>).Name) && ClaimsPrincipal.Current is not null)
            {
                (entity as IUpdaterTrackable<Guid>).UpdatedById = ClaimsPrincipalExtensions.GetIdClaim(ClaimsPrincipal.Current);
            }
            if (InterfaceDictionary[entityType].Any(c => c == typeof(IUpdatedAuditable)))
            {
                (entity as IUpdatedAuditable).DateUpdated = DateTime.UtcNow;
            }
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// The EasyAF 4.x EntityManager.ResetAuditProperties.
        /// </summary>
        /// <param name="entity">The entity whose audit properties should be reset.</param>
        public static void ResetAuditProperties(DbObservableObject entity)
        {
            var entityType = entity.GetType();
            if (!InterfaceDictionary.ContainsKey(entityType))
            {
                InterfaceDictionary[entityType] = entityType.GetInterfaces();
            }

            if (InterfaceDictionary[entityType].Any(c => c.Name == typeof(ICreatorTrackable<>).Name))
            {
                (entity as ICreatorTrackable<Guid>).CreatedById = ClaimsPrincipalExtensions.GetIdClaim(ClaimsPrincipal.Current);
            }
            if (InterfaceDictionary[entityType].Any(c => c == typeof(ICreatedAuditable)))
            {
                (entity as ICreatedAuditable).DateCreated = DateTime.UtcNow;
            }
            if (InterfaceDictionary[entityType].Any(c => c.Name == typeof(IUpdaterTrackable<>).Name))
            {
                (entity as IUpdaterTrackable<Guid>).UpdatedById = null;
            }
            if (InterfaceDictionary[entityType].Any(c => c == typeof(IUpdatedAuditable)))
            {
                (entity as IUpdatedAuditable).DateUpdated = null;
            }
        }

        #endregion

    }

}
