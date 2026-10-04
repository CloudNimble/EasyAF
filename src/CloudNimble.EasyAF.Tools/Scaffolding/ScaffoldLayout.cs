using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudNimble.EasyAF.Tools.Scaffolding
{

    /// <summary>
    /// Defines the projects, references, and packages of a scaffolded EasyAF solution.
    /// </summary>
    /// <remarks>
    /// The graph is:
    /// <code>
    /// Data                → Core
    /// Business            → Core, Data
    /// Api                 → Business, Data, MessageBus.Core
    /// MessageBus.Core     → Core
    /// MessageBus.Dispatch → Business, MessageBus.Core
    /// MessageBus.Runtime  → Business, Data, MessageBus.Core, MessageBus.Dispatch
    /// Tests.Core          → Core
    /// Tests.Business      → Business, Data
    /// Tests.Api           → Api
    /// </code>
    /// References to projects that are not created (because of <c>--no-api</c>, <c>--no-messagebus</c>, or <c>--no-runtime</c>) are dropped.
    /// </remarks>
    public static class ScaffoldLayout
    {

        #region Fields

        private const string BreakdanceVersion = "8.*-*";
        private const string McpVersion = "1.*-*";
        private const string RestierVersion = "1.*";
        private const string SimpleMessageBusVersion = "6.*";

        private static readonly IReadOnlyDictionary<string, string> NoProperties = new Dictionary<string, string>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates the project list for the given options, in solution order.
        /// </summary>
        /// <param name="options">The validated scaffold options.</param>
        /// <returns>The projects to create.</returns>
        public static IReadOnlyList<ScaffoldProject> Create(ScaffoldOptions options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            var microsoft = options.MicrosoftPackageVersion;
            var projects = new List<ScaffoldProject>
            {
                Project("Core", "classlib", "Core", "Core",
                    [new("EasyAF.Core", options.EasyAFPackageVersion)],
                    []),
                Project("Data", "classlib", "Core", "Data",
                    [new("EasyAF.Data.EFCore", options.EasyAFPackageVersion), new("Microsoft.EntityFrameworkCore.SqlServer", microsoft)],
                    ["Core"]),
                Project("Business", "classlib", "Core", "Business",
                    [new("EasyAF.Business.EFCore", options.EasyAFPackageVersion)],
                    ["Core", "Data"]),
            };

            if (options.IncludeApi)
            {
                projects.Add(Project("Api", "web", "Web", "Api",
                    [
                        new("EasyAF.Restier.EFCore", options.EasyAFPackageVersion),
                        new("Microsoft.EntityFrameworkCore.SqlServer", microsoft),
                        new("Microsoft.OData.Mcp.AspNetCore", McpVersion),
                        new("Microsoft.Restier.AspNetCore", RestierVersion),
                        new("SimpleMessageBus.Publish", SimpleMessageBusVersion),
                        new("SimpleMessageBus.Publish.Azure", SimpleMessageBusVersion),
                    ],
                    ["Business", "Data", "MessageBus.Core"]));
            }

            if (options.IncludeMessageBus)
            {
                projects.Add(Project("MessageBus.Core", "classlib", "MessageBus", "SimpleMessageBus",
                    [new("SimpleMessageBus.Core", SimpleMessageBusVersion)],
                    ["Core"]));
                projects.Add(Project("MessageBus.Dispatch", "classlib", "MessageBus", null,
                    [new("SimpleMessageBus.Dispatch", SimpleMessageBusVersion)],
                    ["Business", "MessageBus.Core"]));
            }

            if (options.IncludeRuntime)
            {
                var properties = options.WebJob
                    ? new Dictionary<string, string>
                    {
                        ["IsWebJobProject"] = "true",
                        ["WebJobName"] = $"{options.Product}WebJobs",
                        ["WebJobType"] = "Continuous",
                    }
                    : NoProperties;

                projects.Add(Project("MessageBus.Runtime", "console", "MessageBus", null,
                    [
                        new("SimpleMessageBus.Dispatch.Azure", SimpleMessageBusVersion),
                        new("SimpleMessageBus.Dispatch.FileSystem", SimpleMessageBusVersion),
                        new("SimpleMessageBus.Hosting", SimpleMessageBusVersion),
                    ],
                    ["Business", "Data", "MessageBus.Core", "MessageBus.Dispatch"],
                    properties,
                    copyAppSettingsToOutput: true));
            }

            ScaffoldPackage[] breakdance = [new("Breakdance.Assemblies", BreakdanceVersion), new("Breakdance.Extensions.MSTest2", BreakdanceVersion)];
            projects.Add(Project("Tests.Core", "classlib", "Tests", null, breakdance, ["Core"]));
            projects.Add(Project("Tests.Business", "classlib", "Tests", null, breakdance, ["Business", "Data"]));
            if (options.IncludeApi)
            {
                projects.Add(Project("Tests.Api", "classlib", "Tests", null, breakdance, ["Api"]));
            }

            var created = projects.Select(p => p.Suffix).ToHashSet(StringComparer.Ordinal);
            return [.. projects.Select(p => p with { ProjectReferences = [.. p.ProjectReferences.Where(created.Contains)] })];
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a <see cref="ScaffoldProject"/> with packages and references sorted.
        /// </summary>
        /// <param name="suffix">The project suffix.</param>
        /// <param name="template">The SDK template short name.</param>
        /// <param name="folder">The solution folder.</param>
        /// <param name="projectType">The <c>EasyAFProjectType</c>, or <see langword="null"/>.</param>
        /// <param name="packages">The package references.</param>
        /// <param name="references">The referenced project suffixes.</param>
        /// <param name="properties">Extra MSBuild properties, or <see langword="null"/> for none.</param>
        /// <param name="copyAppSettingsToOutput">Whether appsettings files must be copied to the output folder.</param>
        /// <returns>The project description.</returns>
        private static ScaffoldProject Project(string suffix, string template, string folder, string projectType, ScaffoldPackage[] packages, string[] references,
            IReadOnlyDictionary<string, string> properties = null, bool copyAppSettingsToOutput = false)
        {
            return new ScaffoldProject(
                suffix,
                template,
                folder,
                projectType,
                [.. packages.OrderBy(p => p.Id, StringComparer.OrdinalIgnoreCase)],
                [.. references.OrderBy(r => r, StringComparer.OrdinalIgnoreCase)],
                properties ?? NoProperties,
                copyAppSettingsToOutput);
        }

        #endregion

    }

}
