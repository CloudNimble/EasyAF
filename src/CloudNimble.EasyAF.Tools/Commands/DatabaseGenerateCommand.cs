using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.MSBuild;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tools.Commands
{

    /// <summary>
    /// Command for generating EDMX from database.
    /// </summary>
    [Command(Name = "generate", Description = "Generate EDMX files from database using existing configuration. If no context name is specified, processes all .edmx.config files found.")]
    public class DatabaseGenerateCommand
    {

        #region Fields

        private readonly EdmxConverter _converter;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DbContext class name to use for finding the configuration file.
        /// When not specified, all .edmx.config files will be processed.
        /// </summary>
        [Option("-x|--context-name", Description = "DbContext class name (used to locate {ContextName}.edmx.config file). If not specified, all .edmx.config files will be processed.")]
        public string ContextName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the project directory path (defaults to auto-detected .Data folder).
        /// </summary>
        [Option("-p|--project", Description = "Path to the project directory (defaults to auto-detected .Data folder)")]
        public string Project { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the working directory for the solution. Defaults to current directory.
        /// </summary>
        [Option("-s|--solution-folder", Description = "Solution directory (defaults to current directory)")]
        public string SolutionFolder { get; set; } = Directory.GetCurrentDirectory();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseGenerateCommand"/> class.
        /// </summary>
        /// <param name="converter">The EDMX converter service.</param>
        public DatabaseGenerateCommand(EdmxConverter converter)
        {
            ArgumentNullException.ThrowIfNull(converter);
            _converter = converter;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the generate command.
        /// </summary>
        /// <returns>Exit code.</returns>
        public async Task<int> OnExecuteAsync()
        {
            try
            {
                Console.WriteLine("Generating EDMX from database...");
                
                // Ensure MSBuild is registered before any operations that might use it
                MSBuildProjectManager.EnsureMSBuildRegistered();

                // Find the .Data folder if project path not explicitly provided
                var projectPath = Project;
                if (string.IsNullOrWhiteSpace(projectPath))
                {
                    projectPath = EdmxRootCommand.FindDataFolder(SolutionFolder);
                    if (string.IsNullOrWhiteSpace(projectPath))
                    {
                        Console.Error.WriteLine($"Error: Could not find a project ending in '.Data' in the solution directory: {SolutionFolder}");
                        Console.Error.WriteLine("Please ensure you have a .Data project in your solution or specify the correct solution directory with --solution-folder.");
                        return 1;
                    }
                }

                Console.WriteLine($"Using project: {projectPath}");

                if (string.IsNullOrWhiteSpace(ContextName))
                {
                    return await ProcessAllConfigFilesAsync(projectPath);
                }
                else
                {
                    return await ProcessSingleContextAsync(ContextName, projectPath);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error generating EDMX: {ex.Message}");
                return 1;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Processes all .edmx.config files found in the project directory.
        /// </summary>
        /// <param name="projectPath">The project directory path.</param>
        /// <returns>Exit code.</returns>
        private async Task<int> ProcessAllConfigFilesAsync(string projectPath)
        {
            var configFiles = Directory.GetFiles(projectPath, "*.edmx.config", SearchOption.TopDirectoryOnly);
            
            if (configFiles.Length == 0)
            {
                Console.Error.WriteLine($"Error: No .edmx.config files found in: {projectPath}");
                Console.Error.WriteLine("Please run 'easyaf database init' first to create configuration files.");
                return 1;
            }

            Console.WriteLine($"Found {configFiles.Length} configuration file(s). Processing all...");

            var successCount = 0;
            var failureCount = 0;

            foreach (var configFile in configFiles.OrderBy(f => f))
            {
                var contextName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(configFile));
                Console.WriteLine($"\nProcessing {contextName}...");

                try
                {
                    await ProcessSingleConfigFileAsync(configFile, contextName, projectPath);
                    successCount++;
                    Console.WriteLine($"✓ Successfully generated {contextName}.edmx");
                }
                catch (Exception ex)
                {
                    failureCount++;
                    Console.Error.WriteLine($"✗ Failed to generate {contextName}.edmx: {ex.Message}");
                }
            }

            Console.WriteLine($"\nCompleted processing {configFiles.Length} configuration file(s).");
            Console.WriteLine($"Successful: {successCount}, Failed: {failureCount}");

            return failureCount > 0 ? 1 : 0;
        }

        /// <summary>
        /// Processes a single context by name.
        /// </summary>
        /// <param name="contextName">The context name to process.</param>
        /// <param name="projectPath">The project directory path.</param>
        /// <returns>Exit code.</returns>
        private async Task<int> ProcessSingleContextAsync(string contextName, string projectPath)
        {
            var configFileName = $"{contextName}.edmx.config";
            var configPath = Path.Combine(projectPath, configFileName);

            if (!File.Exists(configPath))
            {
                Console.Error.WriteLine($"Error: Configuration file not found: {configPath}");
                Console.Error.WriteLine($"Please run 'easyaf database init' first to create the configuration file.");
                return 1;
            }

            await ProcessSingleConfigFileAsync(configPath, contextName, projectPath);
            Console.WriteLine($"EDMX file generated successfully for {contextName}");
            return 0;
        }

        /// <summary>
        /// Processes a single configuration file.
        /// </summary>
        /// <param name="configPath">The path to the configuration file.</param>
        /// <param name="contextName">The context name.</param>
        /// <param name="projectPath">The project directory path.</param>
        private async Task ProcessSingleConfigFileAsync(string configPath, string contextName, string projectPath)
        {
            var edmxFileName = $"{contextName}.edmx";
            var edmxPath = Path.Combine(projectPath, edmxFileName);

            var (EdmxContent, OnModelCreatingBody) = await _converter.ConvertFromDatabaseAsync(configPath, projectPath).ConfigureAwait(false);
            await File.WriteAllTextAsync(edmxPath, EdmxContent);
        }

        #endregion

    }

}
