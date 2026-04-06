using Microsoft.Extensions.DependencyInjection;

namespace CloudNimble.EasyAF.EFCoreToEdmx.Extensions
{

    /// <summary>
    /// Provides extension methods for configuring EF Core to EDMX services in an <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>These extension methods allow for the registration and configuration of services related to
    /// EF Core to EDMX functionality within the dependency injection container.</remarks>
    public static  class EFCoreToEdmx_IServiceCollectionExtensions
    {

        /// <summary>
        /// Registers services required for Entity Framework Core to EDMX conversion.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the services will be added.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance, allowing for method chaining.</returns>
        /// <remarks>
        /// This method adds the following services to the dependency injection container: 
        /// <list type="bullet"> <item><description><see cref="EdmxModelBuilder"/> for building EDMX
        /// models.</description></item> <item><description><see cref="DatabaseScaffolder"/> for scaffolding database
        /// schemas.</description></item> <item><description><see cref="ConnectionStringResolver"/> for resolving
        /// database connection strings.</description></item> <item><description><see cref="EdmxConfigManager"/> for
        /// managing EDMX configuration settings.</description></item> <item><description><see cref="EdmxConverter"/>
        /// for converting models to EDMX format.</description></item> </list>
        /// </remarks>
        public static IServiceCollection AddEFCoreToEdmxServices(this IServiceCollection services)
        {
            services
                .AddScoped<EdmxModelBuilder>()
                .AddScoped<DatabaseScaffolder>()
                .AddScoped<ConnectionStringResolver>()
                .AddScoped<EdmxConfigManager>()
                .AddScoped<EdmxConverter>();
            return services;
        }

    }

}
