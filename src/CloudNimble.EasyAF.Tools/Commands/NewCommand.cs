using CloudNimble.EasyAF.Tools.Scaffolding;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tools.Commands
{

    /// <summary>
    /// Creates a new EasyAF solution: the Core, Data, Business, Api, MessageBus, and Tests projects, referenced the way EasyAF expects,
    /// with the solution-level build files and an <c>.slnx</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Projects are created in-process from the SDK's own <c>classlib</c>, <c>web</c>, and <c>console</c> templates, then configured with
    /// EasyAF project types, package references, and project references. EasyAF files (Program.cs, appsettings, test bases) are written over
    /// the template output.
    /// </para>
    /// <para>
    /// <c>new</c> only creates and links projects. It doesn't connect a database, restore, build, or start any process; <c>dotnet easyaf init</c>
    /// connects a database afterwards.
    /// </para>
    /// </remarks>
    [Command(Name = "new", Description = "Create a new EasyAF solution with Core, Data, Business, Api, MessageBus, and Tests projects.")]
    public class NewCommand : EasyAFBaseCommand
    {

        #region Fields

        private const string ClassLibraryTemplate = "classlib";
        private const string SolutionItemsFolder = "Solution Items";
        private const string WebJobSettingsResource = "MessageBus.Runtime/Properties/webjobs-publish-settings.json";

        private static readonly string[] AdditionalBuildTypes = ["DEV", "BETA", "PROD"];
        private static readonly string[] SolutionItems = ["Directory.Build.props", "Directory.Build.targets", "global.json", "DEV.runsettings"];

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the solution name. It is also the root namespace unless <see cref="Namespace"/> is set.
        /// </summary>
        [Argument(0, "name", "The solution name, for example CloudNimble.Contoso. Also the root namespace unless --namespace is set.")]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the folder to create the solution in. Defaults to the current folder.
        /// </summary>
        [Option("-o|--output", Description = "The folder to create the solution in. Defaults to the current folder.")]
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the root namespace for the projects. Defaults to <see cref="Name"/>.
        /// </summary>
        [Option("--namespace", Description = "The root namespace for the projects. Defaults to <name>.")]
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the .NET major version to target: <c>10</c> or <c>11</c> (the default).
        /// </summary>
        [Option("-f|--framework", Description = "The .NET version to target: 10 or 11 (default).")]
        public int Framework { get; set; } = 11;

        /// <summary>
        /// Gets or sets whether to leave out the Api project and its tests.
        /// </summary>
        [Option("--no-api", Description = "Don't create the Api project or its tests.")]
        public bool NoApi { get; set; }

        /// <summary>
        /// Gets or sets whether to leave out the MessageBus.Core, MessageBus.Dispatch, and MessageBus.Runtime projects.
        /// </summary>
        [Option("--no-messagebus", Description = "Don't create the MessageBus.Core, MessageBus.Dispatch, or MessageBus.Runtime projects.")]
        public bool NoMessageBus { get; set; }

        /// <summary>
        /// Gets or sets whether to leave out only the MessageBus.Runtime project.
        /// </summary>
        [Option("--no-runtime", Description = "Don't create the MessageBus.Runtime project.")]
        public bool NoRuntime { get; set; }

        /// <summary>
        /// Gets or sets whether to add Azure WebJob publish settings to the MessageBus.Runtime project.
        /// </summary>
        [Option("--webjob", Description = "Add Azure WebJob publish settings to the MessageBus.Runtime project.")]
        public bool WebJob { get; set; }

        /// <summary>
        /// Gets or sets the folder containing the SDK's versioned template folders. Defaults to <c>{dotnetRoot}/templates</c>.
        /// </summary>
        /// <remarks>
        /// Tests set this to an empty folder to check the missing-templates error.
        /// </remarks>
        internal string TemplatesRoot { get; set; }

        /// <summary>
        /// Gets or sets the function that asks whether a project folder that already has files may be overwritten.
        /// </summary>
        /// <remarks>
        /// Tests replace the console prompt with a fixed answer.
        /// </remarks>
        internal Func<string, bool> ConfirmOverwrite { get; set; } = PromptForOverwrite;

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates the solution.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>0 when the solution was created; otherwise 1.</returns>
        public async Task<int> OnExecuteAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                WriteError("A solution name is required, for example: dotnet easyaf new CloudNimble.Contoso");
                return 1;
            }

            ScaffoldOptions options;
            try
            {
                options = new ScaffoldOptions(string.IsNullOrWhiteSpace(Namespace) ? Name : Namespace, Framework, !NoApi, !NoMessageBus, !NoRuntime, WebJob);
            }
            catch (ArgumentException ex)
            {
                WriteError(ex.ParamName is null ? ex.Message : ex.Message.Replace($" (Parameter '{ex.ParamName}')", string.Empty, StringComparison.Ordinal));
                return 1;
            }

            var output = Path.GetFullPath(string.IsNullOrWhiteSpace(OutputDirectory) ? Environment.CurrentDirectory : OutputDirectory);
            var existingSolution = FindSolution(output);
            if (existingSolution is not null)
            {
                WriteError($"'{output}' already contains the solution '{Path.GetFileName(existingSolution)}'. Choose a different output folder with -o.");
                return 1;
            }

            try
            {
                var layout = ScaffoldLayout.Create(options);
                using var host = new SdkTemplateHost(TemplatesRoot ?? SdkTemplateHost.FindTemplatesRoot(), options.TargetFramework);

                // RWM: Check everything that can fail before writing anything, so a failed run leaves nothing behind.
                var missing = await host.GetMissingTemplatesAsync(layout.Select(p => p.TemplateShortName).Distinct(StringComparer.OrdinalIgnoreCase), cancellationToken).ConfigureAwait(false);
                if (missing.Count > 0)
                {
                    WriteError($"The {string.Join(", ", missing)} C# project template(s) for {options.TargetFramework} were not found in '{host.TemplateFolder ?? host.TemplatesRoot}'. " +
                        $"Install the .NET {options.DotNetVersion} SDK, or set DOTNET_ROOT to a .NET installation that has it.");
                    return 1;
                }

                var overwrite = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var project in layout)
                {
                    var projectFolder = Path.Combine(output, project.GetName(options.Namespace));
                    if (Directory.Exists(projectFolder) && Directory.EnumerateFileSystemEntries(projectFolder).Any())
                    {
                        if (!ConfirmOverwrite(projectFolder))
                        {
                            WriteError($"Nothing was created, because '{projectFolder}' already has files.");
                            return 1;
                        }
                        overwrite.Add(projectFolder);
                    }
                }

                CheckMSBuildRegistered();
                var solutionPath = await CreateSolutionAsync(options, layout, host, output, overwrite, cancellationToken).ConfigureAwait(false);
                WriteSummary(options, layout, output, solutionPath);
                return 0;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                WriteError(ex.Message);
                return 1;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Writes the solution files, creates and configures every project, writes the EasyAF files, and saves the <c>.slnx</c>.
        /// </summary>
        private static async Task<string> CreateSolutionAsync(ScaffoldOptions options, IReadOnlyList<ScaffoldProject> layout, SdkTemplateHost host, string output,
            HashSet<string> overwrite, CancellationToken cancellationToken)
        {
            // RWM: global.json pins the SDK that supplied the templates. If it can't be read, the lowest feature band still rolls forward.
            var sdkVersion = host.SdkVersion ?? $"{options.DotNetVersion}.0.100";
            var writer = new EmbeddedResourceWriter(options.CreateTokens(sdkVersion, DateTime.Now.Year));

            Directory.CreateDirectory(output);
            foreach (var item in SolutionItems)
            {
                await writer.WriteAsync(item, Path.Combine(output, item), cancellationToken).ConfigureAwait(false);
            }

            var configurator = new ProjectGraphConfigurator(output, options.Namespace);
            var solution = new SolutionFileBuilder(output, options.Namespace);
            foreach (var buildType in AdditionalBuildTypes)
            {
                solution.AddBuildType(buildType);
            }

            foreach (var project in layout)
            {
                var name = project.GetName(options.Namespace);
                var projectFolder = Path.Combine(output, name);
                var projectPath = await host.CreateProjectAsync(project.TemplateShortName, name, projectFolder, overwrite.Contains(projectFolder), cancellationToken).ConfigureAwait(false);
                configurator.Configure(project);

                var prefix = $"{project.Suffix}/";
                var resources = writer.ResourceNames
                    .Where(r => r.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .Where(r => options.WebJob || !string.Equals(r, WebJobSettingsResource, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var resource in resources)
                {
                    var relativePath = resource[prefix.Length..].Replace('/', Path.DirectorySeparatorChar);
                    await writer.WriteAsync(resource, Path.Combine(projectFolder, relativePath), cancellationToken).ConfigureAwait(false);
                }

                // RWM: The classlib template's Class1.cs is a placeholder; drop it where EasyAF adds real code.
                if (string.Equals(project.TemplateShortName, ClassLibraryTemplate, StringComparison.OrdinalIgnoreCase) &&
                    resources.Any(r => r.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
                {
                    File.Delete(Path.Combine(projectFolder, "Class1.cs"));
                }

                solution.AddProject(project.SolutionFolder, projectPath);
            }

            foreach (var item in SolutionItems)
            {
                solution.AddSolutionItem(SolutionItemsFolder, Path.Combine(output, item));
            }

            return await solution.SaveAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds an existing <c>.sln</c> or <c>.slnx</c> in the output folder.
        /// </summary>
        /// <remarks>
        /// Filters by extension instead of using a <c>*.sln</c> search pattern, because on Windows that pattern also matches <c>.slnx</c>.
        /// </remarks>
        private static string FindSolution(string folder)
        {
            if (!Directory.Exists(folder))
            {
                return null;
            }

            return Directory.EnumerateFiles(folder)
                .FirstOrDefault(f => Path.GetExtension(f) is { } extension &&
                    (extension.Equals(".sln", StringComparison.OrdinalIgnoreCase) || extension.Equals(".slnx", StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Asks on the console whether a project folder that already has files may be overwritten. Defaults to no.
        /// </summary>
        private static bool PromptForOverwrite(string projectFolder)
        {
            if (Console.IsInputRedirected)
            {
                Console.Error.WriteLine($"'{projectFolder}' already has files, and there's no interactive console to confirm overwriting them.");
                return false;
            }

            return Prompt.GetYesNo($"Overwrite {projectFolder}?", defaultAnswer: false);
        }

        private static void WriteError(string message)
        {
            Console.Error.WriteLine($"Error: {message}");
        }

        /// <summary>
        /// Prints what was created and what to do next.
        /// </summary>
        private static void WriteSummary(ScaffoldOptions options, IReadOnlyList<ScaffoldProject> layout, string output, string solutionPath)
        {
            Console.WriteLine($"Created {options.Namespace} in {output}");
            Console.WriteLine();
            Console.WriteLine($"  {Path.GetFileName(solutionPath)}");
            foreach (var folder in layout.GroupBy(p => p.SolutionFolder))
            {
                Console.WriteLine($"  /{folder.Key}/");
                foreach (var project in folder)
                {
                    Console.WriteLine($"    {project.GetName(options.Namespace)}");
                }
            }
            Console.WriteLine($"  /{SolutionItemsFolder}/");
            foreach (var item in SolutionItems)
            {
                Console.WriteLine($"    {item}");
            }

            Console.WriteLine();
            Console.WriteLine("Next steps:");
            if (!string.Equals(Path.TrimEndingDirectorySeparator(output), Path.TrimEndingDirectorySeparator(Path.GetFullPath(Environment.CurrentDirectory)), StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"  cd \"{output}\"");
            }
            Console.WriteLine("  When you're ready to connect a database, run 'dotnet easyaf init'.");
        }

        #endregion

    }

}
