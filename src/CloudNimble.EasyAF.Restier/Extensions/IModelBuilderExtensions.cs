using CloudNimble.EasyAF.Core;
using Microsoft.AspNet.OData.Builder;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Restier.Core.Model
{

    /// <summary>
    /// Provides extension methods for Restier model configuration to handle EasyAF-specific entity properties.
    /// Includes methods to ignore tracking fields and audit fields in OData model generation.
    /// </summary>
    public static class IModelBuilderExtensions
    {

        /// <summary>
        /// Configures the entity set to ignore DbObservableObject tracking fields in the OData model.
        /// Excludes IsChanged, IsGraphChanged, ShouldTrackChanges, and OriginalValues from the model.
        /// </summary>
        /// <typeparam name="T">The entity type that inherits from DbObservableObject.</typeparam>
        /// <param name="configuration">The entity set configuration to modify.</param>
        /// <returns>The entity set configuration for method chaining.</returns>
        public static EntitySetConfiguration<T> IgnoreTrackingFields<T>(this EntitySetConfiguration<T> configuration) where T : DbObservableObject
        {
            configuration.EntityType.Ignore(c => c.IsChanged);
            configuration.EntityType.Ignore(c => c.IsGraphChanged);
            configuration.EntityType.Ignore(c => c.ShouldTrackChanges);
            configuration.EntityType.Ignore(c => c.OriginalValues);
            return configuration;
        }

        /// <summary>
        /// Configures the entity set to ignore audit trail fields in the OData model.
        /// Dynamically removes DateCreated, DateUpdated, CreatedById, and UpdatedById properties based on implemented interfaces.
        /// </summary>
        /// <typeparam name="T">The entity type that inherits from EasyObservableObject.</typeparam>
        /// <param name="configuration">The entity set configuration to modify.</param>
        /// <returns>The entity set configuration for method chaining.</returns>
        public static EntitySetConfiguration<T> IgnoreAuditFields<T>(this EntitySetConfiguration<T> configuration) where T : EasyObservableObject
        {
            var configInfo = configuration.EntityType.GetType().GetField("_configuration", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance);
            var structuralConfig = (StructuralTypeConfiguration)configInfo.GetValue(configuration.EntityType);

            var properties = typeof(T).GetProperties();

            if (typeof(T).IsAssignableTo(typeof(ICreatedAuditable)))
            {
                structuralConfig.RemoveProperty(properties.Where(c => c.Name == nameof(ICreatedAuditable.DateCreated)).FirstOrDefault());
            }

            if (typeof(T).IsAssignableTo(typeof(IUpdatedAuditable)))
            {
                structuralConfig.RemoveProperty(properties.Where(c => c.Name == nameof(IUpdatedAuditable.DateUpdated)).FirstOrDefault());
            }

            if (typeof(T).IsAssignableTo(typeof(ICreatorTrackable<int>)) || typeof(T).IsAssignableTo(typeof(ICreatorTrackable<Guid>)))
            {
                structuralConfig.RemoveProperty(properties.Where(c => c.Name == nameof(ICreatorTrackable<Guid>.CreatedById)).FirstOrDefault());
            }

            if (typeof(T).IsAssignableTo(typeof(IUpdaterTrackable<int>)) || typeof(T).IsAssignableTo(typeof(IUpdaterTrackable<Guid>)))
            {
                structuralConfig.RemoveProperty(properties.Where(c => c.Name == nameof(IUpdaterTrackable<Guid>.UpdatedById)).FirstOrDefault());
            }
            return configuration;
        }

    }

}
