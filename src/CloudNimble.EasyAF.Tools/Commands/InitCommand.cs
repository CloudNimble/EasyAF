using CloudNimble.EasyAF.EFCoreToEdmx;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMasterAllowedValues = McMaster.Extensions.CommandLineUtils.AllowedValuesAttribute;

namespace CloudNimble.EasyAF.Tools.Commands
{

    /// <summary>
    /// Command for initializing EasyAF project configuration including database scaffolding, project types, and analyzer setup.
    /// </summary>
    [Command(Name = "init", Description = "Initialize EasyAF project configuration")]
    public class InitCommand : EasyAFBaseCommand
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

        /// <summary>
        /// Gets or sets the SimpleMessageBus project name to create. If specified, creates a new SimpleMessageBus project.
        /// </summary>
        [Option("--simplemessagebus-project", Description = "Optional SimpleMessageBus project name to create (e.g., 'MyApp.EventBus' or 'MyApp.SimpleMessageBus')")]
        public string SimpleMessageBusProject { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InitCommand"/> class.
        /// </summary>
        /// <param name="configManager">The configuration manager service.</param>
        public InitCommand(EdmxConfigManager configManager)
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
                
                // Ensure MSBuild is registered before doing anything
                CheckMSBuildRegistered();

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
                var (connectionStringSource, userSecretsId) = await ProcessConnectionStringAsync(ConnectionString, dataFolder);

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
                
                // Configure project types for all projects
                Console.WriteLine();
                Console.WriteLine("Configuring EasyAF project types...");
                ConfigureProjectTypes(userSecretsId);
                
                // Create SimpleMessageBus project if requested
                if (!string.IsNullOrWhiteSpace(SimpleMessageBusProject))
                {
                    Console.WriteLine();
                    Console.WriteLine($"Creating SimpleMessageBus project: {SimpleMessageBusProject}");
                    await CreateSimpleMessageBusProjectAsync(SimpleMessageBusProject, userSecretsId);
                }
                
                Console.WriteLine();
                Console.WriteLine("EasyAF initialization completed successfully!");
                Console.WriteLine("You can now generate EDMX files using:");
                Console.WriteLine($"dotnet easyaf database generate --context-name \"{ContextName}\"");
                
                if (!string.IsNullOrWhiteSpace(SimpleMessageBusProject))
                {
                    Console.WriteLine("You can also generate SimpleMessageBus files using:");
                    Console.WriteLine("dotnet easyaf code generate");
                }

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
        /// <returns>A tuple containing the connection string source and the UserSecretsId (if applicable).</returns>
        private async Task<(string connectionStringSource, string userSecretsId)> ProcessConnectionStringAsync(string connectionString, string dataFolder)
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
                    // For source references, we might still need to generate a UserSecretsId for Directory.Build.props
                    var existingUserSecretsId = ExtractUserSecretsIdFromDataProject(dataFolder);
                    return (connectionString, existingUserSecretsId ?? Guid.NewGuid().ToString());
                }
            }

            // It's an actual connection string - store it in user secrets
            Console.WriteLine("Detected actual connection string. Storing securely in user secrets...");
            
            try
            {
                var userSecretsId = await StoreConnectionStringInUserSecretsAsync(connectionString, dataFolder);
                var userSecretsSource = $"secrets:ConnectionStrings:{ContextName}Connection";
                Console.WriteLine($"Connection string stored in user secrets as: {userSecretsSource}");
                return (userSecretsSource, userSecretsId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not store connection string in user secrets: {ex.Message}");
                Console.WriteLine("Falling back to using the connection string source as-is.");
                Console.WriteLine("Note: This may expose sensitive connection information in your configuration file.");
                var fallbackUserSecretsId = ExtractUserSecretsIdFromDataProject(dataFolder) ?? Guid.NewGuid().ToString();
                return (connectionString, fallbackUserSecretsId);
            }
        }

        /// <summary>
        /// Stores the connection string in user secrets for the data project.
        /// </summary>
        /// <param name="connectionString">The connection string to store.</param>
        /// <param name="dataFolder">The data project folder path.</param>
        /// <returns>The UserSecretsId that was used.</returns>
        private async Task<string> StoreConnectionStringInUserSecretsAsync(string connectionString, string dataFolder)
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
                // Generate new user secrets ID (will be set in Directory.Build.props)
                userSecretsId = Guid.NewGuid().ToString();
            }

            // Store the connection string in user secrets
            var secretKey = $"ConnectionStrings:{ContextName}Connection";
            await SetUserSecretAsync(userSecretsId, secretKey, connectionString, dataFolder);
            
            return userSecretsId;
        }

        /// <summary>
        /// Creates a new SimpleMessageBus project with the specified name.
        /// </summary>
        /// <param name="projectName">The name of the SimpleMessageBus project to create.</param>
        /// <param name="userSecretsId">The UserSecretsId for the solution.</param>
        private async Task CreateSimpleMessageBusProjectAsync(string projectName, string userSecretsId)
        {
            try
            {
                var projectPath = Path.Combine(SolutionFolder, projectName);
                
                // Create project directory if it doesn't exist
                if (!Directory.Exists(projectPath))
                {
                    Directory.CreateDirectory(projectPath);
                    Console.WriteLine($"Created project directory: {projectPath}");
                }
                
                var projectFilePath = Path.Combine(projectPath, $"{projectName}.csproj");
                
                // Only create if project file doesn't already exist
                if (!File.Exists(projectFilePath))
                {
                    // Create a basic class library project file
                    var projectContent = CreateSimpleMessageBusProjectContent(projectName);
                    await File.WriteAllTextAsync(projectFilePath, projectContent);
                    Console.WriteLine($"Created project file: {projectFilePath}");
                    
                    // Set the EasyAFProjectType property
                    SetProjectType(projectFilePath, "SimpleMessageBus");
                    
                    // Add to solution if one exists
                    await AddProjectToSolutionAsync(projectName, projectFilePath);
                }
                else
                {
                    Console.WriteLine($"Project file already exists: {projectFilePath}");
                    // Still set the project type in case it's missing
                    SetProjectType(projectFilePath, "SimpleMessageBus");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to create SimpleMessageBus project: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates the content for a SimpleMessageBus project file.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <returns>The project file content as XML string.</returns>
        private static string CreateSimpleMessageBusProjectContent(string projectName)
        {
            return 
                $"""
                <Project Sdk="Microsoft.NET.Sdk">

                    <PropertyGroup>
                        <TargetFramework>net8.0</TargetFramework>
                        <RootNamespace>{projectName}</RootNamespace>
                        <EasyAFProjectType>SimpleMessageBus</EasyAFProjectType>
                    </PropertyGroup>

                    <ItemGroup>
                        <PackageReference Include="CloudNimble.SimpleMessageBus.Core" Version="6.*" />
                    </ItemGroup>

                </Project>
                """;
        }

        /// <summary>
        /// Attempts to add the created project to an existing solution file.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="projectFilePath">The path to the project file.</param>
        private async Task AddProjectToSolutionAsync(string projectName, string projectFilePath)
        {
            try
            {
                // Look for solution files in the solution folder
                var solutionFiles = Directory.GetFiles(SolutionFolder, "*.sln");
                if (solutionFiles.Length > 0)
                {
                    var solutionFile = solutionFiles[0]; // Use the first solution file found
                    var relativePath = Path.GetRelativePath(SolutionFolder, projectFilePath);
                    
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"sln \"{solutionFile}\" add \"{relativePath}\"",
                        WorkingDirectory = SolutionFolder,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(processStartInfo);
                    if (process is not null)
                    {
                        await process.WaitForExitAsync();
                        if (process.ExitCode == 0)
                        {
                            Console.WriteLine($"Added project to solution: {Path.GetFileName(solutionFile)}");
                        }
                        else
                        {
                            var error = await process.StandardError.ReadToEndAsync();
                            Console.WriteLine($"Warning: Could not add project to solution: {error}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not add project to solution: {ex.Message}");
            }
        }

        #endregion

    }

}
