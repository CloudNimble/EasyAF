using CloudNimble.EasyAF.MSBuild;
using Microsoft.Build.Evaluation;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json;

namespace CloudNimble.EasyAF.EFCoreToEdmx
{

    /// <summary>
    /// Resolves database connection strings from various configuration sources.
    /// </summary>
    /// <remarks>
    /// This class handles finding and extracting connection strings from configuration files,
    /// user secrets, and other supported sources. It supports the connection string source
    /// format used in EDMX configuration files.
    /// </remarks>
    public class ConnectionStringResolver
    {

        /// <summary>
        /// Resolves a connection string from the specified source.
        /// </summary>
        /// <param name="connectionStringSource">
        /// The connection string source in the format "filename:section:key" 
        /// (e.g., "appsettings.json:ConnectionStrings:DefaultConnection").
        /// </param>
        /// <param name="projectPath">The path to the project directory containing configuration files.</param>
        /// <returns>The resolved connection string.</returns>
        /// <exception cref="ArgumentException">Thrown when parameters are null, empty, or in invalid format.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the specified configuration file is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection string cannot be found in the specified location.</exception>
        /// <remarks>
        /// This method supports the following configuration sources:
        /// - JSON files (appsettings.json, appsettings.Development.json, etc.)
        /// - User secrets (when filename is "secrets" or "user-secrets")
        /// - Environment variables (when filename is "environment")
        /// 
        /// The source format is "filename:section:key" where:
        /// - filename: The configuration file name or special source identifier
        /// - section: The configuration section (can be nested with colons)
        /// - key: The specific configuration key containing the connection string
        /// </remarks>
        /// <example>
        /// <code>
        /// var resolver = new ConnectionStringResolver();
        /// var connectionString = resolver.ResolveConnectionString(
        ///     "appsettings.json:ConnectionStrings:DefaultConnection", 
        ///     @"C:\MyProject"
        /// );
        /// </code>
        /// </example>
        public string ResolveConnectionString(string connectionStringSource, string projectPath)
        {

            ArgumentException.ThrowIfNullOrWhiteSpace(connectionStringSource, nameof(connectionStringSource));
            ArgumentException.ThrowIfNullOrWhiteSpace(projectPath, nameof(projectPath));
            MSBuildProjectManager.EnsureMSBuildRegistered();

            var parts = connectionStringSource.Split(':', 3);
            if (parts.Length != 3)
            {

                throw new ArgumentException(
                    "Connection string source must be in the format 'filename:section:key'. " +
                    $"Received: {connectionStringSource}",
                    nameof(connectionStringSource)
                );

            }

            var filename = parts[0];
            var section = parts[1];
            var key = parts[2];

            return filename.ToLowerInvariant() switch
            {

                "secrets" or "user-secrets" => ResolveFromUserSecrets(section, key, projectPath),
                "environment" => ResolveFromEnvironment(section, key),
                _ => ResolveFromConfigurationFile(filename, section, key, projectPath)

            };

        }

        /// <summary>
        /// Resolves a connection string from a JSON configuration file.
        /// </summary>
        /// <param name="filename">The name of the configuration file.</param>
        /// <param name="section">The configuration section.</param>
        /// <param name="key">The configuration key.</param>
        /// <param name="projectPath">The project directory path.</param>
        /// <returns>The resolved connection string.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the configuration file is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection string is not found.</exception>
        /// <remarks>
        /// This method uses the .NET configuration system to load JSON files and resolve
        /// hierarchical configuration paths. It supports nested sections using colon notation.
        /// </remarks>
        private string ResolveFromConfigurationFile(string filename, string section, string key, string projectPath)
        {

            var configPath = Path.Combine(projectPath, filename);
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Configuration file not found: {configPath}");

            var builder = new ConfigurationBuilder()
                .SetBasePath(projectPath)
                .AddJsonFile(filename, optional: false);

            var configuration = builder.Build();
            var connectionString = configuration[$"{section}:{key}"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {

                throw new InvalidOperationException(
                    $"Connection string not found at '{section}:{key}' in {filename}. " +
                    "Please verify the configuration path is correct."
                );

            }

            return connectionString;

        }

        /// <summary>
        /// Resolves a connection string from user secrets.
        /// </summary>
        /// <param name="section">The configuration section.</param>
        /// <param name="key">The configuration key.</param>
        /// <param name="projectPath">The project directory path.</param>
        /// <returns>The resolved connection string.</returns>
        /// <exception cref="InvalidOperationException">Thrown when user secrets are not configured or the connection string is not found.</exception>
        /// <remarks>
        /// This method loads user secrets for the project and attempts to find the connection
        /// string in the specified section and key. The project must have user secrets initialized.
        /// </remarks>
        private string ResolveFromUserSecrets(string section, string key, string projectPath)
        {

            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(projectPath)
                    .AddUserSecrets(GetUserSecretsId(projectPath));

                var configuration = builder.Build();
                var connectionString = configuration[$"{section}:{key}"];

                if (string.IsNullOrWhiteSpace(connectionString))
                {

                    throw new InvalidOperationException(
                        $"Connection string not found at '{section}:{key}' in user secrets. " +
                        "Please add the connection string to user secrets using 'dotnet user-secrets set'."
                    );

                }

                return connectionString;

            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("could not be found"))
            {

                throw new InvalidOperationException(
                    "User secrets are not configured for this project. " +
                    "Please initialize user secrets using 'dotnet user-secrets init'.",
                    ex
                );

            }

        }

        /// <summary>
        /// Resolves a connection string from environment variables.
        /// </summary>
        /// <param name="section">The configuration section (used as environment variable prefix).</param>
        /// <param name="key">The configuration key (used as environment variable suffix).</param>
        /// <returns>The resolved connection string.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the environment variable is not found.</exception>
        /// <remarks>
        /// This method looks for environment variables using both standard naming conventions:
        /// - SECTION__KEY (double underscore, standard .NET configuration format)
        /// - SECTION_KEY (single underscore, common environment variable format)
        /// </remarks>
        private static string ResolveFromEnvironment(string section, string key)
        {

            // Try the standard .NET configuration format first (double underscore)
            var envVarName = $"{section}__{key}";
            var connectionString = Environment.GetEnvironmentVariable(envVarName);

            // Fall back to single underscore format
            if (string.IsNullOrWhiteSpace(connectionString))
            {

                envVarName = $"{section}_{key}";
                connectionString = Environment.GetEnvironmentVariable(envVarName);

            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {

                throw new InvalidOperationException(
                    $"Connection string not found in environment variables. " +
                    $"Please set either '{section}__{key}' or '{section}_{key}' environment variable."
                );

            }

            return connectionString;

        }

        /// <summary>
        /// Gets the user secrets ID for the specified project using MSBuild evaluation.
        /// </summary>
        /// <param name="projectPath">The project directory path.</param>
        /// <returns>The user secrets ID.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the user secrets ID cannot be found.</exception>
        /// <remarks>
        /// This method uses MSBuild evaluation to find the UserSecretsId property, which allows it
        /// to resolve the ID from Directory.Build.props files and other MSBuild imports.
        /// </remarks>
        private static string GetUserSecretsId(string projectPath)
        {

            var projectFiles = Directory.GetFiles(projectPath, "*.csproj");
            if (projectFiles.Length == 0)
                throw new InvalidOperationException($"No .csproj file found in {projectPath}");

            var projectFilePath = projectFiles[0];

            try
            {
                // Ensure MSBuild is registered
                MSBuildProjectManager.EnsureMSBuildRegistered();

                // Use MSBuild APIs to properly evaluate the project with all imports (including Directory.Build.props)
                var project = new Project(projectFilePath);
                var userSecretsId = project.GetPropertyValue("UserSecretsId");
                
                // Clean up the project to avoid memory leaks
                ProjectCollection.GlobalProjectCollection.UnloadProject(project);
                
                if (string.IsNullOrWhiteSpace(userSecretsId))
                {
                    throw new InvalidOperationException(
                        "UserSecretsId not found in project file or Directory.Build.props. " +
                        "Please initialize user secrets using 'dotnet user-secrets init' or ensure the UserSecretsId property is set in Directory.Build.props."
                    );
                }

                return userSecretsId;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                // If MSBuild evaluation fails, fall back to the original string parsing method
                // This provides backward compatibility in case there are MSBuild issues
                return GetUserSecretsIdFallback(projectFilePath);
            }

        }

        /// <summary>
        /// Fallback method to get UserSecretsId by parsing the project file directly.
        /// </summary>
        /// <param name="projectFilePath">The path to the project file.</param>
        /// <returns>The user secrets ID.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the user secrets ID cannot be found.</exception>
        /// <remarks>
        /// This is a fallback method that uses simple string parsing. It won't find UserSecretsId
        /// defined in Directory.Build.props, but provides compatibility if MSBuild evaluation fails.
        /// </remarks>
        private static string GetUserSecretsIdFallback(string projectFilePath)
        {
            var projectContent = File.ReadAllText(projectFilePath);

            // Look for <UserSecretsId> in the project file
            var startTag = "<UserSecretsId>";
            var endTag = "</UserSecretsId>";
            var startIndex = projectContent.IndexOf(startTag);
            
            if (startIndex == -1)
            {
                throw new InvalidOperationException(
                    "UserSecretsId not found in project file. " +
                    "Please initialize user secrets using 'dotnet user-secrets init' or ensure the UserSecretsId property is set in Directory.Build.props."
                );
            }

            startIndex += startTag.Length;
            var endIndex = projectContent.IndexOf(endTag, startIndex);
            
            if (endIndex == -1)
                throw new InvalidOperationException("Malformed UserSecretsId in project file.");

            return projectContent.Substring(startIndex, endIndex - startIndex).Trim();
        }

    }

}
