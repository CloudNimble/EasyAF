using CloudNimble.EasyAF.Core;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{

    /// <summary>
    /// Provides extension methods for the <see cref="EntityTypeBuilder{TEntity}"/> class to configure EasyAF-based types in the Entity Framework Core model.
    /// </summary>
    public static class DataEFCore_EntityTypeBuilderExtensions
    {

        /// <summary>
        /// Configures the entity type to ignore tracking fields defined in the <see cref="DbObservableObject"/> class.
        /// </summary>
        /// <typeparam name="T">The type of the entity being configured.</typeparam>
        /// <param name="builder">The <see cref="EntityTypeBuilder{TEntity}"/> used to configure the entity type.</param>
        /// <returns>The same <see cref="EntityTypeBuilder{TEntity}"/> instance so that multiple calls can be chained.</returns>
        public static EntityTypeBuilder<T> IgnoreTrackingFields<T>(this EntityTypeBuilder<T> builder)
            where T : DbObservableObject
        {
            builder.Ignore(c => c.IsChanged);
            builder.Ignore(c => c.IsGraphChanged);
            builder.Ignore(c => c.ShouldTrackChanges);
            builder.Ignore(c => c.OriginalValues);
            return builder;
        }

    }

}
