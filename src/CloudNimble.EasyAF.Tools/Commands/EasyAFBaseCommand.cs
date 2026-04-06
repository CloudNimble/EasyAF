using CloudNimble.EasyAF.MSBuild;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tools.Commands
{

    /// <summary>
    /// Base class for EasyAF commands that provides common functionality for MSBuild operations, user secrets management, and project configuration.
    /// </summary>
    public abstract class EasyAFBaseCommand
    {

        #region Protected Methods

        /// <summary>
        /// Ensures MSBuild is registered with the latest available version.
        /// </summary>
        protected static void CheckMSBuildRegistered()
        {
            MSBuildProjectManager.EnsureMSBuildRegistered();
        }

        /// <summary>
        /// Extracts the UserSecretsId from a project file using MSBuild evaluation.
        /// This will properly evaluate the project with all imports including Directory.Build.props.
        /// </summary>
        /// <param name="projectFilePath">The path to the project file.</param>
        /// <returns>The UserSecretsId if found, otherwise null.</returns>
        protected static string ExtractUserSecretsId(string projectFilePath)
        {
            try
            {
                // Register MSBuild if not already registered
                CheckMSBuildRegistered();
                
                // Use MSBuild APIs to properly evaluate the project with all imports (including Directory.Build.props)
                var project = new Project(projectFilePath);
                var userSecretsId = project.GetPropertyValue("UserSecretsId");
                
                // Clean up the project to avoid memory leaks
                ProjectCollection.GlobalProjectCollection.UnloadProject(project);
                
                return string.IsNullOrWhiteSpace(userSecretsId) ? null : userSecretsId;
            }
            catch
            {
                // If MSBuild evaluation fails, return null to indicate no UserSecretsId found
                return null;
            }
        }

        /// <summary>
        /// Extracts the UserSecretsId from the data project folder.
        /// </summary>
        /// <param name="dataFolder">The data project folder path.</param>
        /// <returns>The UserSecretsId if found, otherwise null.</returns>
        protected static string ExtractUserSecretsIdFromDataProject(string dataFolder)
        {
            var projectFiles = Directory.GetFiles(dataFolder, "*.csproj");
            if (projectFiles.Length == 0)
            {
                return null;
            }

            return ExtractUserSecretsId(projectFiles[0]);
        }

        /// <summary>
        /// Extracts the UserSecretsId from Directory.Build.props in the current directory.
        /// </summary>
        /// <returns>The UserSecretsId if found, otherwise null.</returns>
        protected static string ExtractUserSecretsIdFromDirectoryBuildProps()
        {
            var directoryBuildPropsPath = Path.Combine(Environment.CurrentDirectory, "Directory.Build.props");
            if (!File.Exists(directoryBuildPropsPath))
            {
                return null;
            }

            try
            {
                // Register MSBuild if not already registered
                CheckMSBuildRegistered();
                
                var project = new Project(directoryBuildPropsPath);
                var userSecretsId = project.GetPropertyValue("UserSecretsId");
                
                // Clean up
                ProjectCollection.GlobalProjectCollection.UnloadProject(project);
                
                return string.IsNullOrWhiteSpace(userSecretsId) ? null : userSecretsId;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Sets a user secret value using the official dotnet user-secrets CLI tool.
        /// </summary>
        /// <param name="userSecretsId">The user secrets ID.</param>
        /// <param name="key">The secret key.</param>
        /// <param name="value">The secret value.</param>
        /// <param name="projectPath">The project directory path.</param>
        protected static async Task SetUserSecretAsync(string userSecretsId, string key, string value, string projectPath)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"user-secrets set \"{key}\" \"{value}\"",
                WorkingDirectory = projectPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process is null)
            {
                throw new InvalidOperationException("Failed to start dotnet user-secrets process.");
            }

            await process.WaitForExitAsync();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Failed to set user secret '{key}'. Exit code: {process.ExitCode}. " +
                    $"Error: {error}. Output: {output}"
                );
            }
        }

        /// <summary>
        /// Determines the EasyAFProjectType based on the project file name and content.
        /// </summary>
        /// <param name="projectFilePath">The path to the project file.</param>
        /// <returns>The determined project type, or null if no supported type is detected.</returns>
        protected static string DetermineProjectType(string projectFilePath)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectFilePath);
            
            // Exclude test projects first
            if (projectName.Contains(".Test", StringComparison.OrdinalIgnoreCase) || 
                projectName.Contains(".Tests", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            
            // Check for supported project types based on naming conventions
            if (projectName.Contains(".Data.", StringComparison.OrdinalIgnoreCase) || 
                projectName.EndsWith(".Data", StringComparison.OrdinalIgnoreCase))
            {
                return "Data";
            }
            
            if (projectName.Contains(".Business.", StringComparison.OrdinalIgnoreCase) || 
                projectName.EndsWith(".Business", StringComparison.OrdinalIgnoreCase))
            {
                return "Business";
            }
            
            if (projectName.Contains(".Api.", StringComparison.OrdinalIgnoreCase) || 
                projectName.EndsWith(".Api", StringComparison.OrdinalIgnoreCase))
            {
                return "Api";
            }
            
            if (projectName.Contains(".SimpleMessageBus.", StringComparison.OrdinalIgnoreCase) || 
                projectName.EndsWith(".SimpleMessageBus", StringComparison.OrdinalIgnoreCase) ||
                projectName.Contains(".MessageBus.", StringComparison.OrdinalIgnoreCase) || 
                projectName.EndsWith(".MessageBus", StringComparison.OrdinalIgnoreCase) ||
                projectName.Contains(".EventBus.", StringComparison.OrdinalIgnoreCase) || 
                projectName.EndsWith(".EventBus", StringComparison.OrdinalIgnoreCase))
            {
                return "SimpleMessageBus";
            }
            
            if (projectName.Contains(".Core.", StringComparison.OrdinalIgnoreCase) || 
                projectName.EndsWith(".Core", StringComparison.OrdinalIgnoreCase))
            {
                return "Core";
            }
            
            // No supported project type detected
            return null;
        }

        /// <summary>
        /// Adds or updates the EasyAFProjectType property in a project file using MSBuildProjectManager.
        /// </summary>
        /// <param name="projectFilePath">The path to the project file.</param>
        /// <param name="projectType">The project type to set.</param>
        protected static void SetProjectType(string projectFilePath, string projectType)
        {
            try
            {
                var manager = new MSBuildProjectManager();
                manager.Load(projectFilePath, preserveFormatting: true)
                       .SetEasyAFProjectType(projectType)
                       .Save();
                
                Console.WriteLine($"Set EasyAFProjectType to '{projectType}' in {Path.GetFileName(projectFilePath)}");
                
                // Report any warnings from the manager
                foreach (var error in manager.ProjectErrors.Where(e => e.IsWarning))
                {
                    Console.WriteLine($"Warning: {error.ErrorText}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to set EasyAFProjectType in {Path.GetFileName(projectFilePath)}: {ex.Message}");
            }
        }

        /// <summary>
        /// Detects the common namespace from existing projects.
        /// </summary>
        /// <param name="projectFiles">Array of project file paths.</param>
        /// <returns>The detected common namespace, or null if none found.</returns>
        protected static string DetectCommonNamespace(string[] projectFiles)
        {
            var namespaces = new List<string>();
            
            foreach (var projectFile in projectFiles)
            {
                try
                {
                    // Register MSBuild if not already registered
                    CheckMSBuildRegistered();
                    
                    var project = new Project(projectFile);
                    
                    // Try to get RootNamespace first, then AssemblyName
                    var rootNamespace = project.GetPropertyValue("RootNamespace");
                    if (!string.IsNullOrWhiteSpace(rootNamespace))
                    {
                        namespaces.Add(rootNamespace);
                    }
                    else
                    {
                        var assemblyName = project.GetPropertyValue("AssemblyName");
                        if (!string.IsNullOrWhiteSpace(assemblyName))
                        {
                            namespaces.Add(assemblyName);
                        }
                    }
                    
                    ProjectCollection.GlobalProjectCollection.UnloadProject(project);
                }
                catch
                {
                    // If we can't read a project, try to infer from the file name
                    var projectName = Path.GetFileNameWithoutExtension(projectFile);
                    if (!string.IsNullOrWhiteSpace(projectName))
                    {
                        namespaces.Add(projectName);
                    }
                }
            }
            
            if (namespaces.Count == 0)
            {
                return null;
            }
            
            // Find the common prefix among all namespaces
            var commonNamespace = FindCommonPrefix(namespaces);
            
            // Remove trailing dots and common suffixes like .Data, .Business, etc.
            commonNamespace = commonNamespace.TrimEnd('.');
            
            // Remove common project type suffixes to get the base namespace
            var suffixesToRemove = new[] { ".Data", ".Business", ".Api", ".Core", ".SimpleMessageBus", ".MessageBus", ".EventBus", ".Tests", ".Test" };
            foreach (var suffix in suffixesToRemove)
            {
                if (commonNamespace.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    commonNamespace = commonNamespace.Substring(0, commonNamespace.Length - suffix.Length);
                    break;
                }
            }
            
            return string.IsNullOrWhiteSpace(commonNamespace) ? null : commonNamespace;
        }

        /// <summary>
        /// Finds the common prefix among a list of strings.
        /// </summary>
        /// <param name="strings">The list of strings to find common prefix for.</param>
        /// <returns>The common prefix.</returns>
        protected static string FindCommonPrefix(List<string> strings)
        {
            if (strings.Count == 0)
            {
                return string.Empty;
            }
            
            if (strings.Count == 1)
            {
                return strings[0];
            }
            
            var firstString = strings[0];
            var commonPrefix = string.Empty;
            
            for (int i = 0; i < firstString.Length; i++)
            {
                var currentChar = firstString[i];
                var isCommon = true;
                
                foreach (var str in strings.Skip(1))
                {
                    if (i >= str.Length || str[i] != currentChar)
                    {
                        isCommon = false;
                        break;
                    }
                }
                
                if (isCommon)
                {
                    commonPrefix += currentChar;
                }
                else
                {
                    break;
                }
            }
            
            return commonPrefix;
        }

        /// <summary>
        /// Discovers and configures project types and namespace for all projects in the current directory and subdirectories.
        /// </summary>
        /// <param name="userSecretsId">The UserSecretsId to set in Directory.Build.props.</param>
        protected static void ConfigureProjectTypes(string userSecretsId)
        {
            var projectFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.csproj", SearchOption.AllDirectories);
            
            if (projectFiles.Length == 0)
            {
                Console.WriteLine("No .csproj files found in the current directory or subdirectories.");
                return;
            }
            
            Console.WriteLine($"Found {projectFiles.Length} project(s). Analyzing project types...");
            
            // Detect common namespace
            var commonNamespace = DetectCommonNamespace(projectFiles);
            if (!string.IsNullOrEmpty(commonNamespace))
            {
                Console.WriteLine($"Detected common namespace: {commonNamespace}");
            }
            
            // Configure project types
            foreach (var projectFile in projectFiles)
            {
                var projectType = DetermineProjectType(projectFile);
                if (!string.IsNullOrEmpty(projectType))
                {
                    SetProjectType(projectFile, projectType);
                }
                else
                {
                    Console.WriteLine($"No supported EasyAF project type detected for {Path.GetFileName(projectFile)}");
                }
            }
            
            // Configure Directory.Build.props
            if (!string.IsNullOrEmpty(commonNamespace))
            {
                ConfigureDirectoryBuildProps(commonNamespace, userSecretsId, projectFiles);
            }
        }

        /// <summary>
        /// Configures Directory.Build.props with EasyAF namespace, UserSecretsId, and analyzer references using MSBuildProjectManager.
        /// </summary>
        /// <param name="commonNamespace">The common namespace to set.</param>
        /// <param name="userSecretsId">The UserSecretsId to set.</param>
        /// <param name="projectFiles">Array of project file paths.</param>
        protected static void ConfigureDirectoryBuildProps(string commonNamespace, string userSecretsId, string[] projectFiles)
        {
            try
            {
                var directoryBuildPropsPath = Path.Combine(Environment.CurrentDirectory, "Directory.Build.props");
                
                // Find the Data project for .edmx file references
                var dataProject = projectFiles.FirstOrDefault(p => DetermineProjectType(p) == "Data");
                var dataProjectRelativePath = dataProject is not null 
                    ? Path.GetRelativePath(Environment.CurrentDirectory, Path.GetDirectoryName(dataProject))
                    : null;
                
                Console.WriteLine($"Configuring Directory.Build.props...");
                
                MSBuildProjectManager manager;
                
                if (File.Exists(directoryBuildPropsPath))
                {
                    // Load existing Directory.Build.props with formatting preservation
                    Console.WriteLine("Updating existing Directory.Build.props");
                    manager = new MSBuildProjectManager();
                    manager.Load(directoryBuildPropsPath, preserveFormatting: true);
                }
                else
                {
                    // Create new Directory.Build.props
                    Console.WriteLine("Creating new Directory.Build.props");
                    manager = MSBuildProjectManager.CreateDirectoryBuildProps(
                        directoryBuildPropsPath, 
                        commonNamespace, 
                        userSecretsId, 
                        dataProjectRelativePath
                    );
                }
                
                // For existing files, update the properties and add analyzers if needed
                if (File.Exists(directoryBuildPropsPath))
                {
                    manager.SetEasyAFNamespace(commonNamespace)
                           .SetUserSecretsId(userSecretsId);
                    
                    // Check if EasyAF analyzers already exist
                    var hasAnalyzers = manager.Project.ItemGroups
                        .Any(ig => ig.Items.Any(item => 
                            item.ItemType == "PackageReference" && 
                            item.Include == "EasyAF.Analyzers.EF6"));
                            
                    if (!hasAnalyzers)
                    {
                        manager.AddEasyAFAnalyzers(dataProjectRelativePath);
                        Console.WriteLine("Added EasyAF analyzer ItemGroup");
                    }
                    else
                    {
                        Console.WriteLine("EasyAF analyzer ItemGroup already exists");
                    }
                }
                
                // Save the project
                manager.Save();
                
                Console.WriteLine($"Set EasyAFNamespace to '{commonNamespace}'");
                Console.WriteLine($"Set UserSecretsId to '{userSecretsId}'");
                Console.WriteLine($"Directory.Build.props configured successfully at: {directoryBuildPropsPath}");
                
                // Report any warnings from the manager
                foreach (var error in manager.ProjectErrors.Where(e => e.IsWarning))
                {
                    Console.WriteLine($"Warning: {error.ErrorText}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to configure Directory.Build.props: {ex.Message}");
            }
        }


        #endregion

    }

}
