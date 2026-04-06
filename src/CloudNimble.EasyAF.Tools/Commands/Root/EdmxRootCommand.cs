using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;

namespace CloudNimble.EasyAF.Tools.Commands
{

    /// <summary>
    /// Root command for EDMX file utilities.
    /// </summary>
    /// <remarks>
    /// This command serves as the entry point for all EDMX-related subcommands, such as generate, swap, and watch.
    /// It provides shared utility methods for locating project folders and EDMX files.
    /// </remarks>
    /// <example>
    /// <code>
    /// dotnet easyaf edmx --help
    /// </code>
    /// </example>
    [Command(Name = "edmx", Description = "EasyAF EDMX commands.")]
    [Subcommand(typeof(EdmxGenerateCommand), typeof(EdmxSwapCommand), typeof(EdmxWatchCommand))]
    public class EdmxRootCommand
    {

        /// <summary>
        /// Shows help for the edmx command.
        /// </summary>
        /// <param name="app">The command line application.</param>
        /// <returns>Exit code 1.</returns>
        public int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();

            return 1;
        }

        /// <summary>
        /// Attempts to find the .Data folder in the given root directory.
        /// </summary>
        /// <param name="rootFolder">The root directory to search.</param>
        /// <returns>The path to the .Data folder, or <c>null</c> if not found.</returns>
        /// <example>
        /// <code>
        /// var dataFolder = EdmxRootCommand.FindDataFolder("C:\\MySolution");
        /// </code>
        /// </example>
        public static string FindDataFolder(string rootFolder)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(rootFolder, nameof(rootFolder));

            var projects = Directory.GetDirectories(rootFolder);
            var notTests = projects.Where(c => !c.ToLower().Contains(".tests.")).OrderBy(c => c.Length);
            var dataFolder = notTests.FirstOrDefault(c => c.EndsWith(".Data"));

            return dataFolder;
        }

        /// <summary>
        /// Attempts to find the first EDMX file in the given folder.
        /// </summary>
        /// <param name="folder">The folder to search for EDMX files.</param>
        /// <returns>The path to the first EDMX file found, or <c>null</c> if none found.</returns>
        /// <example>
        /// <code>
        /// var edmxFile = EdmxRootCommand.FindEdmxFile("C:\\MySolution\\MyProject.Data");
        /// </code>
        /// </example>
        public static string FindEdmxFile(string folder)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(folder, nameof(folder));

            var files = Directory.GetFiles(folder, "*.edmx", SearchOption.TopDirectoryOnly);
            return files.FirstOrDefault();
        }

    }

}
