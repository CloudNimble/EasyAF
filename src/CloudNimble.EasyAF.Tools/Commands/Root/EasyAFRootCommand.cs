
using McMaster.Extensions.CommandLineUtils;
using System;

namespace CloudNimble.EasyAF.Tools.Commands.Root
{

    /// <summary>
    /// Root command for the EasyAF command line tool.
    /// </summary>
    /// <remarks>
    /// This class serves as the entry point for the EasyAF CLI tool and defines available subcommands.
    /// When executed without specific subcommands, it displays the help information.
    /// </remarks>
    /// <example>
    /// <code>
    /// dotnet easyaf
    /// </code>
    /// </example>
    [Command(Description = "EasyAF 3.0 CLI Tools.\nBy CloudNimble. https://nimbleapps.cloud")]
    [Subcommand(typeof(InitCommand), typeof(SetupCommand), typeof(CleanupCommand), typeof(CodeRootCommand), typeof(DatabaseRootCommand), typeof(EdmxRootCommand))]
    public class EasyAFRootCommand
    {

        /// <summary>
        /// Executes when the root command is invoked without subcommands.
        /// </summary>
        /// <param name="app">The command line application instance.</param>
        /// <returns>Exit code 1 to indicate no specific command was executed.</returns>
        public int OnExecute(CommandLineApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.ShowHelp();      
            return 1;
        }

    }

}
