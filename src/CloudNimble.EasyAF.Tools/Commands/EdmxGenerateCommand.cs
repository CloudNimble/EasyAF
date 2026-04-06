using CloudNimble.EasyAF.EFCoreToEdmx;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tools.Commands
{

    /// <summary>
    /// Command to generate an EDMX file from an EF Core DbContext in the Data project.
    /// </summary>
    /// <remarks>
    /// This command locates the Data project, finds the compiled assembly, and generates an EDMX file
    /// using the <see cref="EdmxConverter"/>. The output file is placed in the Data project directory.
    /// </remarks>
    /// <example>
    /// <code>
    /// dotnet easyaf edmx generate --path "C:\MySolution"
    /// </code>
    /// </example>
    [Command(
        Name = "generate",
        Description = "Generate an EDMX file from an EF Core DbContext in the Data project."
    )]
    public class EdmxGenerateCommand
    {

        #region Private Fields

        private readonly EdmxConverter _converter;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the working directory for the code compiler. Defaults to current directory.
        /// </summary>
        [Option("-path <path>", Description = "Working directory for the code compiler. Defaults to current directory.")]
        public string Root { get; set; }

        /// <summary>
        /// Gets or sets the DbContext class to use.
        /// </summary>
        [Option("--context <DbContext>", Description = "The DbContext class to use.")]
        public string Context { get; set; }

        /// <summary>
        /// Gets or sets the environment to use (Development, Production, etc).
        /// </summary>
        [Option("--environment <ENV>", Description = "The environment to use (Development, Production, etc).")]
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets the project folder containing the DbContext.
        /// </summary>
        [Option("--project <PATH>", Description = "The project folder containing the DbContext.")]
        public string Project { get; set; }

        /// <summary>
        /// Gets or sets the startup project folder.
        /// </summary>
        [Option("--startup-project <PATH>", Description = "The startup project folder.")]
        public string StartupProject { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmxGenerateCommand"/> class.
        /// </summary>
        /// <param name="converter">The EDMX converter service.</param>
        public EdmxGenerateCommand(EdmxConverter converter)
        {
            ArgumentNullException.ThrowIfNull(converter);
            _converter = converter;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the EDMX generation command.
        /// </summary>
        /// <returns>0 if successful, 1 if an error occurred.</returns>
        /// <example>
        /// <code>
        /// dotnet easyaf edmx generate --path "C:\MySolution"
        /// </code>
        /// </example>
        public async Task<int> OnExecuteAsync()
        {
            var rootFolder = !string.IsNullOrWhiteSpace(Root)
                ? Root
                : Directory.GetCurrentDirectory();

            ArgumentException.ThrowIfNullOrWhiteSpace(rootFolder, nameof(Root));

            var dataFolder = EdmxRootCommand.FindDataFolder(rootFolder);

            if (string.IsNullOrWhiteSpace(dataFolder))
            {
                Console.WriteLine($"The data folder could not be found in {rootFolder}.\nExiting, sorry about that.");

                return 1;
            }

            var projectPath = !string.IsNullOrWhiteSpace(Project)
                ? Project
                : dataFolder;

            var startupPath = !string.IsNullOrWhiteSpace(StartupProject)
                ? StartupProject
                : projectPath;

            var binPath = Path.Combine(projectPath, "bin");

            if (!Directory.Exists(binPath))
            {
                Console.WriteLine($"Build output not found at {binPath}. Please build your project first.");

                return 1;
            }

            var result = _converter.ConvertToEdmx(binPath);

            var edmxFileName = $"{result.DbContextName}.edmx";
            var edmxFilePath = Path.Combine(dataFolder, edmxFileName);

            await File.WriteAllTextAsync(edmxFilePath, result.EdmxContent);

            Console.WriteLine($"EDMX file generated: {edmxFilePath}");
            
            return 0;
        }

        #endregion

    }

}
