using CloudNimble.EasyAF.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace CloudNimble.EasyAF.Tools.ProjectDiscovery
{

    /// <summary>
    /// Represents information about a discovered project.
    /// </summary>
    /// <remarks>
    /// This class contains metadata about a project file, including its path,
    /// target frameworks, output directories, and XML documentation settings.
    /// It is used by the project discovery system to identify eligible projects
    /// for documentation generation.
    /// </remarks>
    public class ProjectInfo
    {

        #region Properties

        /// <summary>
        /// Gets or sets the full path to the project file.
        /// </summary>
        public string ProjectPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the project name (without extension).
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the assembly name for the project.
        /// </summary>
        public string AssemblyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the collection of target frameworks for this project.
        /// </summary>
        public List<string> TargetFrameworks { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the project directory path.
        /// </summary>
        public string ProjectDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this project generates XML documentation.
        /// </summary>
        public bool GeneratesDocumentation { get; set; }

        /// <summary>
        /// Gets or sets the XML documentation file path pattern.
        /// </summary>
        public string DocumentationFile { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this is a test project.
        /// </summary>
        public bool IsTestProject { get; set; }

        /// <summary>
        /// Gets or sets whether this is a template project.
        /// </summary>
        public bool IsTemplateProject { get; set; }

        /// <summary>
        /// Gets or sets whether this is a tool project.
        /// </summary>
        public bool IsToolProject { get; set; }

        /// <summary>
        /// Gets or sets the latest (highest version) target framework.
        /// </summary>
        public string LatestTargetFramework { get; set; } = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ProjectInfo class.
        /// </summary>
        public ProjectInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ProjectInfo class with a project path.
        /// </summary>
        /// <param name="projectPath">The path to the project file.</param>
        public ProjectInfo(string projectPath)
        {
            Ensure.ArgumentNotNull(projectPath, nameof(projectPath));

            ProjectPath = Path.GetFullPath(projectPath);
            ProjectDirectory = Path.GetDirectoryName(ProjectPath) ?? string.Empty;
            ProjectName = Path.GetFileNameWithoutExtension(ProjectPath);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the XML documentation file path for the latest target framework.
        /// </summary>
        /// <returns>The path to the XML documentation file, or empty string if not available.</returns>
        public string GetLatestDocumentationFilePath()
        {
            // Always check for documentation files in standard locations
            // This handles cases where GenerateDocumentationFile is set in Directory.Build.props
            
            var configurations = new[] { "Debug", "Release" };
            var xmlFileName = $"{AssemblyName}.xml";

            // If we have an explicit DocumentationFile path without MSBuild properties, try it first
            if (!string.IsNullOrWhiteSpace(DocumentationFile) && !DocumentationFile.Contains("$("))
            {
                var explicitPath = Path.IsPathRooted(DocumentationFile) 
                    ? DocumentationFile 
                    : Path.Combine(ProjectDirectory, DocumentationFile);
                    
                if (File.Exists(explicitPath))
                {
                    return Path.GetFullPath(explicitPath);
                }
            }

            // Try standard locations for .NET SDK projects
            foreach (var configuration in configurations)
            {
                var possiblePaths = new List<string>();

                // For projects with target framework
                if (!string.IsNullOrWhiteSpace(LatestTargetFramework))
                {
                    possiblePaths.Add(Path.Combine(ProjectDirectory, "bin", configuration, LatestTargetFramework, xmlFileName));
                }
                
                // For projects without target framework or legacy projects
                possiblePaths.Add(Path.Combine(ProjectDirectory, "bin", configuration, xmlFileName));
                
                // Check obj folder as well (sometimes XML docs are generated there)
                if (!string.IsNullOrWhiteSpace(LatestTargetFramework))
                {
                    possiblePaths.Add(Path.Combine(ProjectDirectory, "obj", configuration, LatestTargetFramework, xmlFileName));
                }
                possiblePaths.Add(Path.Combine(ProjectDirectory, "obj", configuration, xmlFileName));

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        return Path.GetFullPath(path);
                    }
                }
            }

            // Check root directory as last resort
            var rootPath = Path.Combine(ProjectDirectory, xmlFileName);
            if (File.Exists(rootPath))
            {
                return Path.GetFullPath(rootPath);
            }

            // Return expected path even if file doesn't exist (for error messages)
            return Path.GetFullPath(Path.Combine(ProjectDirectory, "bin", "Debug", 
                string.IsNullOrWhiteSpace(LatestTargetFramework) ? "" : LatestTargetFramework, 
                xmlFileName));
        }

        /// <summary>
        /// Gets all XML documentation file paths for all target frameworks.
        /// </summary>
        /// <returns>A dictionary mapping target frameworks to documentation file paths.</returns>
        public Dictionary<string, string> GetAllDocumentationFilePaths()
        {
            var result = new Dictionary<string, string>();

            if (!GeneratesDocumentation || string.IsNullOrWhiteSpace(DocumentationFile))
            {
                return result;
            }

            foreach (var framework in TargetFrameworks)
            {
                var docPath = DocumentationFile
                    .Replace("$(TargetFramework)", framework)
                    .Replace("$(AssemblyName)", AssemblyName)
                    .Replace("$(Configuration)", "Debug");

                if (!Path.IsPathRooted(docPath))
                {
                    docPath = Path.Combine(ProjectDirectory, docPath);
                }

                result[framework] = Path.GetFullPath(docPath);
            }

            return result;
        }

        /// <summary>
        /// Determines whether this project should be included in documentation generation.
        /// </summary>
        /// <returns>True if the project should be included; otherwise, false.</returns>
        public bool ShouldIncludeInDocumentation()
        {
            // Exclude test, template, and tool projects
            if (IsTestProject || IsTemplateProject || IsToolProject)
            {
                return false;
            }

            // Must have a target framework
            if (string.IsNullOrWhiteSpace(LatestTargetFramework))
            {
                return false;
            }

            // Include if we explicitly know it generates documentation
            if (GeneratesDocumentation)
            {
                return true;
            }

            // Otherwise, check if a documentation file actually exists
            // This handles cases where GenerateDocumentationFile is set in Directory.Build.props
            var docPath = GetLatestDocumentationFilePath();
            return !string.IsNullOrWhiteSpace(docPath) && File.Exists(docPath);
        }

        /// <summary>
        /// Returns a string representation of the project information.
        /// </summary>
        /// <returns>A string containing the project name and target frameworks.</returns>
        public override string ToString()
        {
            var frameworks = TargetFrameworks.Count > 0 
                ? $" ({string.Join(", ", TargetFrameworks)})" 
                : string.Empty;
            
            return $"{ProjectName}{frameworks}";
        }

        #endregion

    }

}