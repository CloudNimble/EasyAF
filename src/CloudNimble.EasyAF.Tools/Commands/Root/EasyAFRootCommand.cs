
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

        #region Properties

        /// <summary>
        /// Gets the description shown at the top of the help, with the tool's major and minor version.
        /// </summary>
        internal static string Description { get; }

        /// <summary>
        /// Gets a value indicating whether this tool is a prerelease build, such as <c>5.0.0-CI-20261003-233509</c>.
        /// </summary>
        internal static bool IsPrerelease { get; }

        /// <summary>
        /// Gets the version of this tool, without its prerelease label or build metadata.
        /// </summary>
        /// <remarks>
        /// The version comes from the informational version (the package version the tool was built with), not the assembly version,
        /// which is pinned for binding compatibility.
        /// </remarks>
        internal static Version Version { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Reads the tool version once, when the CLI starts.
        /// </summary>
        static EasyAFRootCommand()
        {
            var assembly = typeof(EasyAFRootCommand).Assembly;
            (Version, IsPrerelease) = ParseVersion(assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
            Version ??= assembly.GetName().Version;
            Description = $"EasyAF {Version.Major}.{Version.Minor} CLI Tools.\nBy CloudNimble. https://nimbleapps.cloud";
        }

        #endregion

        #region Public Methods

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

        #endregion

        #region Internal Methods

        /// <summary>
        /// Splits an informational version into its version and whether it has a prerelease label.
        /// </summary>
        /// <param name="informationalVersion">The informational version, for example <c>5.0.0-CI-20261003-233509+abc123</c>.</param>
        /// <returns>The version (<see langword="null"/> when it can't be parsed) and whether it is a prerelease.</returns>
        internal static (Version Version, bool IsPrerelease) ParseVersion(string informationalVersion)
        {
            // RWM: Build metadata comes after "+", and a prerelease label after the first "-" before it.
            var parts = informationalVersion?.Split('+')[0].Split('-', 2);
            return Version.TryParse(parts?[0], out var version) ? (version, parts.Length > 1) : (null, false);
        }

        #endregion

    }

}
