using McMaster.Extensions.CommandLineUtils;

namespace CloudNimble.EasyAF.Tools.Commands.Root
{

    /// <summary>
    /// Root command for code generation related subcommands.
    /// </summary>
    [Command(Name = "code", Description = "EasyAF C# code generation commands.")]
    [Subcommand(typeof(CodeGenerateCommand))]
    public partial class CodeRootCommand
    {

        /// <summary>
        /// Shows help for the code command.
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
