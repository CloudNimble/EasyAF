
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Reflection;

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
    [Command]
    [Subcommand(typeof(NewCommand), typeof(InitCommand), typeof(SetupCommand), typeof(CleanupCommand), typeof(CodeRootCommand), typeof(DatabaseRootCommand), typeof(EdmxRootCommand))]
    public class EasyAFRootCommand
    {

        /// <summary>
        /// Gets the description shown at the top of the help, with the tool's major and minor version.
        /// </summary>
        /// <remarks>
        /// The version comes from the informational version (the package version the tool was built with), not the assembly version,
        /// which is pinned for binding compatibility.
        /// </remarks>
        internal static string Description { get; } = $"EasyAF {GetMajorMinorVersion()} CLI Tools.\nBy CloudNimble. https://nimbleapps.cloud";

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

        /// <summary>
        /// Gets the major and minor version, for example <c>5.0</c>, from the informational version of this assembly.
        /// </summary>
        private static string GetMajorMinorVersion()
        {
            var assembly = typeof(EasyAFRootCommand).Assembly;
            var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            // RWM: Strip the prerelease label and build metadata, e.g. "5.0.0-CI-20261003-233509+abc123" -> "5.0.0".
            var version = Version.TryParse(informationalVersion?.Split('-', '+')[0], out var parsed) ? parsed : assembly.GetName().Version;
            return $"{version.Major}.{version.Minor}";
        }

    }

}
