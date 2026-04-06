using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CloudNimble.EasyAF.EFCoreToEdmx
{
    /// <summary>
    /// Converts Entity Framework Core DbContext models to EDMX format for use with legacy tooling,
    /// Microsoft Restier, and custom code generation platforms.
    /// </summary>
    /// <remarks>
    /// This converter extracts metadata from EF Core models and generates complete EDMX XML files
    /// that preserve entity relationships, property attributes, documentation, and other model metadata.
    /// The generated EDMX files are compatible with Entity Data Model tools and can be consumed
    /// by Roslyn-based code generators. Additionally supports reverse engineering databases directly
    /// into EDMX files with OnModelCreating method extraction.
    /// </remarks>
    public class EdmxConverter
    {
        #region Fields

        /// <summary>
        /// The model builder responsible for extracting EF Core metadata and converting it to EDMX model structure.
        /// </summary>
        private readonly EdmxModelBuilder _modelBuilder;

        /// <summary>
        /// The database scaffolder responsible for reverse engineering database schemas.
        /// </summary>
        private readonly DatabaseScaffolder _databaseScaffolder;

        /// <summary>
        /// The connection string resolver for finding connection strings in configuration sources.
        /// </summary>
        private readonly ConnectionStringResolver _connectionStringResolver;

        /// <summary>
        /// The configuration manager for handling .edmx.config files.
        /// </summary>
        private readonly EdmxConfigManager _configManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmxConverter"/> class.
        /// </summary>
        /// <remarks>
        /// Creates instances of all required components for both DbContext-based conversion
        /// and database scaffolding operations.
        /// </remarks>
        public EdmxConverter()
        {
            _modelBuilder = new EdmxModelBuilder();
            _databaseScaffolder = new DatabaseScaffolder();
            _connectionStringResolver = new ConnectionStringResolver();
            _configManager = new EdmxConfigManager();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Probes the specified path for assemblies, discovers a public DbContext, and generates EDMX XML.
        /// </summary>
        /// <param name="path">The directory path to probe for assemblies containing a DbContext.</param>
        /// <returns>A string containing the complete EDMX XML content.</returns>
        /// <exception cref="ArgumentException">Thrown if no DbContext is found or multiple are found.</exception>
        /// <example>
        /// <code>
        /// var converter = new EdmxConverter();
        /// string edmxContent = converter.ConvertToEdmx(@"C:\MyProject\bin\Debug\net8.0");
        /// </code>
        /// </example>
        public EdmxConversionResult ConvertToEdmx(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                throw new ArgumentException("The specified path does not exist.", nameof(path));
            }

            // Find all DLLs in the directory
            var dlls = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
            if (dlls.Length == 0)
            {
                throw new InvalidOperationException($"No assemblies found in {path}.");
            }

            // Try to find a DbContext in any of the assemblies
            foreach (var dll in dlls)
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(dll);
                }
                catch
                {
                    continue; // skip non-.NET assemblies
                }

                var dbContextTypes = assembly.GetTypes()
                    .Where(t => typeof(DbContext).IsAssignableFrom(t) && !t.IsAbstract && t.IsPublic)
                    .ToList();

                if (dbContextTypes.Count == 0)
                {
                    continue;
                }

                if (dbContextTypes.Count > 1)
                {
                    throw new InvalidOperationException(
                        $"Multiple DbContext types found in {dll}: {string.Join(", ", dbContextTypes.Select(t => t.FullName))}. " +
                        "Please specify a single context or ensure only one exists in the assembly.");
                }

                var contextType = dbContextTypes[0];

                // Try to use IDesignTimeDbContextFactory<T> if available (including base types)
                var factoryType = assembly.GetTypes()
                    .FirstOrDefault(t => ImplementsDesignTimeFactory(t, contextType));

                DbContext context = null;

                if (factoryType is not null)
                {
                    var factory = Activator.CreateInstance(factoryType);
                    var method = factoryType.GetMethod("CreateDbContext");
                    context = (DbContext)method.Invoke(factory, [Array.Empty<string>()]);
                }
                else
                {
                    // Try to create with default constructor
                    context = (DbContext)Activator.CreateInstance(contextType);
                }

                if (context is null)
                {
                    throw new InvalidOperationException($"Could not instantiate DbContext of type {contextType.FullName}.");
                }

                return ConvertToEdmx(context);
            }

            throw new InvalidOperationException("No public DbContext types found in any assemblies in the specified path.");
        }

        /// <summary>
        /// Converts the specified Entity Framework Core DbContext to EDMX format.
        /// </summary>
        /// <param name="context">The EF Core DbContext to convert. Must not be null.</param>
        /// <returns>A string containing the complete EDMX XML content.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        /// <remarks>
        /// This method extracts all metadata from the provided DbContext including entity types,
        /// properties, relationships, keys, and documentation. The resulting EDMX XML includes
        /// conceptual model, storage model, and mapping sections.
        /// </remarks>
        /// <example>
        /// <code>
        /// using var context = new MyDbContext(options);
        /// var converter = new EdmxConverter();
        /// string edmxContent = converter.ConvertToEdmx(context);
        /// </code>
        /// </example>
        public EdmxConversionResult ConvertToEdmx(DbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            // Try to determine the provider type from the context
            var providerType = DetermineProviderTypeFromContext(context);

            var contextType = context.GetType();

            // Get the design-time model which includes all annotations (comments, etc.)
            // The runtime model (context.Model) is read-optimized and doesn't include comments
            var designTimeModel = context.GetService<IDesignTimeModel>();
            var model = designTimeModel?.Model ?? context.Model;

            // Get actual table names from the model
            var tableInfos = GetTableInfoFromModel(model);

            // Build the EDMX model with provider type and table info
            var edmxModel = _modelBuilder.BuildEdmxModel(
                model,
                contextType.Namespace,
                contextType.Name,
                providerType,
                tableInfos);

            // Generate XML using the simplified generator
            var xmlGenerator = new EdmxXmlGenerator(edmxModel, providerType, tableInfos);
            return new EdmxConversionResult(context.GetType().Name, xmlGenerator.Generate());
        }

        /// <summary>
        /// Converts the specified Entity Framework Core DbContext to EDMX format and saves it to a file.
        /// </summary>
        /// <param name="context">The EF Core DbContext to convert. Must not be null.</param>
        /// <param name="filePath">The file path where the EDMX content should be saved. Must not be null or empty.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the application does not have permission to write to the specified file path.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified in <paramref name="filePath"/> does not exist.</exception>
        /// <remarks>
        /// This method performs the same conversion as <see cref="ConvertToEdmx(DbContext)"/> but
        /// writes the output directly to a file. The file will be created if it doesn't exist,
        /// or overwritten if it does exist.
        /// </remarks>
        /// <example>
        /// <code>
        /// using var context = new MyDbContext(options);
        /// var converter = new EdmxConverter();
        /// await converter.ConvertToEdmxFileAsync(context, "MyModel.edmx");
        /// </code>
        /// </example>
        public async Task ConvertToEdmxFileAsync(DbContext context, string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

            var result = ConvertToEdmx(context);
            await File.WriteAllTextAsync(filePath, result.EdmxContent);
        }

        /// <summary>
        /// Converts a database schema to EDMX format using the specified configuration.
        /// </summary>
        /// <param name="configPath">The path to the .edmx.config file containing scaffolding settings.</param>
        /// <param name="projectPath">The path to the project directory containing configuration files.</param>
        /// <returns>A tuple containing the EDMX XML content and the extracted OnModelCreating method body.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parameters are null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the configuration file is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when scaffolding or conversion fails.</exception>
        /// <remarks>
        /// This method loads the configuration from the specified .edmx.config file, connects to the database,
        /// scaffolds the schema into a temporary DbContext, and converts it to EDMX format. The temporary
        /// DbContext and entities are not persisted to disk. Additionally extracts the OnModelCreating method
        /// body from the scaffolded context for potential reuse.
        /// </remarks>
        /// <example>
        /// <code>
        /// var converter = new EdmxConverter();
        /// var (edmxContent, onModelCreating) = await converter.ConvertFromDatabaseAsync("MyModel.edmx.config", @"C:\MyProject");
        /// </code>
        /// </example>
        public async Task<(string EdmxContent, string OnModelCreatingBody)> ConvertFromDatabaseAsync(string configPath, string projectPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(configPath, nameof(configPath));
            ArgumentException.ThrowIfNullOrWhiteSpace(projectPath, nameof(projectPath));

            var config = await _configManager.LoadConfigAsync(configPath);
            var connectionString = _connectionStringResolver.ResolveConnectionString(config.ConnectionStringSource, projectPath);

            var scaffoldingResult = await _databaseScaffolder.ScaffoldFromDatabaseAsync(connectionString, config);

            try
            {
                // Determine provider type from config
                var providerType = MapProviderStringToEnum(config.Provider);

                // Get the design-time model which includes all annotations (comments, etc.)
                // The runtime model (context.Model) is read-optimized and doesn't include comments
                var designTimeModel = scaffoldingResult.Context.GetService<IDesignTimeModel>();
                var model = designTimeModel?.Model ?? scaffoldingResult.Context.Model;

                // Get table infos from the design-time model
                var tableInfos = GetTableInfoFromModel(model);

                // Create a model builder with pluralization overrides from config
                var modelBuilderWithOverrides = new EdmxModelBuilder(null, config.PluralizationOverrides);

                // Build the EDMX model with all the information we have
                var edmxModel = modelBuilderWithOverrides.BuildEdmxModel(
                    model,
                    scaffoldingResult.Context.GetType().Namespace,
                    scaffoldingResult.Context.GetType().Name,
                    providerType,
                    tableInfos);

                // Store the OnModelCreating body in the model
                edmxModel.OnModelCreatingBody = scaffoldingResult.OnModelCreatingBody;

                // Generate the EDMX XML
                var xmlGenerator = new EdmxXmlGenerator(edmxModel, providerType, tableInfos);
                var edmxContent = xmlGenerator.Generate();

                return (edmxContent, scaffoldingResult.OnModelCreatingBody);
            }
            finally
            {
                scaffoldingResult.Cleanup();
            }
        }

        /// <summary>
        /// Refreshes an existing EDMX file from the database using the associated configuration.
        /// </summary>
        /// <param name="edmxPath">The path to the existing .edmx file.</param>
        /// <param name="projectPath">The path to the project directory containing configuration files.</param>
        /// <returns>A tuple containing the updated EDMX XML content and the OnModelCreating method body.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parameters are null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the .edmx or .edmx.config file is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when scaffolding or conversion fails.</exception>
        /// <remarks>
        /// This method looks for a corresponding .edmx.config file (by adding .config to the .edmx filename),
        /// loads the configuration, and regenerates the EDMX from the current database schema.
        /// This is useful for updating the EDMX when the database schema has changed.
        /// </remarks>
        /// <example>
        /// <code>
        /// var converter = new EdmxConverter();
        /// var (updatedEdmx, onModelCreating) = await converter.RefreshFromDatabaseAsync("MyModel.edmx", @"C:\MyProject");
        /// </code>
        /// </example>
        public async Task<(string EdmxContent, string OnModelCreatingBody)> RefreshFromDatabaseAsync(string edmxPath, string projectPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(edmxPath, nameof(edmxPath));
            ArgumentException.ThrowIfNullOrWhiteSpace(projectPath, nameof(projectPath));

            var configPath = edmxPath + ".config";
            return await ConvertFromDatabaseAsync(configPath, projectPath);
        }

        /// <summary>
        /// Creates a new EDMX configuration file with the specified settings.
        /// </summary>
        /// <param name="configPath">The path where the .edmx.config file should be created.</param>
        /// <param name="connectionStringSource">The connection string source (e.g., "appsettings.json:ConnectionStrings:DefaultConnection").</param>
        /// <param name="provider">The database provider ("SqlServer" or "PostgreSQL").</param>
        /// <param name="contextName">The name for the generated DbContext class.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null or empty.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the application does not have permission to write to the specified path.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory specified in the path does not exist.</exception>
        /// <remarks>
        /// This method creates a new configuration file with default settings for database scaffolding.
        /// The configuration can be modified after creation to customize table inclusion/exclusion,
        /// pluralization, and other scaffolding options.
        /// </remarks>
        /// <example>
        /// <code>
        /// var converter = new EdmxConverter();
        /// await converter.CreateConfigAsync(
        ///     "MyModel.edmx.config",
        ///     "appsettings.json:ConnectionStrings:DefaultConnection",
        ///     "SqlServer",
        ///     "MyDbContext"
        /// );
        /// </code>
        /// </example>
        public async Task CreateConfigAsync(string configPath, string connectionStringSource, string provider, string contextName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(configPath, nameof(configPath));

            var config = _configManager.CreateDefaultConfig(connectionStringSource, provider, contextName);
            await _configManager.SaveConfigAsync(config, configPath);
        }

        /// <summary>
        /// Checks if a configuration file exists for the specified EDMX file.
        /// </summary>
        /// <param name="edmxPath">The path to the .edmx file.</param>
        /// <returns><c>true</c> if a corresponding .edmx.config file exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="edmxPath"/> is null or empty.</exception>
        /// <remarks>
        /// This method checks for the existence of a .edmx.config file by appending ".config" to the
        /// provided .edmx file path. This is useful for determining whether an EDMX file was generated
        /// from a database and can be refreshed.
        /// </remarks>
        /// <example>
        /// <code>
        /// var converter = new EdmxConverter();
        /// if (converter.HasConfig("MyModel.edmx"))
        /// {
        ///     // This EDMX can be refreshed from the database
        ///     var (refreshedEdmx, onModelCreating) = await converter.RefreshFromDatabaseAsync("MyModel.edmx", projectPath);
        /// }
        /// </code>
        /// </example>
        public bool HasConfig(string edmxPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(edmxPath, nameof(edmxPath));

            var configPath = edmxPath + ".config";
            return _configManager.ConfigExists(configPath);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines whether the specified type implements the <see cref="IDesignTimeDbContextFactory{TContext}"/>
        /// interface for the given context type.
        /// </summary>
        /// <remarks>
        /// This method traverses the inheritance hierarchy of the <paramref
        /// name="candidateType"/> and inspects all implemented interfaces to determine if the type implements the
        /// generic <see cref="IDesignTimeDbContextFactory{TContext}"/> interface with the specified <paramref
        /// name="contextType"/> as the generic type argument.
        /// </remarks>
        /// <param name="candidateType">The type to inspect for the implementation of the design-time factory interface.</param>
        /// <param name="contextType">The type of the database context to check against the factory interface.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="candidateType"/> implements the <see
        /// cref="IDesignTimeDbContextFactory{TContext}"/> interface for the specified <paramref name="contextType"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool ImplementsDesignTimeFactory(Type candidateType, Type contextType)
        {
            while (candidateType is not null && candidateType != typeof(object))
            {
                var interfaces = candidateType.GetInterfaces();
                foreach (var iface in interfaces)
                {
                    if (iface.IsGenericType &&
                        iface.GetGenericTypeDefinition() == typeof(IDesignTimeDbContextFactory<>) &&
                        iface.GenericTypeArguments[0] == contextType)
                    {
                        return true;
                    }
                }
                candidateType = candidateType.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Maps a provider string from the configuration to a <see cref="DatabaseProviderType"/> enum value.
        /// </summary>
        /// <param name="providerString">The provider string from the configuration.</param>
        /// <returns>The corresponding <see cref="DatabaseProviderType"/> enum value.</returns>
        internal static DatabaseProviderType MapProviderStringToEnum(string providerString)
        {
            if (string.IsNullOrWhiteSpace(providerString))
            {
                return DatabaseProviderType.Unknown;
            }

            return providerString.ToLowerInvariant() switch
            {
                "sqlserver" => DatabaseProviderType.SqlServer,
                "postgresql" => DatabaseProviderType.PostgreSQL,
                _ => DatabaseProviderType.Unknown
            };
        }

        /// <summary>
        /// Determines the database provider type from the given DbContext.
        /// </summary>
        /// <param name="context">The DbContext instance.</param>
        /// <returns>The determined <see cref="DatabaseProviderType"/>.</returns>
        private DatabaseProviderType DetermineProviderTypeFromContext(DbContext context)
        {
            // Try to determine the provider type from the database provider
            var dbProviderName = context.Database.ProviderName;
            if (!string.IsNullOrWhiteSpace(dbProviderName))
            {
                if (dbProviderName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
                {
                    return DatabaseProviderType.SqlServer;
                }
                else if (dbProviderName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
                         dbProviderName.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                {
                    return DatabaseProviderType.PostgreSQL;
                }
            }
            
            return DatabaseProviderType.Unknown;
        }

        /// <summary>
        /// Extracts table information from the given EF Core model.
        /// </summary>
        /// <param name="model">The EF Core model (should be the design-time model for full metadata access).</param>
        /// <returns>A dictionary mapping CLR type names to table names.</returns>
        private static Dictionary<string, string> GetTableInfoFromModel(IModel model)
        {
            var tableInfos = new Dictionary<string, string>();

            // Extract actual table names from EF Core metadata
            foreach (var entityType in model.GetEntityTypes())
            {
                var clrTypeName = entityType.ClrType.Name;
                var tableName = entityType.GetTableName();
                var schema = entityType.GetSchema() ?? "dbo";

                if (!string.IsNullOrWhiteSpace(tableName))
                {
                    tableInfos[clrTypeName] = $"{schema}.{tableName}";
                }
            }

            return tableInfos;
        }

        #endregion
    }
}
