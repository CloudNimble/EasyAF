using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.MSBuild;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Build.Evaluation;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using McMasterAllowedValues = McMaster.Extensions.CommandLineUtils.AllowedValuesAttribute;

namespace CloudNimble.EasyAF.Tools.Commands
{

    /// <summary>
    /// Command for initializing database scaffolding configuration.
    /// </summary>
    [Command(Name = "init", Description = "Initialize database scaffolding configuration")]
    public class DatabaseInitCommand
    {

        #region Fields

        private readonly EdmxConfigManager _configManager;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the connection string source.
        /// </summary>
        [Option("-c|--connection-string", Description = "Connection string source (e.g., 'appsettings.json:ConnectionStrings:DefaultConnection') or actual connection string")]
        [Required]
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the DbContext class name.
        /// </summary>
        [Option("-x|--context-name", Description = "DbContext class name")]
        [Required]
        public string ContextName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the namespace for the generated DbContext.
        /// </summary>
        [Option("--dbcontext-namespace", Description = "Namespace for generated DbContext (defaults to .Data project namespace)")]
        public string DbContextNamespace { get; set; }

        /// <summary>
        /// Gets or sets the tables to exclude.
        /// </summary>
        [Option("-e|--exclude-tables", Description = "Tables to exclude from scaffolding")]
        public string[] ExcludeTables { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable data annotations.
        /// </summary>
        [Option("--no-data-annotations", Description = "Use fluent API instead of data annotations")]
        public bool NoDataAnnotations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable pluralization.
        /// </summary>
        [Option("--no-pluralizer", Description = "Disable pluralization of entity names")]
        public bool NoPluralize { get; set; }

        /// <summary>
        /// Gets or sets the namespace for the generated entity objects.
        /// </summary>
        [Option("--objects-namespace", Description = "Namespace for generated entity objects (defaults to .Core project namespace)")]
        public string ObjectsNamespace { get; set; }

        /// <summary>
        /// Gets or sets the database provider.
        /// </summary>
        [Option("-p|--provider", Description = "Database provider (SqlServer or PostgreSQL)")]
        [Required]
        [McMasterAllowedValues("SqlServer", "PostgreSQL", IgnoreCase = true)]
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the working directory for the solution. Defaults to current directory.
        /// </summary>
        [Option("-s|--solution-folder", Description = "Solution directory (defaults to current directory)")]
        public string SolutionFolder { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// Gets or sets the specific tables to include.
        /// </summary>
        [Option("-t|--tables", Description = "Specific tables to include (if not specified, all tables will be included)")]
        public string[] Tables { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseInitCommand"/> class.
        /// </summary>
        /// <param name="configManager">The configuration manager service.</param>
        public DatabaseInitCommand(EdmxConfigManager configManager)
        {
            ArgumentNullException.ThrowIfNull(configManager);
            _configManager = configManager;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the init command.
        /// </summary>
        /// <returns>Exit code.</returns>
        public async Task<int> OnExecuteAsync()
        {
            try
            {
                Console.WriteLine("Initializing database scaffolding configuration...");

                // Validate table options
                if (Tables?.Length > 0 && ExcludeTables?.Length > 0)
                {
                    Console.Error.WriteLine("Error: Cannot specify both --tables and --exclude-tables options.");
                    return 1;
                }

                // Find the .Data folder if not explicitly overridden via namespace options
                var dataFolder = EdmxRootCommand.FindDataFolder(SolutionFolder);
                if (string.IsNullOrWhiteSpace(dataFolder))
                {
                    Console.Error.WriteLine($"Error: Could not find a project ending in '.Data' in the solution directory: {SolutionFolder}");
                    Console.Error.WriteLine("Please ensure you have a .Data project in your solution or specify the correct solution directory with --solution-folder.");
                    return 1;
                }

                Console.WriteLine($"Found Data project: {dataFolder}");

                // Auto-detect namespaces if not specified
                var dbContextNamespace = DbContextNamespace;
                var objectsNamespace = ObjectsNamespace;

                if (string.IsNullOrWhiteSpace(dbContextNamespace) || string.IsNullOrWhiteSpace(objectsNamespace))
                {
                    var dataFolderName = Path.GetFileName(dataFolder);
                    
                    if (string.IsNullOrWhiteSpace(dbContextNamespace))
                    {
                        dbContextNamespace = dataFolderName;
                        Console.WriteLine($"Auto-detected DbContext namespace: {dbContextNamespace}");
                    }

                    if (string.IsNullOrWhiteSpace(objectsNamespace))
                    {
                        if (dataFolderName.EndsWith(".Data"))
                        {
                            objectsNamespace = dataFolderName[..^5] + ".Core";
                        }
                        else
                        {
                            objectsNamespace = dataFolderName + ".Core";
                        }
                        Console.WriteLine($"Auto-detected Objects namespace: {objectsNamespace}");
                    }
                }

                // Process connection string (check if it's an actual connection string or a source reference)
                var connectionStringSource = await ProcessConnectionStringAsync(ConnectionString, dataFolder);

                var config = _configManager.CreateDefaultConfig(connectionStringSource, Provider, ContextName);

                // Apply custom settings
                if (Tables?.Length > 0)
                {
                    config.IncludedTables = [.. Tables];
                }

                if (ExcludeTables?.Length > 0)
                {
                    config.ExcludedTables = [.. ExcludeTables];
                }

                config.UsePluralizer = !NoPluralize;
                config.UseDataAnnotations = !NoDataAnnotations;
                config.DbContextNamespace = dbContextNamespace;
                config.ObjectsNamespace = objectsNamespace;

                var configFileName = $"{ContextName}.edmx.config";
                var configPath = Path.Combine(dataFolder, configFileName);

                await _configManager.SaveConfigAsync(config, configPath);

                Console.WriteLine($"Configuration saved to: {configPath}");
                Console.WriteLine("You can now generate EDMX files using:");
                Console.WriteLine($"dotnet easyaf database generate --context-name \"{ContextName}\"");

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error initializing configuration: {ex.Message}");
                return 1;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Processes the connection string parameter, determining if it's a source reference or actual connection string.
        /// </summary>
        /// <param name="connectionString">The connection string parameter value.</param>
        /// <param name="dataFolder">The data project folder path.</param>
        /// <returns>The connection string source to store in configuration.</returns>
        private async Task<string> ProcessConnectionStringAsync(string connectionString, string dataFolder)
        {
            // Check if it looks like a connection string source reference (format: filename:section:key)
            var parts = connectionString.Split(':', 3);
            if (parts.Length == 3)
            {
                var filename = parts[0];
                var isKnownSource = filename.Equals("appsettings.json", StringComparison.OrdinalIgnoreCase) ||
                                  filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
                                  filename.Equals("secrets", StringComparison.OrdinalIgnoreCase) ||
                                  filename.Equals("user-secrets", StringComparison.OrdinalIgnoreCase) ||
                                  filename.Equals("environment", StringComparison.OrdinalIgnoreCase);

                if (isKnownSource)
                {
                    Console.WriteLine($"Using connection string source: {connectionString}");
                    return connectionString;
                }
            }

            // It's an actual connection string - store it in user secrets
            Console.WriteLine("Detected actual connection string. Storing securely in user secrets...");
            
            try
            {
                await StoreConnectionStringInUserSecretsAsync(connectionString, dataFolder);
                var userSecretsSource = $"secrets:ConnectionStrings:{ContextName}Connection";
                Console.WriteLine($"Connection string stored in user secrets as: {userSecretsSource}");
                return userSecretsSource;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not store connection string in user secrets: {ex.Message}");
                Console.WriteLine("Falling back to using the connection string source as-is.");
                Console.WriteLine("Note: This may expose sensitive connection information in your configuration file.");
                return connectionString;
            }
        }

        /// <summary>
        /// Stores the connection string in user secrets for the data project.
        /// </summary>
        /// <param name="connectionString">The connection string to store.</param>
        /// <param name="dataFolder">The data project folder path.</param>
        private async Task StoreConnectionStringInUserSecretsAsync(string connectionString, string dataFolder)
        {
            var projectFiles = Directory.GetFiles(dataFolder, "*.csproj");
            if (projectFiles.Length == 0)
            {
                throw new InvalidOperationException($"No .csproj file found in {dataFolder}");
            }

            var projectFile = projectFiles[0];
            
            // Check if user secrets are already initialized
            var userSecretsId = ExtractUserSecretsId(projectFile);

            if (string.IsNullOrWhiteSpace(userSecretsId))
            {
                // Initialize user secrets
                userSecretsId = Guid.NewGuid().ToString();
                await InitializeUserSecretsAsync(projectFile, userSecretsId);
            }

            // Store the connection string in user secrets
            var secretKey = $"ConnectionStrings:{ContextName}Connection";
            await SetUserSecretAsync(userSecretsId, secretKey, connectionString, dataFolder);
        }

        /// <summary>
        /// Extracts the UserSecretsId from a project file using MSBuild evaluation.
        /// This will properly evaluate the project with all imports including Directory.Build.props.
        /// </summary>
        /// <param name="projectFilePath">The path to the project file.</param>
        /// <returns>The UserSecretsId if found, otherwise null.</returns>
        private static string ExtractUserSecretsId(string projectFilePath)
        {
            try
            {
                // Register MSBuild if not already registered
                MSBuildProjectManager.EnsureMSBuildRegistered();
                
                // Use MSBuild APIs to properly evaluate the project with all imports (including Directory.Build.props)
                var project = new Project(projectFilePath);
                var userSecretsId = project.GetPropertyValue("UserSecretsId");
                
                // Clean up the project to avoid memory leaks
                ProjectCollection.GlobalProjectCollection.UnloadProject(project);
                
                return string.IsNullOrWhiteSpace(userSecretsId) ? null : userSecretsId;
            }
            catch
            {
                // If MSBuild evaluation fails, return null to indicate no UserSecretsId found
                return null;
            }
        }

        /// <summary>
        /// Initializes user secrets for a project by adding UserSecretsId to the project file.
        /// </summary>
        /// <param name="projectFilePath">The path to the project file.</param>
        /// <param name="userSecretsId">The user secrets ID to add.</param>
        private static async Task InitializeUserSecretsAsync(string projectFilePath, string userSecretsId)
        {
            var projectContent = await File.ReadAllTextAsync(projectFilePath);
            
            // Find the first PropertyGroup and add UserSecretsId
            var propertyGroupStart = projectContent.IndexOf("<PropertyGroup>");
            if (propertyGroupStart == -1)
            {
                throw new InvalidOperationException("Could not find PropertyGroup in project file to add UserSecretsId.");
            }

            var propertyGroupEnd = projectContent.IndexOf("</PropertyGroup>", propertyGroupStart);
            if (propertyGroupEnd == -1)
            {
                throw new InvalidOperationException("Could not find end of PropertyGroup in project file.");
            }

            var userSecretsElement = $"    <UserSecretsId>{userSecretsId}</UserSecretsId>{Environment.NewLine}  ";
            var insertPosition = propertyGroupEnd;
            
            var newContent = projectContent.Insert(insertPosition, userSecretsElement);
            await File.WriteAllTextAsync(projectFilePath, newContent);
        }

        /// <summary>
        /// Sets a user secret value using the official dotnet user-secrets CLI tool.
        /// </summary>
        /// <param name="userSecretsId">The user secrets ID.</param>
        /// <param name="key">The secret key.</param>
        /// <param name="value">The secret value.</param>
        /// <param name="projectPath">The project directory path.</param>
        private static async Task SetUserSecretAsync(string userSecretsId, string key, string value, string projectPath)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"user-secrets set \"{key}\" \"{value}\"",
                WorkingDirectory = projectPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo)
                ?? throw new InvalidOperationException("Failed to start dotnet user-secrets process.");
            await process.WaitForExitAsync();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Failed to set user secret '{key}'. Exit code: {process.ExitCode}. " +
                    $"Error: {error}. Output: {output}"
                );
            }
        }


        #endregion

    }

}
