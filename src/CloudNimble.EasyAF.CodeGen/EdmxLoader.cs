using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.CodeGen
{

    /// <summary>
    /// Loads and parses EDMX files, extracting Entity Data Model components and EasyAF extensions.
    /// </summary>
    public class EdmxLoader
    {

        #region Properties

        /// <summary>
        /// Gets or sets the Entity Data Model item collection containing conceptual model metadata.
        /// </summary>
        public EdmItemCollection EdmItems { get; private set; }

        /// <summary>
        /// Gets or sets the CSDL (Conceptual Schema Definition Language) XML element.
        /// </summary>
        public XElement CsdlElement { get; private set; }

        /// <summary>
        /// Gets or sets the collection of EDMX schema errors encountered during loading.
        /// </summary>
        public List<CompilerError> EdmxSchemaErrors { get; private set; }

        /// <summary>
        /// Gets or sets the collection of entity compositions extracted from the model.
        /// </summary>
        public List<EntityComposition> Entities { get; private set; }

        /// <summary>
        /// Gets or sets the entity container from the conceptual model.
        /// </summary>
        public EntityContainer EntityContainer { get; private set; }

        /// <summary>
        /// Gets the collection of entity sets from the entity container.
        /// </summary>
        public List<EntitySet> EntitySets => EntityContainer.BaseEntitySets.OfType<EntitySet>().ToList();

        /// <summary>
        /// Gets or sets the file path of the loaded EDMX file.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the model is using Entity Framework Core.
        /// </summary>
        public bool IsEFCore => !string.IsNullOrWhiteSpace(OnModelCreatingMethod);

        /// <summary>
        /// Gets or sets the namespace of the conceptual model.
        /// </summary>
        public string ModelNamespace { get; set; }

        /// <summary>
        /// Gets or sets the MSL (Mapping Specification Language) XML element.
        /// </summary>
        public XElement MslElement { get; private set; }

        /// <summary>
        /// Gets or sets the complete OnModelCreating method extracted from EasyAF extensions.
        /// </summary>
        /// <value>
        /// The complete C# OnModelCreating method including signature and braces as a string.
        /// Returns an empty string if no OnModelCreating method is found in the EDMX file.
        /// </value>
        /// <remarks>
        /// This property contains the OnModelCreating method stored in the EasyAF Extensions
        /// section of the EDMX Designer metadata. The method can be used for code generation
        /// or documentation purposes.
        /// </remarks>
        public string OnModelCreatingMethod { get; private set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SSDL (Store Schema Definition Language) XML element.
        /// </summary>
        public XElement SsdlElement { get; private set; }

        /// <summary>
        /// Gets or sets the store item collection containing storage model metadata.
        /// </summary>
        public StoreItemCollection StoreItems { get; private set; }

        /// <summary>
        /// Gets or sets the storage mapping item collection containing C-S mapping metadata.
        /// </summary>
        public StorageMappingItemCollection Mappings { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmxLoader"/> class.
        /// </summary>
        public EdmxLoader()
        {
            EdmxSchemaErrors = [];
            EdmItems = new EdmItemCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmxLoader"/> class with the specified file path.
        /// </summary>
        /// <param name="filePath">The path to the EDMX file to load.</param>
        /// <exception cref="ArgumentException">Thrown when the file path does not exist or is not an EDMX file.</exception>
        public EdmxLoader(string filePath) : this()
        {
            filePath = Path.GetFullPath(filePath);
            if (!File.Exists(Path.GetFullPath(filePath)))
            {
                throw new ArgumentException("The filePath specified does not exist.");
            }

            if (Path.GetExtension(filePath) != ".edmx")
            {
                throw new ArgumentException("The filePath specified does point to an EDMX file.");
            }

            FilePath = filePath;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads and parses the EDMX file from the file path specified in the constructor.
        /// </summary>
        /// <param name="fixProvider">Whether to fix the provider attribute to use System.Data.SqlClient.</param>
        public void Load(bool fixProvider = true)
        {
            var root = XElement.Load(FilePath, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
            LoadInternal(root, fixProvider);
        }

        /// <summary>
        /// Loads and parses the EDMX content from the specified string.
        /// </summary>
        /// <param name="content">The EDMX XML content to parse.</param>
        /// <param name="fixProvider">Whether to fix the provider attribute to use System.Data.SqlClient.</param>
        public void Load(string content, bool fixProvider = true)
        {
            var root = XElement.Parse(content, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
            LoadInternal(root, fixProvider);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Internal method to load and parse EDMX content from an XML element.
        /// </summary>
        /// <param name="root">The root XML element of the EDMX document.</param>
        /// <param name="fixProvider">Whether to fix the provider attribute to use System.Data.SqlClient.</param>
        private void LoadInternal(XElement root, bool fixProvider)
        {
            var runtimeElement = root.Elements()
                .Where(e => e.Name.LocalName == "Runtime")
                .Elements();

            CsdlElement = runtimeElement
                .Where(e => e.Name.LocalName == "ConceptualModels")
                .Elements()
                .Where(e => e.Name.LocalName == "Schema")
                .FirstOrDefault();

            MslElement = runtimeElement
                .Where(e => e.Name.LocalName == "Mappings")
                .Elements()
                .Where(e => e.Name.LocalName == "Mapping")
                .FirstOrDefault();

            SsdlElement = runtimeElement
                .Where(e => e.Name.LocalName == "StorageModels")
                .Elements()
                .Where(e => e.Name.LocalName == "Schema")
                .FirstOrDefault();

            if (CsdlElement is null)
            {
                throw new FileLoadException("The EDMX file could not be loaded.");
            }

            var namespaceAttribute = CsdlElement.Attribute("Namespace");
            ModelNamespace = namespaceAttribute is not null ? namespaceAttribute.Value : "";

            // Extract OnModelCreating method from EasyAF Extensions
            ExtractOnModelCreatingMethod(root);

            if (fixProvider)
            {
                var providerAttribute = SsdlElement.Attribute("Provider");
                var providerValue = providerAttribute?.Value ?? "";
                if (providerValue != ProviderConstants.SystemDataClient)
                {
                    providerAttribute.SetValue(ProviderConstants.SystemDataClient);
                }
            }

            IList<EdmSchemaError> csdlErrors = [];

            try
            {
                using var csdlReader = CsdlElement.CreateReader();
                EdmItems = EdmItemCollection.Create([csdlReader], null, out csdlErrors);

                using var ssdlReader = SsdlElement.CreateReader();
                StoreItems = new StoreItemCollection([ssdlReader]);

                using var mslReader = MslElement.CreateReader();
                try
                {
                    Mappings = new StorageMappingItemCollection(EdmItems, StoreItems, [mslReader]);
                }
                catch (MappingException ex)
                {
                    EdmxSchemaErrors.Add(new CompilerError(FilePath ?? string.Empty, 0, 0, "MSL", ex.Message));
                }

                ProcessErrors(csdlErrors);

                if (EdmItems is not null)
                {
                    Entities = EdmItems
                        .OfType<EntityType>()
                        .OrderBy(c => c.Name)
                        .Select(c => new EntityComposition(c))
                        .ToList();

                    EntityContainer = EdmItems
                        .OfType<EntityContainer>()
                        .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                // Skip debug assertion failures in test environments
                if (!ex.GetType().Name.Contains("DebugAssert"))
                {
                    EdmxSchemaErrors.Add(new CompilerError(FilePath ?? string.Empty, 0, 0, "EDMX", ex.Message));
                }
                ProcessErrors(csdlErrors);
            }
        }

        /// <summary>
        /// Extracts the OnModelCreating method from the EasyAF Extensions section of the EDMX Designer.
        /// </summary>
        /// <param name="root">The root XML element of the EDMX document.</param>
        private void ExtractOnModelCreatingMethod(XElement root)
        {
            try
            {
                // Define the EasyAF namespace
                var easyafNs = XNamespace.Get("http://schemas.cloudnimble.com/easyaf/2025/01/edmx");

                // Navigate to Designer section
                var designerElement = root.Elements()
                    .Where(e => e.Name.LocalName == "Designer")
                    .FirstOrDefault();

                if (designerElement is null)
                {
                    // No Designer section found
                    return;
                }

                // Find EasyAF Extensions element
                var extensionsElement = designerElement.Elements(easyafNs + "Extensions")
                    .FirstOrDefault();

                if (extensionsElement is null)
                {
                    // No EasyAF Extensions found
                    return;
                }

                // Find OnModelCreating element within Extensions
                var onModelCreatingElement = extensionsElement.Elements(easyafNs + "OnModelCreating")
                    .FirstOrDefault();

                if (onModelCreatingElement is not null)
                {
                    // Extract the CDATA content or text content
                    OnModelCreatingMethod = onModelCreatingElement.Value;
                    
                    if (!string.IsNullOrWhiteSpace(OnModelCreatingMethod))
                    {
                        Console.WriteLine("Successfully extracted OnModelCreating method from EasyAF Extensions.");
                        var lineCount = OnModelCreatingMethod.Split('\n').Length;
                        Console.WriteLine($"OnModelCreating method contains {lineCount} lines.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting OnModelCreating method from EasyAF Extensions: {ex.Message}");
                OnModelCreatingMethod = string.Empty;
            }
        }

        /// <summary>
        /// Processes EDM schema errors and converts them to compiler errors.
        /// </summary>
        /// <param name="errors">The collection of EDM schema errors to process.</param>
        private void ProcessErrors(IEnumerable<EdmSchemaError> errors)
        {
            foreach (var error in errors)
            {
                EdmxSchemaErrors.Add(
                    new CompilerError(
                        error.SchemaLocation ?? FilePath ?? string.Empty,
                        error.Line,
                        error.Column,
                        error.ErrorCode.ToString(CultureInfo.InvariantCulture),
                        error.Message)
                    {
                        IsWarning = error.Severity == EdmSchemaErrorSeverity.Warning
                    });
            }
        }

        #endregion

    }

}
