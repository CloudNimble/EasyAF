using CloudNimble.EasyAF.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{

    /// <summary>
    /// Provides extension methods for registering EasyAF configuration services in the dependency injection container.
    /// </summary>
    public static class EasyAF_Configuration_IServiceCollectionExtensions
    {

        /// <summary>
        /// Adds a configuration class that inherits from <see cref="ConfigurationBase"/> to the service collection.
        /// The configuration is bound from the specified configuration section and registered as both the specific
        /// type and the base <see cref="ConfigurationBase"/> type for dependency injection.
        /// </summary>
        /// <typeparam name="TConfiguration">The type of configuration class that inherits from <see cref="ConfigurationBase"/>.</typeparam>
        /// <param name="services">The service collection to add the configuration to.</param>
        /// <param name="configuration">The configuration instance to bind from.</param>
        /// <param name="configSectionName">The name of the configuration section to bind from.</param>
        /// <returns>The bound configuration instance for immediate use or further configuration.</returns>
        /// <example>
        /// <code>
        /// // In Program.cs or Startup.cs
        /// var myConfig = builder.Services.AddConfigurationBase&lt;MyAppConfiguration&gt;(
        ///     builder.Configuration, 
        ///     "AppSettings"
        /// );
        /// 
        /// // The configuration can now be injected as either type:
        /// // [Inject] public MyAppConfiguration Config { get; set; }
        /// // [Inject] public ConfigurationBase BaseConfig { get; set; }
        /// </code>
        /// </example>
        public static TConfiguration AddConfigurationBase<TConfiguration>(this IServiceCollection services, IConfiguration configuration, string configSectionName)
            where TConfiguration : ConfigurationBase
        {
            var config = configuration.GetSection(configSectionName).Get<TConfiguration>();
            services.AddSingleton(c => config);

            if (typeof(TConfiguration) != typeof(ConfigurationBase))
            {
                services.AddSingleton(sp => sp.GetRequiredService<TConfiguration>() as ConfigurationBase);
            }
            return config;
        }

    }

}
