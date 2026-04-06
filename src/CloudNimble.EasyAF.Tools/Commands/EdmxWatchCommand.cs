using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tools.Commands
{

    /// <summary>
    /// Command to watch EDMX files in your Data project for changes and regenerate the framework.
    /// </summary>
    /// <remarks>
    /// This command monitors the Data project for changes to EDMX files and triggers regeneration logic
    /// when changes are detected. It is useful for development workflows where EDMX files are updated frequently.
    /// </remarks>
    /// <example>
    /// <code>
    /// dotnet easyaf edmx watch --path "C:\MySolution"
    /// </code>
    /// </example>
    [Command(
        Name = "watch",
        Description = "Watch EDMX files in your Data project for changes and regenerate the Framework."
    )]
    public class EdmxWatchCommand
    {

        #region Properties

        /// <summary>
        /// Gets or sets the working directory for the code compiler. Defaults to current directory.
        /// </summary>
        [Option("-path <path>", Description = "Working directory for the code compiler. Defaults to current directory.")]
        public string Root { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the EDMX watch command, monitoring for file changes.
        /// </summary>
        /// <returns>0 when completed.</returns>
        /// <example>
        /// <code>
        /// dotnet easyaf edmx watch --path "C:\MySolution"
        /// </code>
        /// </example>
        public Task<int> OnExecuteAsync()
        {
            var rootFolder = !string.IsNullOrWhiteSpace(Root)
                ? Root
                : Directory.GetCurrentDirectory();

            ArgumentException.ThrowIfNullOrWhiteSpace(rootFolder, nameof(Root));

            var dataFolder = EdmxRootCommand.FindDataFolder(rootFolder);

            if (string.IsNullOrWhiteSpace(dataFolder))
            {
                Console.WriteLine($"The data folder could not be found in {rootFolder}.\nExiting, sorry about that.");

                return Task.FromResult(1);
            }

            using (var watcher = new FileSystemWatcher())
            {
                watcher.Path = dataFolder;

                watcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;

                watcher.Filter = "*.edmx";

                watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                watcher.Deleted += OnChanged;
                watcher.Renamed += OnChanged;

                watcher.EnableRaisingEvents = true;

                Console.WriteLine("Press 'q' to stop watching for EDMX file changes.");
                while (Console.Read() != 'q')
                {
                    // Wait for user to quit
                }
            }

            Console.WriteLine("EasyAF code generation has completed.");

            return Task.FromResult(0);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles file system change events for EDMX files.
        /// </summary>
        /// <param name="source">The event source.</param>
        /// <param name="e">The file system event arguments.</param>
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"File: {e.FullPath} changed. Regenerating EasyAF framework...");

            var dataDirectory = Path.GetDirectoryName(e.FullPath);
            CodeGenerateCommand.Generate(Directory.GetParent(dataDirectory).FullName, "all", @"Controllers\\Public\\");

            Console.WriteLine("Finished.");
        }

        #endregion

    }

}
