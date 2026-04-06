using CloudNimble.EasyAF.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tools.ProjectDiscovery
{

    /// <summary>
    /// Service for discovering and analyzing .NET projects in a solution.
    /// </summary>
    /// <remarks>
    /// This service scans for solution files, project files, and analyzes their configurations
    /// to identify projects that are eligible for documentation generation. It handles
    /// multi-targeting scenarios and determines the best documentation files to use.
    /// </remarks>
    public class ProjectDiscoveryService
    {

        #region Private Fields

        private static readonly string[] TestProjectIndicators = 
        {
            "test", "tests", "testing", "unittest", "integrationtest", "spec", "specs"
        };

        private static readonly string[] TemplateProjectIndicators = 
        {
            "template", "templates", "scaffold", "boilerplate"
        };

        private static readonly string[] ToolProjectIndicators = 
        {
            "tool", "tools", "utility", "utilities", "cli"
        };

        private static readonly Dictionary<string, int> FrameworkVersions = new()
        {
            { "net48", 48 },
            { "net472", 472 },
            { "net471", 471 },
            { "net47", 470 },
            { "net462", 462 },
            { "net461", 461 },
            { "net46", 460 },
            { "netstandard2.1", 21 },
            { "netstandard2.0", 20 },
            { "netstandard1.6", 16 },
            { "netstandard1.5", 15 },
            { "netstandard1.4", 14 },
            { "netstandard1.3", 13 },
            { "netstandard1.2", 12 },
            { "netstandard1.1", 11 },
            { "netstandard1.0", 10 },
            { "netcoreapp3.1", 31 },
            { "netcoreapp3.0", 30 },
            { "netcoreapp2.2", 22 },
            { "netcoreapp2.1", 21 },
            { "netcoreapp2.0", 20 },
            { "netcoreapp1.1", 11 },
            { "netcoreapp1.0", 10 },
            { "net5.0", 50 },
            { "net6.0", 60 },
            { "net7.0", 70 },
            { "net8.0", 80 },
            { "net9.0", 90 },
            { "net10.0", 100 }
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Discovers all eligible projects in the specified directory.
        /// </summary>
        /// <param name="rootDirectory">The root directory to search.</param>
        /// <param name="specificProject">Optional specific project name to filter by.</param>
        /// <returns>A collection of discovered project information.</returns>
        public List<ProjectInfo> DiscoverProjects(string rootDirectory, string specificProject = null)
        {
            Ensure.ArgumentNotNull(rootDirectory, nameof(rootDirectory));

            if (!Directory.Exists(rootDirectory))
            {
                throw new DirectoryNotFoundException($"Directory not found: {rootDirectory}");
            }

            var projects = new List<ProjectInfo>();

            // First, try to find a solution file
            var solutionFiles = Directory.GetFiles(rootDirectory, "*.sln", SearchOption.TopDirectoryOnly);
            
            if (solutionFiles.Length > 0)
            {
                // Parse solution file to get projects
                foreach (var solutionFile in solutionFiles)
                {
                    var solutionProjects = ParseSolutionFile(solutionFile);
                    projects.AddRange(solutionProjects);
                }
            }
            else
            {
                // No solution file found, search for project files recursively
                var projectFiles = Directory.GetFiles(rootDirectory, "*.csproj", SearchOption.AllDirectories);
                
                foreach (var projectFile in projectFiles)
                {
                    var projectInfo = AnalyzeProject(projectFile);
                    if (projectInfo is not null)
                    {
                        projects.Add(projectInfo);
                    }
                }
            }

            // Filter by specific project if requested
            if (!string.IsNullOrWhiteSpace(specificProject))
            {
                projects = projects.Where(p => 
                    p.ProjectName.Equals(specificProject, StringComparison.OrdinalIgnoreCase) ||
                    p.AssemblyName.Equals(specificProject, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Filter out ineligible projects and sort by name
            return projects
                .Where(p => p.ShouldIncludeInDocumentation())
                .OrderBy(p => p.ProjectName)
                .ToList();
        }

        /// <summary>
        /// Analyzes a single project file to extract project information.
        /// </summary>
        /// <param name="projectPath">The path to the project file.</param>
        /// <returns>The project information, or null if the project cannot be analyzed.</returns>
        public ProjectInfo AnalyzeProject(string projectPath)
        {
            Ensure.ArgumentNotNull(projectPath, nameof(projectPath));

            if (!File.Exists(projectPath))
            {
                return null;
            }

            try
            {
                var projectInfo = new ProjectInfo(projectPath);
                var projectXml = XDocument.Load(projectPath);

                // Parse basic project properties
                ParseProjectProperties(projectXml, projectInfo);

                // Parse target frameworks
                ParseTargetFrameworks(projectXml, projectInfo);

                // Determine latest target framework
                DetermineLatestTargetFramework(projectInfo);

                // Check if project generates documentation
                CheckDocumentationGeneration(projectXml, projectInfo);

                // Classify project type
                ClassifyProject(projectInfo);

                return projectInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Warning: Could not analyze project {projectPath}: {ex.Message}");
                if (ex.Message.Contains("filePath specified does not exist"))
                {
                    Console.WriteLine($"    Full project path: '{projectPath}'");
                    Console.WriteLine($"    Path exists: {File.Exists(projectPath)}");
                    Console.WriteLine($"    Exception type: {ex.GetType().Name}");
                }
                return null;
            }
        }

        /// <summary>
        /// Finds the solution file in the specified directory.
        /// </summary>
        /// <param name="directory">The directory to search.</param>
        /// <returns>The path to the solution file, or null if not found.</returns>
        public string FindSolutionFile(string directory)
        {
            Ensure.ArgumentNotNull(directory, nameof(directory));

            var solutionFiles = Directory.GetFiles(directory, "*.sln", SearchOption.TopDirectoryOnly);
            return solutionFiles.FirstOrDefault();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Parses a Visual Studio solution file to extract project references.
        /// </summary>
        /// <param name="solutionPath">The path to the solution file.</param>
        /// <returns>A list of project information extracted from the solution.</returns>
        private List<ProjectInfo> ParseSolutionFile(string solutionPath)
        {
            var projects = new List<ProjectInfo>();
            var solutionDirectory = Path.GetDirectoryName(solutionPath);

            try
            {
                var solutionContent = File.ReadAllText(solutionPath);
                var projectMatches = Regex.Matches(solutionContent, 
                    @"Project\(""\{[^}]+\}""\)\s*=\s*""([^""]+)"",\s*""([^""]+)"",\s*""\{[^}]+\}""");

                foreach (Match match in projectMatches)
                {
                    var projectName = match.Groups[1].Value;
                    var projectRelativePath = match.Groups[2].Value;

                    // Skip solution folders
                    if (projectRelativePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                    {
                        var projectFullPath = Path.Combine(solutionDirectory, projectRelativePath);
                        projectFullPath = Path.GetFullPath(projectFullPath);

                        if (File.Exists(projectFullPath))
                        {
                            var projectInfo = AnalyzeProject(projectFullPath);
                            if (projectInfo is not null)
                            {
                                projects.Add(projectInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Warning: Could not parse solution file {solutionPath}: {ex.Message}");
            }

            return projects;
        }

        /// <summary>
        /// Parses basic project properties from the project XML document.
        /// </summary>
        /// <param name="projectXml">The parsed project XML document.</param>
        /// <param name="projectInfo">The project info object to populate.</param>
        private void ParseProjectProperties(XDocument projectXml, ProjectInfo projectInfo)
        {
            var propertyGroups = projectXml.Root?.Elements("PropertyGroup");
            if (propertyGroups is null) return;

            foreach (var propertyGroup in propertyGroups)
            {
                // Get assembly name
                var assemblyNameElement = propertyGroup.Element("AssemblyName");
                if (assemblyNameElement is not null && string.IsNullOrWhiteSpace(projectInfo.AssemblyName))
                {
                    projectInfo.AssemblyName = assemblyNameElement.Value;
                }
            }

            // Default assembly name to project name if not specified
            if (string.IsNullOrWhiteSpace(projectInfo.AssemblyName))
            {
                projectInfo.AssemblyName = projectInfo.ProjectName;
            }
        }

        /// <summary>
        /// Parses target framework information from the project XML document.
        /// </summary>
        /// <param name="projectXml">The parsed project XML document.</param>
        /// <param name="projectInfo">The project info object to populate.</param>
        private void ParseTargetFrameworks(XDocument projectXml, ProjectInfo projectInfo)
        {
            var propertyGroups = projectXml.Root?.Elements("PropertyGroup");
            if (propertyGroups is null) return;

            foreach (var propertyGroup in propertyGroups)
            {
                // Check for single target framework
                var targetFrameworkElement = propertyGroup.Element("TargetFramework");
                if (targetFrameworkElement is not null)
                {
                    projectInfo.TargetFrameworks.Add(targetFrameworkElement.Value);
                }

                // Check for multiple target frameworks
                var targetFrameworksElement = propertyGroup.Element("TargetFrameworks");
                if (targetFrameworksElement is not null)
                {
                    var frameworks = targetFrameworksElement.Value.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    projectInfo.TargetFrameworks.AddRange(frameworks.Select(f => f.Trim()));
                }
            }

            // Remove duplicates and sort
            projectInfo.TargetFrameworks = projectInfo.TargetFrameworks
                .Distinct()
                .OrderBy(GetFrameworkSortOrder)
                .ToList();
        }

        private void DetermineLatestTargetFramework(ProjectInfo projectInfo)
        {
            if (projectInfo.TargetFrameworks.Count == 0)
            {
                return;
            }

            // Find the highest version framework
            projectInfo.LatestTargetFramework = projectInfo.TargetFrameworks
                .OrderByDescending(GetFrameworkVersion)
                .First();
        }

        private void CheckDocumentationGeneration(XDocument projectXml, ProjectInfo projectInfo)
        {
            var propertyGroups = projectXml.Root?.Elements("PropertyGroup");
            if (propertyGroups is null) return;

            foreach (var propertyGroup in propertyGroups)
            {
                // Check GenerateDocumentationFile
                var generateDocElement = propertyGroup.Element("GenerateDocumentationFile");
                if (generateDocElement is not null && 
                    bool.TryParse(generateDocElement.Value, out var generateDoc) && 
                    generateDoc)
                {
                    projectInfo.GeneratesDocumentation = true;
                }

                // Check DocumentationFile path - only use if it's a simple path without MSBuild properties
                var docFileElement = propertyGroup.Element("DocumentationFile");
                if (docFileElement is not null && !string.IsNullOrWhiteSpace(docFileElement.Value))
                {
                    var docValue = docFileElement.Value.Trim();
                    // Only use if it doesn't contain MSBuild property references
                    if (!docValue.Contains("$("))
                    {
                        projectInfo.DocumentationFile = docValue;
                        projectInfo.GeneratesDocumentation = true;
                    }
                }
            }

            // Don't set a default DocumentationFile - we'll detect it at runtime
            // This allows us to check for documentation files even when GenerateDocumentationFile
            // is set in Directory.Build.props or other imported files
        }

        private void ClassifyProject(ProjectInfo projectInfo)
        {
            var projectNameLower = projectInfo.ProjectName.ToLowerInvariant();
            var projectPathLower = projectInfo.ProjectPath.ToLowerInvariant();

            // Check for test project
            projectInfo.IsTestProject = TestProjectIndicators.Any(indicator => 
                projectNameLower.Contains(indicator) || projectPathLower.Contains(indicator));

            // Check for template project
            projectInfo.IsTemplateProject = TemplateProjectIndicators.Any(indicator =>
                projectNameLower.Contains(indicator) || projectPathLower.Contains(indicator));

            // Check for tool project
            projectInfo.IsToolProject = ToolProjectIndicators.Any(indicator =>
                projectNameLower.Contains(indicator) || projectPathLower.Contains(indicator));

            // Additional checks for project types
            if (projectNameLower.EndsWith(".tests") || projectNameLower.EndsWith(".test"))
            {
                projectInfo.IsTestProject = true;
            }

            if (projectPathLower.Contains("templates") || projectPathLower.Contains("scaffolding"))
            {
                projectInfo.IsTemplateProject = true;
            }
        }

        private int GetFrameworkVersion(string framework)
        {
            if (string.IsNullOrWhiteSpace(framework))
            {
                return 0;
            }

            var normalizedFramework = framework.ToLowerInvariant();
            
            if (FrameworkVersions.TryGetValue(normalizedFramework, out var version))
            {
                return version;
            }

            // Try to extract version from framework string
            if (normalizedFramework.StartsWith("net") && !normalizedFramework.StartsWith("netstandard") && !normalizedFramework.StartsWith("netcoreapp"))
            {
                var versionPart = normalizedFramework.Substring(3);
                if (double.TryParse(versionPart, out var netVersion))
                {
                    return (int)(netVersion * 10);
                }
            }

            return 0;
        }

        private int GetFrameworkSortOrder(string framework)
        {
            // Sort by version, with newer frameworks first
            return -GetFrameworkVersion(framework);
        }

        #endregion

    }

}