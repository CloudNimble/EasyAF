using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tools.Commands
{

    /// <summary>
    /// Command for setting up local development environment for existing EasyAF projects.
    /// </summary>
    [Command(Name = "setup", Description = "Set up local development environment for existing EasyAF project")]
    public class SetupCommand : EasyAFBaseCommand
    {

        #region Fields

        private readonly EdmxConfigManager _configManager;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the connection string to store locally.
        /// </summary>
        [Option("-c|--connection-string", Description = "Local connection string to store in user secrets")]
        [Required]
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the DbContext class name to configure.
        /// </summary>
        [Option("-x|--context-name", Description = "DbContext class name (if multiple contexts exist)")]
        public string ContextName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show what would be configured without making changes.
        /// </summary>
        [Option("--dry-run", Description = "Show what would be configured without making changes")]
        public bool DryRun { get; set; }

        /// <summary>
        /// Gets or sets the working directory for the solution. Defaults to current directory.
        /// </summary>
        [Option("-s|--solution-folder", Description = "Solution directory (defaults to current directory)")]
        public string SolutionFolder { get; set; } = Directory.GetCurrentDirectory();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupCommand"/> class.
        /// </summary>
        /// <param name="configManager">The configuration manager service.</param>
        public SetupCommand(EdmxConfigManager configManager)
        {
            ArgumentNullException.ThrowIfNull(configManager);
            _configManager = configManager;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the setup command.
        /// </summary>
        /// <returns>Exit code.</returns>
        public async Task<int> OnExecuteAsync()
        {
            try
            {
                Console.WriteLine("Setting up local development environment for existing EasyAF project...");

                // Discover existing EDMX configuration files
                var configFiles = DiscoverEdmxConfigFiles();
                if (configFiles.Length == 0)
                {
                    Console.Error.WriteLine("Error: No *.edmx.config files found in the solution.");
                    Console.Error.WriteLine("This command is for setting up existing EasyAF projects. Use 'dotnet easyaf init' to initialize a new project.");
                    return 1;
                }

                // Determine which context to configure
                var selectedConfig = await SelectContextConfigAsync(configFiles);
                if (selectedConfig == null)
                {
                    return 1;
                }

                Console.WriteLine($"Using configuration: {Path.GetFileName(selectedConfig.ConfigPath)}");
                Console.WriteLine($"Context: {selectedConfig.Config.ContextName}");

                // Parse the connection string source
                var connectionStringSource = selectedConfig.Config.ConnectionStringSource;
                if (string.IsNullOrWhiteSpace(connectionStringSource))
                {
                    Console.Error.WriteLine("Error: No connection string source found in the configuration file.");
                    return 1;
                }

                Console.WriteLine($"Connection string source: {connectionStringSource}");

                // Check if this references user secrets
                if (!IsUserSecretsReference(connectionStringSource))
                {
                    Console.WriteLine("Connection string source does not reference user secrets.");
                    Console.WriteLine("No local setup required - the connection string is already configured externally.");
                    return 0;
                }

                // Extract the secret key from the source
                var secretKey = ExtractSecretKey(connectionStringSource);
                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    Console.Error.WriteLine("Error: Could not extract secret key from connection string source.");
                    return 1;
                }

                Console.WriteLine($"Secret key: {secretKey}");

                // Get the UserSecretsId from Directory.Build.props
                var userSecretsId = ExtractUserSecretsIdFromDirectoryBuildProps();
                if (string.IsNullOrWhiteSpace(userSecretsId))
                {
                    Console.Error.WriteLine("Error: No UserSecretsId found in Directory.Build.props.");
                    Console.Error.WriteLine("The project may not be properly initialized with EasyAF. Try running 'dotnet easyaf init' first.");
                    return 1;
                }

                Console.WriteLine($"UserSecretsId: {userSecretsId}");

                if (DryRun)
                {
                    Console.WriteLine();
                    Console.WriteLine("DRY RUN - No changes will be made");
                    Console.WriteLine($"Would store connection string in user secrets:");
                    Console.WriteLine($"  UserSecretsId: {userSecretsId}");
                    Console.WriteLine($"  Key: {secretKey}");
                    Console.WriteLine($"  Value: {ConnectionString}");
                    return 0;
                }

                // Store the connection string in user secrets
                var dataFolder = Path.GetDirectoryName(selectedConfig.ConfigPath);
                await SetUserSecretAsync(userSecretsId, secretKey, ConnectionString, dataFolder);

                Console.WriteLine();
                Console.WriteLine("Local development environment setup completed successfully!");
                Console.WriteLine($"Connection string stored in user secrets with key: {secretKey}");
                Console.WriteLine("You can now run database operations and generate EDMX files.");

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error setting up local environment: {ex.Message}");
                return 1;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Discovers all *.edmx.config files in the solution.
        /// </summary>
        /// <returns>Array of paths to EDMX configuration files.</returns>
        private string[] DiscoverEdmxConfigFiles()
        {
            return Directory.GetFiles(SolutionFolder, "*.edmx.config", SearchOption.AllDirectories);
        }

        /// <summary>
        /// Represents an EDMX configuration file and its parsed content.
        /// </summary>
        private class EdmxConfigInfo
        {
            public string ConfigPath { get; set; }
            public EdmxConfig Config { get; set; }
        }

        /// <summary>
        /// Selects which context configuration to use based on user input or automatic detection.
        /// </summary>
        /// <param name="configFiles">Array of configuration file paths.</param>
        /// <returns>The selected configuration info, or null if selection failed.</returns>
        private async Task<EdmxConfigInfo> SelectContextConfigAsync(string[] configFiles)
        {
            var configInfos = new List<EdmxConfigInfo>();

            // Load all configuration files
            foreach (var configFile in configFiles)
            {
                try
                {
                    var config = await _configManager.LoadConfigAsync(configFile);
                    configInfos.Add(new EdmxConfigInfo
                    {
                        ConfigPath = configFile,
                        Config = config
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not load configuration from {Path.GetFileName(configFile)}: {ex.Message}");
                }
            }

            if (configInfos.Count == 0)
            {
                Console.Error.WriteLine("Error: No valid EDMX configuration files could be loaded.");
                return null;
            }

            // If context name is specified, find matching configuration
            if (!string.IsNullOrWhiteSpace(ContextName))
            {
                var matchingConfig = configInfos.FirstOrDefault(c => 
                    c.Config.ContextName.Equals(ContextName, StringComparison.OrdinalIgnoreCase));

                if (matchingConfig == null)
                {
                    Console.Error.WriteLine($"Error: No configuration found for context '{ContextName}'.");
                    Console.Error.WriteLine("Available contexts:");
                    foreach (var info in configInfos)
                    {
                        Console.Error.WriteLine($"  - {info.Config.ContextName} ({Path.GetFileName(info.ConfigPath)})");
                    }
                    return null;
                }

                return matchingConfig;
            }

            // If only one configuration exists, use it
            if (configInfos.Count == 1)
            {
                return configInfos[0];
            }

            // Multiple configurations exist - user must specify which one
            Console.Error.WriteLine("Error: Multiple EDMX configurations found. Please specify which context to configure using --context-name.");
            Console.Error.WriteLine("Available contexts:");
            foreach (var info in configInfos)
            {
                Console.Error.WriteLine($"  - {info.Config.ContextName} ({Path.GetFileName(info.ConfigPath)})");
            }

            return null;
        }

        /// <summary>
        /// Checks if a connection string source references user secrets.
        /// </summary>
        /// <param name="connectionStringSource">The connection string source to check.</param>
        /// <returns>True if it references user secrets, false otherwise.</returns>
        private static bool IsUserSecretsReference(string connectionStringSource)
        {
            return connectionStringSource.StartsWith("secrets:", StringComparison.OrdinalIgnoreCase) ||
                   connectionStringSource.StartsWith("user-secrets:", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Extracts the secret key from a user secrets reference.
        /// </summary>
        /// <param name="connectionStringSource">The connection string source (e.g., "secrets:ConnectionStrings:MyAppConnection").</param>
        /// <returns>The extracted secret key, or null if extraction failed.</returns>
        private static string ExtractSecretKey(string connectionStringSource)
        {
            // Handle both "secrets:" and "user-secrets:" prefixes
            var prefix = connectionStringSource.StartsWith("secrets:", StringComparison.OrdinalIgnoreCase) 
                ? "secrets:" 
                : "user-secrets:";

            if (!connectionStringSource.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Extract everything after the prefix
            var secretKey = connectionStringSource.Substring(prefix.Length);
            return string.IsNullOrWhiteSpace(secretKey) ? null : secretKey;
        }

        #endregion

    }

}