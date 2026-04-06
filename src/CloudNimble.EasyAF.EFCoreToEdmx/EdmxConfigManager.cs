using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.EFCoreToEdmx
{

    /// <summary>
    /// Manages loading and saving of EDMX configuration files.
    /// </summary>
    /// <remarks>
    /// This class handles serialization and deserialization of .edmx.config files
    /// that store settings for database scaffolding and EDMX generation. The configuration
    /// files use JSON format with consistent formatting for source control friendliness.
    /// </remarks>
    public class EdmxConfigManager
    {

        /// <summary>
        /// JSON serializer options used for consistent formatting of configuration files.
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {

            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull

        };

        /// <summary>
        /// Loads an EDMX configuration from the specified file path.
        /// </summary>
        /// <param name="configPath">The path to the .edmx.config file to load.</param>
        /// <returns>The loaded <see cref="EdmxConfig"/> object.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="configPath"/> is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the configuration file does not exist.</exception>
        /// <exception cref="JsonException">Thrown when the configuration file contains invalid JSON.</exception>
        /// <exception cref="InvalidOperationException">Thrown when both IncludedTables and ExcludedTables are specified.</exception>
        /// <remarks>
        /// The configuration file is expected to be in JSON format. After loading, the configuration
        /// is validated to ensure that only one of IncludedTables or ExcludedTables is specified.
        /// </remarks>
        /// <example>
        /// <code>
        /// var manager = new EdmxConfigManager();
        /// var config = await manager.LoadConfigAsync("MyModel.edmx.config");
        /// </code>
        /// </example>
        public async Task<EdmxConfig> LoadConfigAsync(string configPath)
        {

            ArgumentException.ThrowIfNullOrWhiteSpace(configPath, nameof(configPath));

            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Configuration file not found: {configPath}");

            var json = await File.ReadAllTextAsync(configPath);
            var config = JsonSerializer.Deserialize<EdmxConfig>(json, JsonOptions)
                ?? throw new JsonException("Failed to deserialize configuration file.");

            ValidateConfig(config);
            return config;

        }

        /// <summary>
        /// Saves an EDMX configuration to the specified file path.
        /// </summary>
        /// <param name="config">The <see cref="EdmxConfig"/> to save.</param>
        /// <param name="configPath">The path where the .edmx.config file should be saved.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="configPath"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when both IncludedTables and ExcludedTables are specified.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the application does not have permission to write to the specified path.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified in <paramref name="configPath"/> does not exist.</exception>
        /// <remarks>
        /// The configuration is validated before saving to ensure it contains valid settings.
        /// The file will be created if it doesn't exist, or overwritten if it does exist.
        /// The JSON output is formatted with indentation for readability.
        /// </remarks>
        /// <example>
        /// <code>
        /// var manager = new EdmxConfigManager();
        /// var config = new EdmxConfig 
        /// { 
        ///     ConnectionStringSource = "appsettings.json:ConnectionStrings:DefaultConnection",
        ///     Provider = "SqlServer" 
        /// };
        /// await manager.SaveConfigAsync(config, "MyModel.edmx.config");
        /// </code>
        /// </example>
        public async Task SaveConfigAsync(EdmxConfig config, string configPath)
        {

            ArgumentNullException.ThrowIfNull(config, nameof(config));
            ArgumentException.ThrowIfNullOrWhiteSpace(configPath, nameof(configPath));

            ValidateConfig(config);

            var json = JsonSerializer.Serialize(config, JsonOptions);
            await File.WriteAllTextAsync(configPath, json);

        }

        /// <summary>
        /// Checks if a configuration file exists at the specified path.
        /// </summary>
        /// <param name="configPath">The path to check for the configuration file.</param>
        /// <returns><c>true</c> if the configuration file exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="configPath"/> is null or empty.</exception>
        /// <remarks>
        /// This method is useful for determining whether to load an existing configuration
        /// or create a new one during the initial setup process.
        /// </remarks>
        public bool ConfigExists(string configPath)
        {

            ArgumentException.ThrowIfNullOrWhiteSpace(configPath, nameof(configPath));

            return File.Exists(configPath);

        }

        /// <summary>
        /// Creates a default configuration with commonly used settings.
        /// </summary>
        /// <param name="connectionStringSource">The connection string source to use.</param>
        /// <param name="provider">The database provider to use ("SqlServer" or "PostgreSQL").</param>
        /// <param name="contextName">The name for the generated DbContext class.</param>
        /// <returns>A new <see cref="EdmxConfig"/> with default settings.</returns>
        /// <exception cref="ArgumentException">Thrown when any parameter is null or empty.</exception>
        /// <remarks>
        /// This method creates a configuration with sensible defaults:
        /// - UsePluralizer = true
        /// - UseDataAnnotations = true
        /// - No table include/exclude filters (scaffold all tables)
        /// - Empty namespaces (uses project defaults)
        /// </remarks>
        /// <example>
        /// <code>
        /// var manager = new EdmxConfigManager();
        /// var config = manager.CreateDefaultConfig(
        ///     "appsettings.json:ConnectionStrings:DefaultConnection", 
        ///     "SqlServer", 
        ///     "MyDbContext"
        /// );
        /// </code>
        /// </example>
        public EdmxConfig CreateDefaultConfig(string connectionStringSource, string provider, string contextName)
        {

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringSource, nameof(connectionStringSource));
            ArgumentException.ThrowIfNullOrWhiteSpace(provider, nameof(provider));
            ArgumentException.ThrowIfNullOrWhiteSpace(contextName, nameof(contextName));

            return new EdmxConfig
            {

                ConnectionStringSource = connectionStringSource,
                Provider = provider,
                ContextName = contextName,
                UsePluralizer = true,
                UseDataAnnotations = true,
                DbContextNamespace = string.Empty,
                ObjectsNamespace = string.Empty

            };

        }

        /// <summary>
        /// Validates that the configuration contains valid settings.
        /// </summary>
        /// <param name="config">The configuration to validate.</param>
        /// <exception cref="InvalidOperationException">Thrown when the configuration contains invalid settings.</exception>
        /// <remarks>
        /// This method checks that:
        /// - Only one of IncludedTables or ExcludedTables is specified (not both)
        /// - The provider is a supported value
        /// - Required fields are not empty
        /// </remarks>
        private static void ValidateConfig(EdmxConfig config)
        {
            if (config.IncludedTables?.Count > 0 && config.ExcludedTables?.Count > 0)
            {
                throw new InvalidOperationException(
                    "Configuration cannot specify both IncludedTables and ExcludedTables. " +
                    "Use either IncludedTables to specify which tables to include, " +
                    "or ExcludedTables to specify which tables to exclude, but not both."
                );
            }

            if (string.IsNullOrWhiteSpace(config.ConnectionStringSource))
                throw new InvalidOperationException("ConnectionStringSource is required.");

            if (string.IsNullOrWhiteSpace(config.Provider))
                throw new InvalidOperationException("Provider is required.");

            if (config.Provider != "SqlServer" && config.Provider != "PostgreSQL")
                throw new InvalidOperationException($"Unsupported provider: {config.Provider}. Supported providers are 'SqlServer' and 'PostgreSQL'.");

            if (string.IsNullOrWhiteSpace(config.ContextName))
                throw new InvalidOperationException("ContextName is required.");
        }

    }

}
