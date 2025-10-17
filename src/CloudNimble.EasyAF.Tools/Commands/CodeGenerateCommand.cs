using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Generators.Core;
using CloudNimble.EasyAF.MSBuild;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tools.Commands
{

    /// <summary>
    /// Represents a command for generating code for a specified EasyAF component.
    /// </summary>
    /// <remarks>
    /// This command is used within the EasyAF tooling to automate the generation of code for various components,
    /// such as business logic, core libraries, data access, APIs, or all components at once.
    /// </remarks>
    /// <example>
    /// <code>
    /// dotnet easyaf generate business -path "C:\Projects\MyApp" -dontdelete "Controllers\Public" -notpublic "User,Role"
    /// </code>
    /// </example>
    [Command(Name = "generate", Description = "Run code generation for a given EasyAF component.")]
    public class CodeGenerateCommand
    {

        #region Properties

        /// <summary>
        /// Gets or sets the component to generate.
        /// Available options: business, core, data, api, simplemessagebus, all.
        /// </summary>
        [Argument(0, Description = "The component to generate. Available options: business, core, data, api, simplemessagebus, all")]
        [Required]
        public string Component { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the working directory for the code compiler.
        /// Defaults to the current directory if not specified.
        /// </summary>
        [Option("-p|--path <path>", Description = "Working directory for the code compiler. Defaults to current directory.")]
        public string Root { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a directory that will be ignored when deleting files during code generation.
        /// </summary>
        [Option("-dontdelete <path>", Description = "A directory that will be ignored when deleting files.")]
        public string DontDelete { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a comma-separated list of table names to ignore when generating the public API surface.
        /// </summary>
        [Option("-notpublic <path>", Description = "A comma-separated list of table names to ignore when generating the public API surface.")]
        public string NotPublic { get; set; } = string.Empty;

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the code generation command asynchronously.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a result of 0 on success.
        /// </returns>
        public Task<int> OnExecuteAsync()
        {
            Console.WriteLine("Hello. Welcome to EasyAF.");
            Console.WriteLine($"You've chosen to generate C# for the '{Component}' component.");

            var rootFolder = !string.IsNullOrWhiteSpace(Root) ? Root : Directory.GetCurrentDirectory();
            var pathToIgnore = !string.IsNullOrWhiteSpace(DontDelete) ? DontDelete : @"Controllers\Public\";

            Console.WriteLine($"Generating code in the directory: {rootFolder}");
            Console.WriteLine($"Ignoring path: {pathToIgnore}");

            Generate(rootFolder, Component, pathToIgnore);
            Console.WriteLine("EasyAF code generation has completed.");
            return Task.FromResult(0);
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Configuration for API controller generation.
        /// </summary>
        private class ApiGeneratorConfig
        {
            public bool ApiInheritance { get; set; } = true;
            public bool AdminApiInheritance { get; set; } = true;
            public string ApiBaseClass { get; set; } = CodeGenConstants.ApiBaseClassName;
            public string AdminApiBaseClass { get; set; } = CodeGenConstants.ApiBaseClassName;
            public List<string> ApiAdditionalUsings { get; set; } = new();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder">The folder to clean.</param>
        /// <param name="filesToKeep">A list of files that should not be deleted.</param>
        /// <param name="folderToIgnore">A folder that will be ignored during the clean-up.</param>
        private static void CleanOtherFiles(string folder, List<string> filesToKeep, string folderToIgnore)
        {
            foreach (var file in Directory.GetFiles(folder, "*.Generated.cs", SearchOption.AllDirectories).Where(c => !filesToKeep.Contains(c) && !c.Contains(folderToIgnore)))
            {
                File.Delete(file);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="pathToIgnore"></param>
        internal static void Generate(string path, string type, string pathToIgnore)
        {
            type = type.ToLower();
            var projects = Directory.GetDirectories(path);
            var notTests = projects.Where(c => !c.ToLower().Contains(".tests.")).OrderBy(c => c.Length);

            var entityFolder = notTests.FirstOrDefault(c => c.EndsWith(".Core"));
            var dataFolder = notTests.FirstOrDefault(c => c.EndsWith(".Data"));
            var businessFolder = notTests.FirstOrDefault(c => c.EndsWith(".Business"));
            var apiFolder = notTests.FirstOrDefault(c => c.EndsWith(".RestService") || c.EndsWith(".RestServices") || c.EndsWith(".Api") || c.EndsWith(".Api2"));
            var simpleMessageBusFolder = notTests.FirstOrDefault(c => c.EndsWith(".SimpleMessageBus") || c.EndsWith(".MessageBus") || c.EndsWith(".EventBus"));

            //RWM: Expanded folder validation. We should probably unit test some of this stuff more deeply.
            var mustExit = dataFolder is null;
            switch (type)
            {
                case "core":
                    mustExit = mustExit || entityFolder is null;
                    break;
                case "business":
                    mustExit = mustExit || businessFolder is null;
                    break;
                case "api":
                    mustExit = mustExit || apiFolder is null;
                    break;
                case "simplemessagebus":
                    mustExit = mustExit || simpleMessageBusFolder is null || entityFolder is null;
                    break;
                case "all":
                    mustExit = mustExit || entityFolder is null || businessFolder is null || apiFolder is null;
                    // Note: SimpleMessageBus is optional for "all" - only generate if folder exists
                    break;
            }

            if (mustExit)
            {
                Console.WriteLine("Unable to generate files because one or more of the required projects were not found in this folder.  You should run 'dotnet new easyaf' in your solution folder first to generate the required projects.");
                return;
            }

            var helpersFolder = Path.Combine(apiFolder, "Helpers");
            if (!Directory.Exists(helpersFolder))
            {
                Directory.CreateDirectory(helpersFolder);
            }

            var modelBuildersFolder = Path.Combine(apiFolder, "ModelBuilders");
            if (!Directory.Exists(modelBuildersFolder))
            {
                Directory.CreateDirectory(modelBuildersFolder);
            }

            var controllerFolder = Path.Combine(apiFolder, "Controllers");
            if (!Directory.Exists(controllerFolder))
            {
                Directory.CreateDirectory(controllerFolder);
            }

            var entityNamespace = GetNamespaceFromFolder(entityFolder);
            var dataNamespace = GetNamespaceFromFolder(dataFolder);
            var businessNamespace = GetNamespaceFromFolder(businessFolder);
            var apiNamespace = GetNamespaceFromFolder(apiFolder);

            var entityGenerators = new List<string> { "core", "business", "api", "simplemessagebus", "all" };
            var isAll = type == "all";

            var edmxFiles = Directory.GetFiles(dataFolder, "*.edmx", SearchOption.AllDirectories);

            var generatedFileNames = new List<string>();

            foreach (var edmx in edmxFiles)
            {
                var edmxLoader = new EdmxLoader(edmx);
                edmxLoader.Load(true);

                if (edmxLoader.EdmxSchemaErrors.Count != 0)
                {
                    Console.WriteLine($"There were errors parsing {edmxLoader.FilePath}. Open the file in the designer and check the error window.");
                    continue;
                }

                // RWM: Let's start with generators that don't loop through Entities.
                if (type == "data" || isAll)
                {
                    using var dbContext = new DbContextPartialGenerator([GetNamespaceFromFolder(entityFolder)], dataNamespace, edmxLoader.EntityContainer, edmxLoader.OnModelCreatingMethod, edmxLoader.FilePath);
                    generatedFileNames.Add(dbContext.WriteFile(dataFolder));
                    Console.WriteLine(generatedFileNames.Last());

                    //using var dbViews = new DbViewGenerator([GetNamespaceFromFolder(entityFolder)], dataNamespace, edmxLoader.EntityContainer, edmxLoader.Mappings);
                    //generatedFileNames.Add(dbViews.WriteFile(dataFolder));
                    //Console.WriteLine(generatedFileNames.Last());
                }

                //if (type == "views" || isAll)
                //{
                //    using var dbViews = new DbViewGenerator([GetNamespaceFromFolder(entityFolder)], dataNamespace, edmxLoader.EntityContainer, edmxLoader.Mappings);
                //    generatedFileNames.Add(dbViews.WriteFile(dataFolder));
                //    Console.WriteLine(generatedFileNames.Last());
                //}

                if (type == "api" || isAll)
                {
                    var apiConfig = GetApiGeneratorConfig(apiFolder);

                    var extraUsings = new List<string>
                    {
                        entityNamespace,
                        dataNamespace,
                        businessNamespace
                    };

                    List<string> extraUsings2 =
                    [
                        apiNamespace,
                        dataNamespace,
                    ];

                    // Create API-specific usings that include additional usings from config
                    var apiExtraUsings = new List<string>(extraUsings);
                    apiExtraUsings.AddRange(apiConfig.ApiAdditionalUsings);

                    using var restierDI = new RestierDependencyGenerator(extraUsings2, apiNamespace, edmxLoader.EntityContainer, edmxLoader.IsEFCore);
                    generatedFileNames.Add(restierDI.WriteFile(Path.Combine(apiFolder, "Extensions")));
                    Console.WriteLine(generatedFileNames.Last());

                    using var authorization = new AuthorizationGenerator(extraUsings, apiNamespace, edmxLoader.EntityContainer);
                    generatedFileNames.Add(authorization.WriteFile(helpersFolder));
                    Console.WriteLine(generatedFileNames.Last());

                    using var modelBuilder = new ModelBuilderGenerator(extraUsings, apiNamespace, edmxLoader.EntityContainer);
                    generatedFileNames.Add(modelBuilder.WriteFile(modelBuildersFolder));
                    Console.WriteLine(generatedFileNames.Last());

                    using var apiController = new ApiControllerGenerator(apiExtraUsings, $"{apiNamespace}.Controllers", edmxLoader.EntityContainer, edmxLoader.IsEFCore, apiConfig.ApiInheritance, apiConfig.ApiBaseClass);
                    generatedFileNames.Add(apiController.WriteFile(controllerFolder));
                    Console.WriteLine(generatedFileNames.Last());

                    using var adminApiController = new AdminApiControllerGenerator(apiExtraUsings, $"{apiNamespace}.Controllers", edmxLoader, edmxLoader.IsEFCore, apiConfig.AdminApiInheritance, apiConfig.AdminApiBaseClass);
                    generatedFileNames.Add(adminApiController.WriteFile(controllerFolder));
                    Console.WriteLine(generatedFileNames.Last());
                }

                if (type == "business" || isAll)
                {
                    using var businessDI = new BusinessDependencyGenerator([businessNamespace], businessNamespace, edmxLoader.EntityContainer);
                    generatedFileNames.Add(businessDI.WriteFile(Path.Combine(businessFolder, "Extensions")));
                    Console.WriteLine(generatedFileNames.Last());
                }

                if ((type == "simplemessagebus" || isAll) && simpleMessageBusFolder is not null)
                {
                    var simpleMessageBusNamespace = GetNamespaceFromFolder(simpleMessageBusFolder);
                    var extraUsings = new List<string>
                    {
                        "System",
                        "System.Collections.Generic",
                        "System.Collections.Concurrent",
                        "CloudNimble.SimpleMessageBus.Core",
                        entityNamespace  // This is the Core namespace where entities live
                    };

                    // Generate base class once
                    if (edmxLoader.Entities.Any())
                    {
                        var firstEntity = edmxLoader.Entities.First();
                        using var baseGenerator = new SimpleMessageBusGenerator(extraUsings, simpleMessageBusNamespace, firstEntity, "Base");
                        generatedFileNames.Add(baseGenerator.WriteFile(simpleMessageBusFolder));
                        Console.WriteLine(generatedFileNames.Last());
                    }
                }

                // RWM: Now lets see if we need to roll through Entities
                if (entityGenerators.Contains(type.ToLower()))
                {
                    foreach (var composition in edmxLoader.Entities.OrderBy(c => c.EntityType.Name))
                    {
                        if (type == "core" || isAll)
                        {
                            using var entities = new EntityGenerator(null, entityNamespace, composition);
                            generatedFileNames.Add(entities.WriteFile(entityFolder));
                            Console.WriteLine(generatedFileNames.Last());
                        }

                        if (type == "business" || isAll)
                        {
                            var extraUsings = new List<string>
                            {
                                GetNamespaceFromFolder(entityFolder),
                                GetNamespaceFromFolder(dataFolder)
                            };
                            using var managers = new ManagerGenerator(extraUsings, businessNamespace, composition, edmxLoader.EntityContainer.Name);
                            generatedFileNames.Add(managers.WriteFile(businessFolder));
                            Console.WriteLine(generatedFileNames.Last());
                        }

                        if (type == "api" || isAll)
                        {
                            var extraUsings = new List<string>
                            {
                                entityNamespace,
                                dataNamespace,
                                businessNamespace
                            };

                            using var interceptor = new InterceptorGenerator(extraUsings, $"{apiNamespace}.Controllers", edmxLoader.EntityContainer, composition);
                            generatedFileNames.Add(interceptor.WriteFile(controllerFolder));
                            Console.WriteLine(generatedFileNames.Last());
                        }

                        if ((type == "simplemessagebus" || isAll) && simpleMessageBusFolder is not null)
                        {
                            var simpleMessageBusNamespace = GetNamespaceFromFolder(simpleMessageBusFolder);
                            var extraUsings = new List<string>
                            {
                                "System",
                                "System.Collections.Generic",
                                "System.Collections.Concurrent",
                                "CloudNimble.SimpleMessageBus.Core",
                                entityNamespace  // This is the Core namespace where entities live
                            };

                            // Generate Created message
                            using var createdGenerator = new SimpleMessageBusGenerator(extraUsings, simpleMessageBusNamespace, composition, "Created");
                            generatedFileNames.Add(createdGenerator.WriteFile(simpleMessageBusFolder));
                            Console.WriteLine(generatedFileNames.Last());

                            // Generate Updated message
                            using var updatedGenerator = new SimpleMessageBusGenerator(extraUsings, simpleMessageBusNamespace, composition, "Updated");
                            generatedFileNames.Add(updatedGenerator.WriteFile(simpleMessageBusFolder));
                            Console.WriteLine(generatedFileNames.Last());

                            // Generate Deleted message
                            using var deletedGenerator = new SimpleMessageBusGenerator(extraUsings, simpleMessageBusNamespace, composition, "Deleted");
                            generatedFileNames.Add(deletedGenerator.WriteFile(simpleMessageBusFolder));
                            Console.WriteLine(generatedFileNames.Last());
                        }
                    }
                }
            }

            //RWM: Cleanup Time!
            if (type == "core" || isAll)
            {
                CleanOtherFiles(entityFolder, generatedFileNames, pathToIgnore);
            }

            if (type == "data" || isAll)
            {
                CleanOtherFiles(dataFolder, generatedFileNames, pathToIgnore);
            }

            if (type == "business" || isAll)
            {
                CleanOtherFiles(businessFolder, generatedFileNames, pathToIgnore);
            }

            if (type == "api" || isAll)
            {
                CleanOtherFiles(helpersFolder, generatedFileNames, pathToIgnore);
                CleanOtherFiles(modelBuildersFolder, generatedFileNames, pathToIgnore);
                CleanOtherFiles(controllerFolder, generatedFileNames, pathToIgnore);
            }

            if ((type == "simplemessagebus" || isAll) && simpleMessageBusFolder is not null)
            {
                CleanOtherFiles(simpleMessageBusFolder, generatedFileNames, pathToIgnore);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static string GetNamespaceFromFolder(string folder)
        {
            return folder[(folder.LastIndexOf('\\') + 1)..];
        }

        /// <summary>
        /// Reads API generator configuration from MSBuild properties in the API project file.
        /// </summary>
        /// <param name="apiFolder">The path to the API project folder.</param>
        /// <returns>An ApiGeneratorConfig with the settings from the project file.</returns>
        private static ApiGeneratorConfig GetApiGeneratorConfig(string apiFolder)
        {
            var config = new ApiGeneratorConfig();

            try
            {
                MSBuildProjectManager.EnsureMSBuildRegistered();

                var projectFiles = Directory.GetFiles(apiFolder, "*.csproj");
                if (projectFiles.Length == 0)
                {
                    return config;
                }

                var project = new Project(projectFiles[0]);

                // Read the inheritance settings (default to true if not specified)
                if (project.GetProperty("EasyAFApiInheritance")?.EvaluatedValue is string apiInheritanceValue)
                {
                    bool.TryParse(apiInheritanceValue, out var apiInheritance);
                    config.ApiInheritance = apiInheritance;
                }

                if (project.GetProperty("EasyAFAdminApiInheritance")?.EvaluatedValue is string adminApiInheritanceValue)
                {
                    bool.TryParse(adminApiInheritanceValue, out var adminApiInheritance);
                    config.AdminApiInheritance = adminApiInheritance;
                }

                // Read base class settings (only if inheritance is enabled)
                if (config.ApiInheritance && project.GetProperty("EasyAFApiBaseClass")?.EvaluatedValue is string apiBaseClass)
                {
                    config.ApiBaseClass = apiBaseClass;
                }

                if (config.AdminApiInheritance && project.GetProperty("EasyAFAdminApiBaseClass")?.EvaluatedValue is string adminApiBaseClass)
                {
                    config.AdminApiBaseClass = adminApiBaseClass;
                }

                // Read additional usings
                if (project.GetProperty("EasyAFApiAdditionalUsings")?.EvaluatedValue is string additionalUsings)
                {
                    var usings = additionalUsings.Split(';');
                    foreach (var usingStatement in usings)
                    {
                        if (!string.IsNullOrWhiteSpace(usingStatement))
                        {
                            config.ApiAdditionalUsings.Add(usingStatement.Trim());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not read MSBuild properties from API project. Using defaults. Error: {ex.Message}");
            }

            return config;
        }

        #endregion

    }

}
