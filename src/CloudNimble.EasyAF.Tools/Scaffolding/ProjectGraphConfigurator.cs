using CloudNimble.EasyAF.MSBuild;
using Microsoft.Build.Construction;
using System;
using System.IO;
using System.Linq;

namespace CloudNimble.EasyAF.Tools.Scaffolding
{

    /// <summary>
    /// Applies a <see cref="ScaffoldProject"/> description to a project file freshly created from an SDK template.
    /// </summary>
    /// <remarks>
    /// The SDK templates always add <c>ImplicitUsings</c> and <c>Nullable</c> (the class library and console templates through
    /// computed symbols a caller cannot set, the web template unconditionally), so they are removed here and the solution's
    /// <c>Directory.Build.props</c> decides. Reference groups are written immediately after the leading <c>PropertyGroup</c>,
    /// <c>PackageReference</c> first, then <c>ProjectReference</c>, entries alphabetical, versions as attributes.
    /// </remarks>
    public class ProjectGraphConfigurator
    {

        #region Fields

        private static readonly string[] TemplatePropertiesToRemove = ["ImplicitUsings", "Nullable"];

        #endregion

        #region Properties

        /// <summary>
        /// Gets the solution namespace that prefixes every project name.
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// Gets the solution directory that contains one folder per project.
        /// </summary>
        public string SolutionDirectory { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectGraphConfigurator"/> class.
        /// </summary>
        /// <param name="solutionDirectory">The solution directory that contains one folder per project.</param>
        /// <param name="namespace">The solution namespace that prefixes every project name.</param>
        public ProjectGraphConfigurator(string solutionDirectory, string @namespace)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(solutionDirectory, nameof(solutionDirectory));
            ArgumentException.ThrowIfNullOrWhiteSpace(@namespace, nameof(@namespace));

            SolutionDirectory = Path.GetFullPath(solutionDirectory);
            Namespace = @namespace;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Configures the project file at <c>{SolutionDirectory}/{name}/{name}.csproj</c>.
        /// </summary>
        /// <param name="project">The project description.</param>
        /// <returns>The full path of the configured project file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the project file does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the project file cannot be loaded.</exception>
        public string Configure(ScaffoldProject project)
        {
            ArgumentNullException.ThrowIfNull(project, nameof(project));

            var name = project.GetName(Namespace);
            var path = Path.Combine(SolutionDirectory, name, $"{name}.csproj");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"The project file '{path}' was not found. Create it from the '{project.TemplateShortName}' template first.", path);
            }

            var manager = new MSBuildProjectManager();
            manager.Load(path, preserveFormatting: true);
            if (manager.Project is null)
            {
                throw new InvalidOperationException($"The project file '{path}' could not be loaded: {string.Join("; ", manager.ProjectErrors.Select(e => e.ErrorText))}");
            }

            foreach (var property in TemplatePropertiesToRemove)
            {
                while (manager.GetPropertyValue(property) is not null)
                {
                    manager.RemoveProperty(property);
                }
            }

            if (!string.IsNullOrWhiteSpace(project.EasyAFProjectType))
            {
                manager.SetEasyAFProjectType(project.EasyAFProjectType);
            }

            foreach (var (propertyName, value) in project.Properties.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
            {
                manager.SetProperty(propertyName, value);
            }

            var root = manager.Project;
            if (project.Packages.Count > 0)
            {
                var packages = root.AddItemGroup();
                foreach (var package in project.Packages)
                {
                    packages.AddItem("PackageReference", package.Id).AddMetadata("Version", package.Version, expressAsAttribute: true);
                }
            }

            if (project.ProjectReferences.Count > 0)
            {
                var references = root.AddItemGroup();
                foreach (var reference in project.ProjectReferences)
                {
                    var referenceName = $"{Namespace}.{reference}";
                    references.AddItem("ProjectReference", $@"..\{referenceName}\{referenceName}.csproj");
                }
            }

            if (project.CopyAppSettingsToOutput)
            {
                var appSettings = root.AddItemGroup();
                AddNoneUpdate(root, appSettings, "appsettings.json", dependentUpon: null);
                AddNoneUpdate(root, appSettings, "appsettings.*.json", dependentUpon: "appsettings.json");
            }

            manager.Save();
            return path;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds a <c>&lt;None Update="..."&gt;</c> item that always copies the matching files to the output folder.
        /// </summary>
        /// <param name="root">The project root, used to create the element.</param>
        /// <param name="group">The item group to add to.</param>
        /// <param name="update">The file pattern to update.</param>
        /// <param name="dependentUpon">The file to nest under in Solution Explorer, or <see langword="null"/>.</param>
        private static void AddNoneUpdate(ProjectRootElement root, ProjectItemGroupElement group, string update, string dependentUpon)
        {
            // MSBuild validates an item as soon as it is parented, so Update must be set before AppendChild.
            var item = root.CreateItemElement("None");
            item.Update = update;
            group.AppendChild(item);

            if (dependentUpon is not null)
            {
                item.AddMetadata("DependentUpon", dependentUpon);
            }

            item.AddMetadata("CopyToOutputDirectory", "Always");
        }

        #endregion

    }

}
