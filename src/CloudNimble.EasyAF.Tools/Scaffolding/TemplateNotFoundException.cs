using System;

namespace CloudNimble.EasyAF.Tools.Scaffolding
{

    /// <summary>
    /// Thrown when a .NET SDK project template cannot be found in the installed SDK's templates folder.
    /// </summary>
    public class TemplateNotFoundException : Exception
    {

        #region Properties

        /// <summary>
        /// Gets the short name of the template that was requested (for example, <c>classlib</c>).
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Gets the folder that was searched for template packages.
        /// </summary>
        public string SearchedPath { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateNotFoundException"/> class.
        /// </summary>
        /// <param name="shortName">The short name of the template that was requested.</param>
        /// <param name="searchedPath">The folder that was searched for template packages.</param>
        /// <param name="targetFramework">The target framework the template was requested for.</param>
        public TemplateNotFoundException(string shortName, string searchedPath, string targetFramework)
            : base($"The '{shortName}' C# project template for {targetFramework} was not found in '{searchedPath}'. " +
                   $"Install a .NET SDK whose major version matches {targetFramework}, or set DOTNET_ROOT to the installation that has one.")
        {
            ShortName = shortName;
            SearchedPath = searchedPath;
        }

        #endregion

    }

}
