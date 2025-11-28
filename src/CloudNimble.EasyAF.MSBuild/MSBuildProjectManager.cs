using CloudNimble.EasyAF.Core;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CloudNimble.EasyAF.MSBuild
{

    /// <summary>
    /// Manages MSBuild project files (.csproj, Directory.Build.props, etc.) with formatting preservation capabilities.
    /// </summary>
    /// <remarks>
    /// This class provides comprehensive support for loading, validating, and modifying MSBuild
    /// project files while preserving the original formatting (indentation, line breaks).
    /// It follows the same pattern as DocsJsonManager for consistency.
    /// </remarks>
    public class MSBuildProjectManager
    {

        #region Static Methods

        /// <summary>
        /// Ensures MSBuild is registered with the latest available version.
        /// </summary>
        /// <remarks>
        /// This method should be called before any MSBuild operations to ensure the correct
        /// version of MSBuild is loaded. On .NET Core, QueryVisualStudioInstances() returns
        /// SDK instances (versions like 8.0.x, 9.0.x, 10.0.x), not Visual Studio instances.
        /// We explicitly select and register the latest available instance.
        /// </remarks>
        public static void EnsureMSBuildRegistered()
        {
            if (!MSBuildLocator.IsRegistered)
            {
                try
                {
                    // Query all available instances and pick the latest one.
                    // On .NET Core, these are SDK instances (8.0.x, 9.0.x, 10.0.x).
                    // MSBuildLocator filters by runtime compatibility, so .NET 8 only sees SDKs <= 8.x.
                    var instances = MSBuildLocator.QueryVisualStudioInstances().ToList();
                    var latestInstance = instances
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefault();

                    if (latestInstance is not null)
                    {
                        MSBuildLocator.RegisterInstance(latestInstance);
                    }
                    else
                    {
                        // Fallback if no instances found
                        MSBuildLocator.RegisterDefaults();
                    }
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("assemblies were already loaded"))
                {
                    // MSBuild assemblies were loaded before registration could happen.
                    // This is common in test parallelization scenarios.
                    // In this case, we're already using whatever MSBuild was loaded, so just continue.
                    return;
                }
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// The file path of the loaded project.
        /// </summary>
        private string _filePath;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the loaded MSBuild project root element.
        /// </summary>
        /// <value>
        /// The <see cref="ProjectRootElement"/> instance loaded from the file system.
        /// Returns null if no project has been loaded or if loading failed.
        /// </value>
        public ProjectRootElement Project { get; private set; }

        /// <summary>
        /// Gets the file path of the loaded project.
        /// </summary>
        /// <value>
        /// The absolute path to the project file that was loaded or will be saved to.
        /// Returns null if no file path has been specified.
        /// </value>
        public string FilePath
        {
            get => _filePath;
            private set => _filePath = value;
        }

        /// <summary>
        /// Gets a value indicating whether a project is successfully loaded.
        /// </summary>
        /// <value>
        /// True if a project is loaded and there are no errors; otherwise, false.
        /// </value>
        public bool IsLoaded => Project is not null && ProjectErrors.Count == 0;

        /// <summary>
        /// Gets the collection of project loading and processing errors.
        /// </summary>
        /// <value>
        /// A list of <see cref="CompilerError"/> instances representing any errors
        /// encountered during project loading, validation, or processing operations.
        /// </value>
        public List<CompilerError> ProjectErrors { get; private set; }

        /// <summary>
        /// Gets a value indicating whether formatting preservation is enabled.
        /// </summary>
        /// <value>
        /// True if the project was loaded with formatting preservation; otherwise, false.
        /// </value>
        public bool PreserveFormatting { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MSBuildProjectManager"/> class.
        /// </summary>
        public MSBuildProjectManager()
        {
            ProjectErrors = new List<CompilerError>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSBuildProjectManager"/> class with the specified file path.
        /// </summary>
        /// <param name="filePath">The file path to the MSBuild project file.</param>
        /// <exception cref="ArgumentException">Thrown when filePath is null or whitespace.</exception>
        public MSBuildProjectManager(string filePath) : this()
        {
            Ensure.ArgumentNotNullOrWhiteSpace(filePath, nameof(filePath));
            FilePath = Path.GetFullPath(filePath);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads an existing MSBuild project file from the file path specified in the constructor.
        /// </summary>
        /// <param name="preserveFormatting">Whether to preserve the original formatting of the project file.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no file path has been specified.</exception>
        public MSBuildProjectManager Load(bool preserveFormatting = true)
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                throw new InvalidOperationException("No file path has been specified. Use the constructor overload or Load(string) method.");
            }

            LoadInternal(FilePath, preserveFormatting);
            return this;
        }

        /// <summary>
        /// Loads an existing MSBuild project file from the specified file path.
        /// </summary>
        /// <param name="filePath">The file path to the MSBuild project file.</param>
        /// <param name="preserveFormatting">Whether to preserve the original formatting of the project file.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when filePath is null or whitespace.</exception>
        public MSBuildProjectManager Load(string filePath, bool preserveFormatting = true)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(filePath, nameof(filePath));
            FilePath = Path.GetFullPath(filePath);
            LoadInternal(FilePath, preserveFormatting);
            return this;
        }

        /// <summary>
        /// Creates a new MSBuild project file with default structure.
        /// </summary>
        /// <param name="filePath">The file path where the new project should be created.</param>
        /// <param name="targetFramework">The target framework for the project (default: net8.0).</param>
        /// <exception cref="ArgumentException">Thrown when filePath is null or whitespace.</exception>
        public void CreateNew(string filePath, string targetFramework = "net8.0")
        {
            Ensure.ArgumentNotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.ArgumentNotNullOrWhiteSpace(targetFramework, nameof(targetFramework));

            FilePath = Path.GetFullPath(filePath);
            PreserveFormatting = false; // New files don't have existing formatting to preserve
            ProjectErrors.Clear();

            try
            {
                // Ensure MSBuild is registered
                EnsureMSBuildRegistered();

                // Create a new project
                Project = ProjectRootElement.Create(FilePath);
                
                // Add basic structure for .csproj files
                if (Path.GetExtension(filePath).Equals(".csproj", StringComparison.OrdinalIgnoreCase))
                {
                    Project.Sdk = "Microsoft.NET.Sdk";
                    
                    var propertyGroup = Project.AddPropertyGroup();
                    propertyGroup.AddProperty("TargetFramework", targetFramework);
                }
            }
            catch (Exception ex)
            {
                ProjectErrors.Add(new CompilerError(FilePath, 0, 0, "PROJECT_CREATE", ex.Message));
                Project = null;
            }
        }

        /// <summary>
        /// Saves the current project to the file system using the original file path.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no project is loaded or no file path is specified.</exception>
        public void Save()
        {
            if (Project is null)
            {
                throw new InvalidOperationException("No project is loaded. Load or create a project before saving.");
            }

            if (string.IsNullOrWhiteSpace(FilePath))
            {
                throw new InvalidOperationException("No file path is specified. Use Save(string) to specify a path.");
            }

            Save(FilePath);
        }

        /// <summary>
        /// Saves the current project to the specified file path.
        /// </summary>
        /// <param name="filePath">The file path where the project should be saved.</param>
        /// <exception cref="ArgumentException">Thrown when filePath is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no project is loaded.</exception>
        public void Save(string filePath)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(filePath, nameof(filePath));

            if (Project is null)
            {
                throw new InvalidOperationException("No project is loaded. Load or create a project before saving.");
            }

            try
            {
                // Ensure the directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                Project.Save(filePath);
            }
            catch (Exception ex)
            {
                ProjectErrors.Add(new CompilerError(filePath, 0, 0, "PROJECT_SAVE", $"Failed to save project: {ex.Message}"));
                throw;
            }
        }

        /// <summary>
        /// Sets a property value in the project.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when name or value is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no project is loaded.</exception>
        public MSBuildProjectManager SetProperty(string name, string value)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(name, nameof(name));
            Ensure.ArgumentNotNullOrWhiteSpace(value, nameof(value));

            if (Project is null)
            {
                throw new InvalidOperationException("No project is loaded. Load or create a project before setting properties.");
            }

            try
            {
                // Find existing property
                var existingProperty = Project.Properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                
                if (existingProperty is not null)
                {
                    // Update existing property
                    existingProperty.Value = value;
                }
                else
                {
                    // Add new property to the first PropertyGroup, or create one if none exists
                    var propertyGroup = Project.PropertyGroups.FirstOrDefault();
                    if (propertyGroup is null)
                    {
                        propertyGroup = Project.AddPropertyGroup();
                    }
                    
                    propertyGroup.AddProperty(name, value);
                }
            }
            catch (Exception ex)
            {
                ProjectErrors.Add(new CompilerError(FilePath, 0, 0, "PROPERTY_SET", $"Failed to set property '{name}': {ex.Message}"));
                throw;
            }

            return this;
        }

        /// <summary>
        /// Removes a property from the project.
        /// </summary>
        /// <param name="name">The property name to remove.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no project is loaded.</exception>
        public MSBuildProjectManager RemoveProperty(string name)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(name, nameof(name));

            if (Project is null)
            {
                throw new InvalidOperationException("No project is loaded. Load or create a project before removing properties.");
            }

            try
            {
                var propertyToRemove = Project.Properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (propertyToRemove is not null)
                {
                    propertyToRemove.Parent.RemoveChild(propertyToRemove);
                }
            }
            catch (Exception ex)
            {
                ProjectErrors.Add(new CompilerError(FilePath, 0, 0, "PROPERTY_REMOVE", $"Failed to remove property '{name}': {ex.Message}"));
                throw;
            }

            return this;
        }

        /// <summary>
        /// Gets the value of a property from the project.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>The property value, or null if the property does not exist.</returns>
        /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no project is loaded.</exception>
        public string GetPropertyValue(string name)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(name, nameof(name));

            if (Project is null)
            {
                throw new InvalidOperationException("No project is loaded. Load or create a project before getting properties.");
            }

            return Project.Properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        /// <summary>
        /// Adds a PackageReference to the project.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <param name="version">The package version.</param>
        /// <param name="condition">Optional condition for the PackageReference.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when packageId or version is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no project is loaded.</exception>
        public MSBuildProjectManager AddPackageReference(string packageId, string version, string condition = null)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(packageId, nameof(packageId));
            Ensure.ArgumentNotNullOrWhiteSpace(version, nameof(version));

            if (Project is null)
            {
                throw new InvalidOperationException("No project is loaded. Load or create a project before adding package references.");
            }

            try
            {
                // Find or create ItemGroup for PackageReference
                var itemGroup = Project.ItemGroups.FirstOrDefault(ig => 
                    ig.Items.Any(item => item.ItemType == "PackageReference") &&
                    (string.IsNullOrEmpty(condition) || ig.Condition == condition));

                if (itemGroup is null)
                {
                    itemGroup = Project.AddItemGroup();
                    if (!string.IsNullOrEmpty(condition))
                    {
                        itemGroup.Condition = condition;
                    }
                }

                // Check if the package reference already exists
                var existingReference = itemGroup.Items.FirstOrDefault(item => 
                    item.ItemType == "PackageReference" && 
                    item.Include.Equals(packageId, StringComparison.OrdinalIgnoreCase));

                if (existingReference is not null)
                {
                    // Update existing reference
                    var versionMetadata = existingReference.Metadata.FirstOrDefault(m => m.Name == "Version");
                    if (versionMetadata is not null)
                    {
                        versionMetadata.Value = version;
                    }
                    else
                    {
                        existingReference.AddMetadata("Version", version);
                    }
                }
                else
                {
                    // Add new package reference
                    var packageRef = itemGroup.AddItem("PackageReference", packageId);
                    packageRef.AddMetadata("Version", version);
                }
            }
            catch (Exception ex)
            {
                ProjectErrors.Add(new CompilerError(FilePath, 0, 0, "PACKAGE_REFERENCE_ADD", $"Failed to add package reference '{packageId}': {ex.Message}"));
                throw;
            }

            return this;
        }

        /// <summary>
        /// Adds an ItemGroup with the specified condition and configures it using the provided action.
        /// </summary>
        /// <param name="condition">The condition for the ItemGroup.</param>
        /// <param name="configure">An action to configure the ItemGroup.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when configure is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no project is loaded.</exception>
        public MSBuildProjectManager AddItemGroup(string condition, Action<ItemGroupBuilder> configure)
        {
            Ensure.ArgumentNotNull(configure, nameof(configure));

            if (Project is null)
            {
                throw new InvalidOperationException("No project is loaded. Load or create a project before adding item groups.");
            }

            try
            {
                var itemGroup = Project.AddItemGroup();
                if (!string.IsNullOrEmpty(condition))
                {
                    itemGroup.Condition = condition;
                }

                var builder = new ItemGroupBuilder(itemGroup);
                configure(builder);
            }
            catch (Exception ex)
            {
                ProjectErrors.Add(new CompilerError(FilePath, 0, 0, "ITEMGROUP_ADD", $"Failed to add item group: {ex.Message}"));
                throw;
            }

            return this;
        }

        #endregion

        #region EasyAF-Specific Methods

        /// <summary>
        /// Sets the EasyAFProjectType property in the project.
        /// </summary>
        /// <param name="projectType">The EasyAF project type (e.g., "Data", "Business", "Api", "Core", "SimpleMessageBus").</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when projectType is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no project is loaded.</exception>
        internal MSBuildProjectManager SetEasyAFProjectType(string projectType)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(projectType, nameof(projectType));
            return SetProperty("EasyAFProjectType", projectType);
        }

        /// <summary>
        /// Sets the UserSecretsId property in the project.
        /// </summary>
        /// <param name="userSecretsId">The UserSecretsId GUID.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when userSecretsId is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no project is loaded.</exception>
        internal MSBuildProjectManager SetUserSecretsId(string userSecretsId)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(userSecretsId, nameof(userSecretsId));
            return SetProperty("UserSecretsId", userSecretsId);
        }

        /// <summary>
        /// Sets the EasyAFNamespace property in the project.
        /// </summary>
        /// <param name="namespaceName">The EasyAF namespace.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when namespaceName is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no project is loaded.</exception>
        internal MSBuildProjectManager SetEasyAFNamespace(string namespaceName)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(namespaceName, nameof(namespaceName));
            return SetProperty("EasyAFNamespace", namespaceName);
        }

        /// <summary>
        /// Adds the EasyAF analyzer package and configuration to the project.
        /// </summary>
        /// <param name="dataProjectRelativePath">Optional relative path to the Data project for .edmx file references.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no project is loaded.</exception>
        internal MSBuildProjectManager AddEasyAFAnalyzers(string dataProjectRelativePath = null)
        {
            return AddItemGroup(" '$(EasyAFProjectType)' != '' ", itemGroup =>
            {
                var packageRef = itemGroup.AddPackageReference("EasyAF.Analyzers.EF6", "3.*-*")
                                          .SetPrivateAssets("all");

                // Add AdditionalFiles if we have a Data project path
                if (!string.IsNullOrEmpty(dataProjectRelativePath))
                {
                    itemGroup.AddAdditionalFiles($"..\\{dataProjectRelativePath}\\*.edmx")
                             .SetLink("EasyAF\\%(FileName).edmx")
                             .SetVisible(false);
                }
            });
        }

        /// <summary>
        /// Creates a default Directory.Build.props configuration for EasyAF projects.
        /// </summary>
        /// <param name="filePath">The path where Directory.Build.props should be created.</param>
        /// <param name="easyAFNamespace">The EasyAF namespace for the solution.</param>
        /// <param name="userSecretsId">The UserSecretsId for the solution.</param>
        /// <param name="dataProjectRelativePath">Optional relative path to the Data project for .edmx file references.</param>
        /// <returns>A new MSBuildProjectManager instance configured for Directory.Build.props.</returns>
        /// <exception cref="ArgumentException">Thrown when required parameters are null or whitespace.</exception>
        internal static MSBuildProjectManager CreateDirectoryBuildProps(string filePath, string easyAFNamespace, string userSecretsId, string dataProjectRelativePath = null)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.ArgumentNotNullOrWhiteSpace(easyAFNamespace, nameof(easyAFNamespace));
            Ensure.ArgumentNotNullOrWhiteSpace(userSecretsId, nameof(userSecretsId));

            var manager = new MSBuildProjectManager();
            manager.CreateNew(filePath); // Directory.Build.props doesn't need a target framework
            
            // Remove the default TargetFramework property that gets added for .csproj files
            manager.RemoveProperty("TargetFramework");

            manager.SetEasyAFNamespace(easyAFNamespace)
                   .SetUserSecretsId(userSecretsId)
                   .AddEasyAFAnalyzers(dataProjectRelativePath);

            return manager;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Internal method to load a project file with error handling.
        /// </summary>
        /// <param name="filePath">The file path to load.</param>
        /// <param name="preserveFormatting">Whether to preserve formatting.</param>
        private void LoadInternal(string filePath, bool preserveFormatting)
        {
            PreserveFormatting = preserveFormatting;
            ProjectErrors.Clear();

            if (!File.Exists(filePath))
            {
                ProjectErrors.Add(new CompilerError(filePath, 0, 0, "FILE_NOT_FOUND", $"Project file not found: {filePath}"));
                Project = null;
                return;
            }

            try
            {
                // Ensure MSBuild is registered
                EnsureMSBuildRegistered();

                // Try to load with formatting preservation first
                if (preserveFormatting)
                {
                    try
                    {
                        Project = ProjectRootElement.Open(filePath, ProjectCollection.GlobalProjectCollection, preserveFormatting: true);
                    }
                    catch (Exception ex)
                    {
                        // If formatting preservation fails, log it and try without preservation
                        ProjectErrors.Add(new CompilerError(filePath, 0, 0, "FORMATTING_PRESERVATION_FAILED", 
                            $"Failed to load with formatting preservation, falling back to standard loading: {ex.Message}")
                        {
                            IsWarning = true
                        });

                        Project = ProjectRootElement.Open(filePath, ProjectCollection.GlobalProjectCollection, preserveFormatting: false);
                        PreserveFormatting = false;
                    }
                }
                else
                {
                    Project = ProjectRootElement.Open(filePath, ProjectCollection.GlobalProjectCollection, preserveFormatting: false);
                }
            }
            catch (Exception ex)
            {
                ProjectErrors.Add(new CompilerError(filePath, 0, 0, "PROJECT_LOAD", $"Failed to load project: {ex.Message}"));
                Project = null;
            }
        }

        #endregion

    }

}
