using System.Collections.Generic;

namespace CloudNimble.EasyAF.Tools.Scaffolding
{

    /// <summary>
    /// Describes one project in a scaffolded EasyAF solution: how it is created and how its project file is configured.
    /// </summary>
    /// <param name="Suffix">The project name after the solution namespace, for example <c>Api</c> or <c>MessageBus.Runtime</c>.</param>
    /// <param name="TemplateShortName">The SDK template the project is created from: <c>classlib</c>, <c>web</c>, or <c>console</c>.</param>
    /// <param name="SolutionFolder">The solution folder the project is placed in.</param>
    /// <param name="EasyAFProjectType">The <c>EasyAFProjectType</c> to stamp, or <see langword="null"/> for none.</param>
    /// <param name="Packages">The package references, sorted by id.</param>
    /// <param name="ProjectReferences">The suffixes of referenced projects, sorted.</param>
    /// <param name="Properties">Extra MSBuild properties to stamp.</param>
    /// <param name="CopyAppSettingsToOutput">Whether <c>appsettings*.json</c> must be copied to the output folder (non-web executables).</param>
    public sealed record ScaffoldProject(
        string Suffix,
        string TemplateShortName,
        string SolutionFolder,
        string EasyAFProjectType,
        IReadOnlyList<ScaffoldPackage> Packages,
        IReadOnlyList<string> ProjectReferences,
        IReadOnlyDictionary<string, string> Properties,
        bool CopyAppSettingsToOutput)
    {

        /// <summary>
        /// Gets the full project name for a solution namespace.
        /// </summary>
        /// <param name="namespace">The solution namespace.</param>
        /// <returns>The project name, for example <c>CloudNimble.Contoso.Api</c>.</returns>
        public string GetName(string @namespace)
        {
            return $"{@namespace}.{Suffix}";
        }

    }

}
