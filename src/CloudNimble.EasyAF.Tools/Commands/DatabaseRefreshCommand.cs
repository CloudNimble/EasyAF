using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.Tools.Commands.Root;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tools.Commands
{

    /// <summary>
    /// Command for refreshing existing EDMX files.
    /// </summary>
    [Command(Name = "refresh", Description = "Refresh existing EDMX files from the database. If no context name is specified, processes all .edmx files found.")]
    public class DatabaseRefreshCommand
    {

        #region Fields

        private readonly EdmxConverter _converter;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DbContext class name to use for finding the EDMX and configuration files.
        /// When not specified, all .edmx files will be processed.
        /// </summary>
        [Option("-x|--context-name", Description = "DbContext class name (used to locate {ContextName}.edmx and {ContextName}.edmx.config files). If not specified, all .edmx files will be processed.")]
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
        /// Initializes a new instance of the <see cref="DatabaseRefreshCommand"/> class.
        /// </summary>
        /// <param name="converter">The EDMX converter service.</param>
        public DatabaseRefreshCommand(EdmxConverter converter)
        {
            ArgumentNullException.ThrowIfNull(converter);
            _converter = converter;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the refresh command.
        /// </summary>
        /// <returns>Exit code.</returns>
        public async Task<int> OnExecuteAsync()
        {
            try
            {
                Console.WriteLine("Refreshing EDMX from database...");

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
                    return await ProcessAllEdmxFilesAsync(projectPath);
                }
                else
                {
                    return await ProcessSingleContextAsync(ContextName, projectPath);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error refreshing EDMX: {ex.Message}");
                Console.Error.WriteLine("Please check the database connection and the EDMX file path.");
                return 1;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Processes all .edmx files found in the project directory.
        /// </summary>
        /// <param name="projectPath">The project directory path.</param>
        /// <returns>Exit code.</returns>
        private async Task<int> ProcessAllEdmxFilesAsync(string projectPath)
        {
            var edmxFiles = Directory.GetFiles(projectPath, "*.edmx", SearchOption.TopDirectoryOnly);
            
            if (edmxFiles.Length == 0)
            {
                Console.Error.WriteLine($"Error: No .edmx files found in: {projectPath}");
                Console.Error.WriteLine("Please run 'easyaf database generate' first to create EDMX files.");
                return 1;
            }

            Console.WriteLine($"Found {edmxFiles.Length} EDMX file(s). Processing all...");

            var successCount = 0;
            var failureCount = 0;

            foreach (var edmxFile in edmxFiles.OrderBy(f => f))
            {
                var contextName = Path.GetFileNameWithoutExtension(edmxFile);
                Console.WriteLine($"\nProcessing {contextName}...");

                try
                {
                    await ProcessSingleEdmxFileAsync(edmxFile, contextName, projectPath);
                    successCount++;
                    Console.WriteLine($"✓ Successfully refreshed {contextName}.edmx");
                }
                catch (Exception ex)
                {
                    failureCount++;
                    Console.Error.WriteLine($"✗ Failed to refresh {contextName}.edmx: {ex.Message}");
                }
            }

            Console.WriteLine($"\nCompleted processing {edmxFiles.Length} EDMX file(s).");
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
            var edmxFileName = $"{contextName}.edmx";
            var edmxPath = Path.Combine(projectPath, edmxFileName);

            if (!File.Exists(edmxPath))
            {
                Console.Error.WriteLine($"Error: EDMX file not found: {edmxPath}");
                Console.Error.WriteLine($"Please run 'easyaf database generate' first to create the EDMX file.");
                return 1;
            }

            if (!_converter.HasConfig(edmxPath))
            {
                Console.Error.WriteLine($"Error: No configuration found for EDMX file: {edmxPath}");
                Console.Error.WriteLine("This EDMX file was not generated from a database or the .edmx.config file is missing.");
                return 1;
            }

            await ProcessSingleEdmxFileAsync(edmxPath, contextName, projectPath);
            Console.WriteLine($"EDMX file refreshed successfully for {contextName}");
            return 0;
        }

        /// <summary>
        /// Processes a single EDMX file.
        /// </summary>
        /// <param name="edmxPath">The path to the EDMX file.</param>
        /// <param name="contextName">The context name.</param>
        /// <param name="projectPath">The project directory path.</param>
        private async Task ProcessSingleEdmxFileAsync(string edmxPath, string contextName, string projectPath)
        {
            if (!_converter.HasConfig(edmxPath))
            {
                throw new InvalidOperationException($"No configuration found for EDMX file: {edmxPath}. This EDMX file was not generated from a database or the .edmx.config file is missing.");
            }

            var (EdmxContent, OnModelCreatingBody) = await _converter.RefreshFromDatabaseAsync(edmxPath, projectPath);
            await File.WriteAllTextAsync(edmxPath, EdmxContent);
        }

        #endregion

    }

}
