using McMaster.Extensions.CommandLineUtils;

namespace CloudNimble.EasyAF.Tools.Commands.Root
{

    /// <summary>
    /// Command-line interface for generating EDMX files from databases.
    /// </summary>
    /// <remarks>
    /// This class provides CLI commands for database scaffolding and EDMX generation,
    /// using McMaster.Extensions.CommandLineUtils for attribute-based command definition.
    /// </remarks>
    [Command(Name = "database", Description = "EasyAF database scaffolding commands.")]
    [Subcommand(typeof(DatabaseGenerateCommand), typeof(DatabaseRefreshCommand))]
    public partial class DatabaseRootCommand
    {

        /// <summary>
        /// Executes the database command. Shows help since this is a parent command.
        /// </summary>
        /// <param name="app">The command line application.</param>
        /// <returns>Exit code.</returns>
        public int OnExecute(CommandLineApplication app)
        {

            app.ShowHelp();
            return 1;

        }

    }

}
