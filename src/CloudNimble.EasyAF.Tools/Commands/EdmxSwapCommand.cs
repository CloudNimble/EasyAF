using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tools.Commands
{

    /// <summary>
    /// Command to switch the Provider in the EDMX file between System.Data.SqlClient and Microsoft.Data.SqlClient.
    /// </summary>
    /// <remarks>
    /// This command locates the EDMX file in the specified directory (or the .Data folder) and swaps the provider string.
    /// </remarks>
    /// <example>
    /// <code>
    /// dotnet easyaf edmx swap --path "C:\MySolution"
    /// </code>
    /// </example>
    [Command(
        Name = "swap",
        Description = "Switch the Provider in the EDMX file between the System.Data.SqlClient & Microsoft.Data.SqlClient."
    )]
    public class EdmxSwapCommand
    {

        #region Properties

        /// <summary>
        /// Gets or sets the working directory for the code compiler. Defaults to current directory.
        /// </summary>
        [Option("-path <path>", Description = "Working directory for the code compiler. Defaults to current directory.")]
        public string Root { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the EDMX provider swap command.
        /// </summary>
        /// <returns>0 if successful, 1 if an error occurred.</returns>
        /// <example>
        /// <code>
        /// dotnet easyaf edmx swap --path "C:\MySolution"
        /// </code>
        /// </example>
        public Task<int> OnExecuteAsync()
        {
            Console.WriteLine("Hello. Welcome to EasyAF.");
            Console.WriteLine("You've chosen to swap the Provider in an EDMX file.");

            var rootPath = !string.IsNullOrWhiteSpace(Root)
                ? Root
                : Directory.GetCurrentDirectory();

            ArgumentException.ThrowIfNullOrWhiteSpace(rootPath, nameof(Root));

            if (Directory.Exists(rootPath))
            {
                Console.WriteLine("The path specified was a folder, not an EDMX file. Looking for one now.");

                var file = EdmxRootCommand.FindEdmxFile(rootPath);
                if (!string.IsNullOrWhiteSpace(file))
                {
                    Console.WriteLine("EDMX files were found in this folder. Fixing the first one.");
                    rootPath = file;
                }
                else
                {
                    Console.WriteLine("EDMX files not found. Attempting to locate the .Data folder.");
                    var dataFolder = EdmxRootCommand.FindDataFolder(rootPath);

                    if (string.IsNullOrWhiteSpace(dataFolder))
                    {
                        Console.WriteLine($"The data folder could not be found in {rootPath}.\nExiting, sorry about that.");

                        return Task.FromResult(1);
                    }

                    file = EdmxRootCommand.FindEdmxFile(dataFolder);
                    if (string.IsNullOrWhiteSpace(file))
                    {
                        Console.WriteLine("There were no EDMX files in the .Data folder.\nExiting, sorry about that.");

                        return Task.FromResult(1);
                    }

                    Console.WriteLine("EDMX files were found in this folder. Fixing the first one.");
                    rootPath = Path.Combine(dataFolder, file);
                }
            }

            FixEdmxProvider(rootPath);

            Console.WriteLine("EDMX manipulation finished. Have a great day!");

            return Task.FromResult(0);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Swaps the provider in the EDMX file between System.Data.SqlClient and Microsoft.Data.SqlClient.
        /// </summary>
        internal static void FixEdmxProvider(string path)
        {
            var fullPath = Path.GetFullPath(path);
            Console.WriteLine($"Attepting to load EDMX file at {fullPath}...");
            var edmx = XElement.Load(fullPath, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
            var schemaElement = (edmx.Elements()
                .Where(e => e.Name.LocalName == "Runtime")
                .Elements()
                .Where(e => e.Name.LocalName == "StorageModels")
                .Elements()
                .Where(e => e.Name.LocalName == "Schema")
                .FirstOrDefault()
                    ?? edmx)
                    
                    ?? throw new FileLoadException("The EDMX file at {} could not be loaded.");

            var providerAttribute = schemaElement.Attribute("Provider");
            var providerValue = providerAttribute?.Value ?? "";
            Console.WriteLine($"Current Provider: {providerValue}");

            if (providerAttribute is null || string.IsNullOrWhiteSpace(providerValue))
            {
                Console.WriteLine("Provider value not found, can't continue.");
                return;
            }

            switch (providerValue)
            {
                case "System.Data.SqlClient":
                    providerAttribute.SetValue("Microsoft.Data.SqlClient");
                    break;
                case "Microsoft.Data.SqlClient":
                    providerAttribute.SetValue("System.Data.SqlClient");
                    break;
            }
            Console.WriteLine($"New Provider: {providerAttribute.Value}");
            edmx.Save(Path.GetFullPath(path), SaveOptions.None);
        }

        #endregion

    }

}
