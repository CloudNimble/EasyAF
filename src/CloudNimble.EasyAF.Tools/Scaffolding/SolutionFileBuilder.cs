using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tools.Scaffolding
{

    /// <summary>
    /// Builds a Visual Studio <c>.slnx</c> solution file in-process with
    /// Microsoft.VisualStudio.SolutionPersistence, placing projects and solution items into solution folders.
    /// </summary>
    public class SolutionFileBuilder
    {

        #region Fields

        private readonly List<string> _buildTypes = ["Debug", "Release"];
        private readonly List<(string Folder, string RelativePath)> _projects = [];
        private readonly List<(string Folder, string RelativePath)> _solutionItems = [];

        #endregion

        #region Properties

        /// <summary>
        /// Gets the build types (configurations) the solution declares, in the order they were added.
        /// Always starts with <c>Debug</c> and <c>Release</c>.
        /// </summary>
        public IReadOnlyList<string> BuildTypes => _buildTypes;

        /// <summary>
        /// Gets the directory the solution file is written to. Project and item paths are stored relative to it.
        /// </summary>
        public string SolutionDirectory { get; }

        /// <summary>
        /// Gets the solution file name without extension.
        /// </summary>
        public string SolutionName { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFileBuilder"/> class.
        /// </summary>
        /// <param name="solutionDirectory">The directory the solution file is written to.</param>
        /// <param name="solutionName">The solution file name without extension.</param>
        public SolutionFileBuilder(string solutionDirectory, string solutionName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(solutionDirectory, nameof(solutionDirectory));
            ArgumentException.ThrowIfNullOrWhiteSpace(solutionName, nameof(solutionName));

            SolutionDirectory = Path.GetFullPath(solutionDirectory);
            SolutionName = solutionName;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a build type (configuration) such as <c>DEV</c>. Duplicates are ignored.
        /// </summary>
        /// <param name="buildType">The build type name.</param>
        /// <returns>The current instance for method chaining.</returns>
        public SolutionFileBuilder AddBuildType(string buildType)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(buildType, nameof(buildType));

            if (!_buildTypes.Contains(buildType, StringComparer.OrdinalIgnoreCase))
            {
                _buildTypes.Add(buildType);
            }

            return this;
        }

        /// <summary>
        /// Adds a project to a solution folder.
        /// </summary>
        /// <param name="solutionFolder">The solution folder, for example <c>Core</c> or <c>/Core/</c>.</param>
        /// <param name="projectFilePath">The project file path; relative paths are resolved against <see cref="SolutionDirectory"/>.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the same project has already been added.</exception>
        public SolutionFileBuilder AddProject(string solutionFolder, string projectFilePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(solutionFolder, nameof(solutionFolder));
            ArgumentException.ThrowIfNullOrWhiteSpace(projectFilePath, nameof(projectFilePath));

            var relativePath = GetRelativePath(projectFilePath);
            if (_projects.Any(p => string.Equals(p.RelativePath, relativePath, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"The project '{relativePath}' has already been added to the solution.");
            }

            _projects.Add((NormalizeFolder(solutionFolder), relativePath));
            return this;
        }

        /// <summary>
        /// Adds a loose file (for example <c>Directory.Build.props</c>) to a solution folder.
        /// </summary>
        /// <param name="solutionFolder">The solution folder, for example <c>Solution Items</c>.</param>
        /// <param name="filePath">The file path; relative paths are resolved against <see cref="SolutionDirectory"/>.</param>
        /// <returns>The current instance for method chaining.</returns>
        public SolutionFileBuilder AddSolutionItem(string solutionFolder, string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(solutionFolder, nameof(solutionFolder));
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

            _solutionItems.Add((NormalizeFolder(solutionFolder), GetRelativePath(filePath)));
            return this;
        }

        /// <summary>
        /// Writes the solution file, replacing any existing file of the same name.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The full path of the written solution file.</returns>
        public async Task<string> SaveAsync(CancellationToken cancellationToken = default)
        {
            var model = new SolutionModel();
            foreach (var buildType in _buildTypes)
            {
                model.AddBuildType(buildType);
            }

            foreach (var (folder, relativePath) in _projects)
            {
                model.AddProject(relativePath, projectTypeName: null, folder: model.FindFolder(folder) ?? model.AddFolder(folder));
            }

            foreach (var (folder, relativePath) in _solutionItems)
            {
                (model.FindFolder(folder) ?? model.AddFolder(folder)).AddFile(relativePath);
            }

            var path = Path.Combine(SolutionDirectory, SolutionName + ".slnx");

            Directory.CreateDirectory(SolutionDirectory);
            await SolutionSerializers.SlnXml.SaveAsync(path, model, cancellationToken).ConfigureAwait(false);

            return path;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converts a path into the forward-slash form, relative to <see cref="SolutionDirectory"/>, that solution files store.
        /// </summary>
        /// <param name="path">An absolute path, or a path relative to <see cref="SolutionDirectory"/>.</param>
        /// <returns>The relative path with forward slashes.</returns>
        private string GetRelativePath(string path)
        {
            var fullPath = Path.IsPathRooted(path) ? Path.GetFullPath(path) : Path.GetFullPath(Path.Combine(SolutionDirectory, path));
            return Path.GetRelativePath(SolutionDirectory, fullPath).Replace('\\', '/');
        }

        /// <summary>
        /// Normalizes a solution folder name into the <c>/Name/</c> form SolutionPersistence expects.
        /// </summary>
        /// <param name="solutionFolder">The folder name, with or without surrounding slashes.</param>
        /// <returns>The folder path in <c>/Name/</c> form.</returns>
        private static string NormalizeFolder(string solutionFolder)
        {
            return "/" + solutionFolder.Trim().Trim('/', '\\') + "/";
        }

        #endregion

    }

}
