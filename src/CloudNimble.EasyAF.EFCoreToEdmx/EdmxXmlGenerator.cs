using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.EFCoreToEdmx
{

    /// <summary>
    /// Generates EDMX XML content from structured EDMX model objects.
    /// </summary>
    /// <remarks>
    /// This class is responsible for converting the intermediate EDMX model representation
    /// into properly formatted EDMX XML that conforms to the Entity Data Model specification.
    /// The generated XML includes conceptual model, storage model, mapping sections, and
    /// designer metadata. The model is provided during construction and the XML can be generated on demand.
    /// </remarks>
    public class EdmxXmlGenerator
    {

        #region Fields

        private readonly XNamespace _annotationNs = XNamespace.Get("http://schemas.microsoft.com/ado/2009/02/edm/annotation");
        private readonly XNamespace _customAnnotationNs = XNamespace.Get("http://schemas.microsoft.com/ado/2013/11/edm/customannotation");
        private readonly XNamespace _easyafNs = XNamespace.Get("http://schemas.cloudnimble.com/easyaf/2025/01/edmx");
        private readonly XNamespace _edmNs = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm");
        private readonly XNamespace _edmxNs = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edmx");
        private readonly XNamespace _mappingNs = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/mapping/cs");
        private readonly EdmxModel _model;
        private readonly XNamespace _ssdlNs = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm/ssdl");
        private readonly XNamespace _storeNs = XNamespace.Get("http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator");
        private readonly DatabaseProviderType _databaseProviderType = DatabaseProviderType.Unknown;
        private readonly Dictionary<string, string> _tableInfos;
        private readonly IPluralizer _pluralizationService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmxXmlGenerator"/> class.
        /// </summary>
        /// <param name="model">The EDMX model to convert to XML. Must not be null.</param>
        /// <param name="providerType">The database provider type for the storage model. Must be explicitly specified to ensure correct type mappings.</param>
        /// <param name="tableInfos">Optional dictionary mapping entity type names to actual table names.</param>
        /// <param name="pluralizationService">The pluralization service for proper entity name pluralization. If null, EF Core's design-time service will be used.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is null.</exception>
        public EdmxXmlGenerator(EdmxModel model, DatabaseProviderType providerType, Dictionary<string, string> tableInfos = null, IPluralizer pluralizationService = null)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));
            _model = model;
            _databaseProviderType = providerType;
            _tableInfos = tableInfos ?? new Dictionary<string, string>();
            _pluralizationService = pluralizationService ?? CreateEFCoreDesignTimePluralizationService();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates complete EDMX XML from the model provided during construction.
        /// </summary>
        /// <returns>A string containing the complete EDMX XML document.</returns>
        /// <remarks>
        /// Generates a complete EDMX XML document including the XML declaration, root edmx:Edmx element,
        /// and all required sections: ConceptualModels, StorageModels, Mappings, and Designer metadata.
        /// The output conforms to EDMX version 3.0 specification with EasyAF extensions.
        /// </remarks>
        /// <example>
        /// <code>
        /// var generator = new EdmxXmlGenerator(edmxModel, DatabaseProviderType.SqlServer);
        /// string xmlContent = generator.Generate();
        /// </code>
        /// </example>
        public string Generate()
        {
            // Validate model
            if (string.IsNullOrWhiteSpace(_model.Namespace))
            {
                throw new InvalidOperationException("Model namespace cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(_model.ContainerName))
            {
                throw new InvalidOperationException("Model container name cannot be null or empty.");
            }

            // Validate entity types
            foreach (var entityType in _model.EntityTypes)
            {
                if (string.IsNullOrWhiteSpace(entityType.Name))
                {
                    throw new InvalidOperationException("Entity type name cannot be null or empty.");
                }
            }

            // Validate entity sets
            foreach (var entitySet in _model.EntitySets)
            {
                if (string.IsNullOrWhiteSpace(entitySet.Name))
                {
                    throw new InvalidOperationException("Entity set name cannot be null or empty.");
                }

                if (string.IsNullOrWhiteSpace(entitySet.EntityTypeName))
                {
                    throw new InvalidOperationException($"Entity set '{entitySet.Name}' has empty EntityTypeName.");
                }
            }

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(_edmxNs + "Edmx",
                    new XAttribute("Version", "3.0"),
                    new XAttribute(XNamespace.Xmlns + "edmx", _edmxNs.NamespaceName),
                    new XComment("EF Runtime content"),
                    new XElement(_edmxNs + "Runtime",
                        new XComment("SSDL content"),
                        new XElement(_edmxNs + "StorageModels",
                            GenerateStorageModel()
                        ),
                        new XComment("CSDL content"),
                        new XElement(_edmxNs + "ConceptualModels",
                            GenerateConceptualModel()
                        ),
                        new XComment("C-S mapping content"),
                        new XElement(_edmxNs + "Mappings",
                            GenerateMappings()
                        )
                    ),
                    new XComment("EF Designer content (Do not edit manually)"),
                    GenerateDesignerSection()
                )
            );

            return doc.ToString();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates EF Core's design-time pluralization service.
        /// </summary>
        /// <returns>An instance of <see cref="IPluralizer"/>.</returns>
        /// <remarks>
        /// Uses EF Core's design-time services to get the proper pluralization service.
        /// This ensures consistency with how EF Core handles pluralization during scaffolding.
        /// </remarks>
        private static IPluralizer CreateEFCoreDesignTimePluralizationService()
        {
            try
            {
                var services = new ServiceCollection();
                services.AddEntityFrameworkDesignTimeServices();
                var serviceProvider = services.BuildServiceProvider();
                return serviceProvider.GetRequiredService<IPluralizer>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create EF Core pluralization service. Ensure Microsoft.EntityFrameworkCore.Design package is referenced. Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates the conceptual model section of the EDMX XML.
        /// </summary>
        /// <returns>An <see cref="XElement"/> representing the conceptual model schema.</returns>
        /// <remarks>
        /// The conceptual model contains entity types, associations, and the entity container
        /// that define the logical structure of the data model independent of storage details.
        /// In CSDL, entity type names are SINGULAR.
        /// </remarks>
        private XElement GenerateConceptualModel()
        {
            var schema = new XElement(_edmNs + "Schema",
                new XAttribute("Namespace", _model.Namespace),
                new XAttribute("Alias", "Self"),
                new XAttribute(_annotationNs + "UseStrongSpatialTypes", "false"),
                new XAttribute(XNamespace.Xmlns + "annotation", _annotationNs.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "customannotation", _customAnnotationNs.NamespaceName)
            );

            // Add entity container to the schema
            schema.Add(GenerateConceptualEntityContainer());

            // Add entity types to the schema (SINGULAR names in CSDL)
            foreach (var entityType in _model.EntityTypes)
            {
                schema.Add(GenerateConceptualEntityType(entityType));
            }

            // Add associations to the schema
            foreach (var association in _model.Associations)
            {
                schema.Add(GenerateConceptualAssociation(association));
            }

            return schema;
        }

        /// <summary>
        /// Generates XML for an individual entity type in the conceptual model.
        /// </summary>
        /// <param name="entityType">The entity type to convert to XML.</param>
        /// <returns>An <see cref="XElement"/> representing the entity type.</returns>
        /// <remarks>
        /// Includes the entity type definition with key properties, scalar properties,
        /// and navigation properties, along with all associated metadata.
        /// Uses singular entity type names for CSDL as per EDMX specification.
        /// </remarks>
        private XElement GenerateConceptualEntityType(EdmxEntityType entityType)
        {
            // Validate entity type name
            if (string.IsNullOrWhiteSpace(entityType.Name))
            {
                throw new InvalidOperationException("Entity type name cannot be null or empty.");
            }

            // In CSDL, entity type names should be SINGULAR
            var element = new XElement(_edmNs + "EntityType",
                new XAttribute("Name", entityType.Name) // Keep as singular
            );

            // Add documentation if available
            if (!string.IsNullOrWhiteSpace(entityType.Documentation))
            {
                element.Add(new XElement(_edmNs + "Documentation",
                    new XElement(_edmNs + "Summary", entityType.Documentation)
                ));
            }

            // Add keys
            var keyElement = new XElement(_edmNs + "Key");
            if (entityType.Keys.Count != 0)
            {
                foreach (var key in entityType.Keys)
                {
                    keyElement.Add(new XElement(_edmNs + "PropertyRef", new XAttribute("Name", key)));
                }
            }
            else
            {
                // If no explicit keys, use all properties as composite key (fallback)
                foreach (var property in entityType.Properties)
                {
                    keyElement.Add(new XElement(_edmNs + "PropertyRef", new XAttribute("Name", property.Name)));
                }
            }
            element.Add(keyElement);

            // Add scalar properties
            foreach (var property in entityType.Properties)
            {
                element.Add(GenerateConceptualProperty(property));
            }

            // Add navigation properties
            foreach (var navProperty in entityType.NavigationProperties)
            {
                element.Add(GenerateNavigationProperty(navProperty));
            }

            return element;
        }

        /// <summary>
        /// Generates XML for an individual scalar property in the conceptual model.
        /// </summary>
        /// <param name="property">The property to convert to XML.</param>
        /// <returns>An <see cref="XElement"/> representing the property.</returns>
        /// <remarks>
        /// Uses CLR types and excludes implementation-specific default values for the conceptual model.
        /// Includes all property attributes such as nullability, length constraints, precision and scale.
        /// </remarks>
        private XElement GenerateConceptualProperty(EdmxProperty property)
        {
            // Validate property name
            if (string.IsNullOrWhiteSpace(property.Name))
            {
                throw new InvalidOperationException("Property name cannot be null or empty.");
            }

            // Validate property type
            if (string.IsNullOrWhiteSpace(property.Type))
            {
                throw new InvalidOperationException($"Property type cannot be null or empty for property '{property.Name}'.");
            }

            var element = new XElement(_edmNs + "Property",
                new XAttribute("Name", property.Name),
                new XAttribute("Type", property.Type), // Use CLR type for EF6 compatibility
                new XAttribute("Nullable", property.Nullable.ToString().ToLower())
            );

            // Add optional attributes based on property characteristics
            if (property.MaxLength.HasValue)
                element.Add(new XAttribute("MaxLength", property.MaxLength.Value));

            if (property.Precision.HasValue)
                element.Add(new XAttribute("Precision", property.Precision.Value));

            if (property.Scale.HasValue)
                element.Add(new XAttribute("Scale", property.Scale.Value));

            if (property.IsFixedLength.HasValue)
                element.Add(new XAttribute("FixedLength", property.IsFixedLength.Value.ToString().ToLower()));

            if (property.IsUnicode.HasValue)
                element.Add(new XAttribute("Unicode", property.IsUnicode.Value.ToString().ToLower()));

            // Don't include default values in conceptual model unless explicitly requested
            if (property.IncludeDefaultInConceptual && !string.IsNullOrWhiteSpace(property.DefaultValue))
            {
                element.Add(new XAttribute("DefaultValue", property.DefaultValue));
            }

            // Add documentation if available
            if (!string.IsNullOrWhiteSpace(property.Documentation))
            {
                element.Add(new XElement(_edmNs + "Documentation",
                    new XElement(_edmNs + "Summary", property.Documentation)
                ));
            }

            return element;
        }

        /// <summary>
        /// Generates XML for an individual navigation property.
        /// </summary>
        /// <param name="navProperty">The navigation property to convert to XML.</param>
        /// <returns>An <see cref="XElement"/> representing the navigation property.</returns>
        /// <remarks>
        /// Includes the navigation property definition with relationship reference and role assignments.
        /// </remarks>
        private XElement GenerateNavigationProperty(EdmxNavigationProperty navProperty)
        {
            return new XElement(_edmNs + "NavigationProperty",
                new XAttribute("Name", navProperty.Name),
                new XAttribute("Relationship", $"{_model.Namespace}.{navProperty.Relationship}"),
                new XAttribute("FromRole", navProperty.FromRole),
                new XAttribute("ToRole", navProperty.ToRole)
            );
        }

        /// <summary>
        /// Generates XML for an individual association in the conceptual model.
        /// </summary>
        /// <param name="association">The association to convert to XML.</param>
        /// <returns>An <see cref="XElement"/> representing the association.</returns>
        /// <remarks>
        /// Includes both association ends with their multiplicity constraints and any
        /// referential constraints that define foreign key mappings.
        /// </remarks>
        private XElement GenerateConceptualAssociation(EdmxAssociation association)
        {
            var element = new XElement(_edmNs + "Association",
                new XAttribute("Name", association.Name)
            );

            // Add first association end
            element.Add(new XElement(_edmNs + "End",
                new XAttribute("Role", association.End1.Role),
                new XAttribute("Type", $"Self.{association.End1.Type}"),
                new XAttribute("Multiplicity", association.End1.Multiplicity)
            ));

            // Add second association end
            element.Add(new XElement(_edmNs + "End",
                new XAttribute("Role", association.End2.Role),
                new XAttribute("Type", $"Self.{association.End2.Type}"),
                new XAttribute("Multiplicity", association.End2.Multiplicity)
            ));

            // Add referential constraint if present
            if (association.ReferentialConstraint is not null)
            {
                var refConstraint = new XElement(_edmNs + "ReferentialConstraint");

                // Add principal role with property references
                var principal = new XElement(_edmNs + "Principal",
                    new XAttribute("Role", association.ReferentialConstraint.Principal.Role)
                );
                foreach (var propRef in association.ReferentialConstraint.Principal.PropertyRefs)
                {
                    principal.Add(new XElement(_edmNs + "PropertyRef", new XAttribute("Name", propRef)));
                }
                refConstraint.Add(principal);

                // Add dependent role with property references
                var dependent = new XElement(_edmNs + "Dependent",
                    new XAttribute("Role", association.ReferentialConstraint.Dependent.Role)
                );
                foreach (var propRef in association.ReferentialConstraint.Dependent.PropertyRefs)
                {
                    dependent.Add(new XElement(_edmNs + "PropertyRef", new XAttribute("Name", propRef)));
                }
                refConstraint.Add(dependent);

                element.Add(refConstraint);
            }

            return element;
        }

        /// <summary>
        /// Generates the entity container section of the conceptual model.
        /// </summary>
        /// <returns>An <see cref="XElement"/> representing the entity container.</returns>
        /// <remarks>
        /// The entity container groups all entity sets and association sets, providing
        /// the runtime context for the conceptual model.
        /// </remarks>
        private XElement GenerateConceptualEntityContainer()
        {
            var container = new XElement(_edmNs + "EntityContainer",
                new XAttribute("Name", _model.ContainerName),
                new XAttribute(_annotationNs + "LazyLoadingEnabled", "false")
            );

            // Add all entity sets (PLURAL names)
            foreach (var entitySet in _model.EntitySets)
            {
                container.Add(new XElement(_edmNs + "EntitySet",
                    new XAttribute("Name", entitySet.Name), // Plural
                    new XAttribute("EntityType", $"{_model.Namespace}.{entitySet.EntityTypeName}") // Singular entity type name
                ));
            }

            // Add all association sets
            foreach (var associationSet in _model.AssociationSets)
            {
                var assocSetElement = new XElement(_edmNs + "AssociationSet",
                    new XAttribute("Name", associationSet.Name),
                    new XAttribute("Association", $"{_model.Namespace}.{associationSet.Association}")
                );

                // Add first association set end
                assocSetElement.Add(new XElement(_edmNs + "End",
                    new XAttribute("Role", associationSet.End1.Role),
                    new XAttribute("EntitySet", associationSet.End1.EntitySet)
                ));

                // Add second association set end
                assocSetElement.Add(new XElement(_edmNs + "End",
                    new XAttribute("Role", associationSet.End2.Role),
                    new XAttribute("EntitySet", associationSet.End2.EntitySet)
                ));

                container.Add(assocSetElement);
            }

            return container;
        }

        /// <summary>
        /// Generates the storage model section of the EDMX XML.
        /// </summary>
        /// <returns>An <see cref="XElement"/> representing the storage model schema.</returns>
        /// <remarks>
        /// The storage model defines the database schema structure using SSDL (Store Schema Definition Language).
        /// Uses database-specific SQL types and includes default values and store generation patterns.
        /// In SSDL, entity type names are PLURAL to match database table naming conventions.
        /// </remarks>
        private XElement GenerateStorageModel()
        {
            // EDMX files are code generation helpers, not functional databases
            // Always use Microsoft.Data.SqlClient and 2012.Azure regardless of source database
            // This ensures EDMX compatibility and prevents provider-specific issues
            var providerName = "Microsoft.Data.SqlClient";
            var providerToken = "2012.Azure";
            
            // Generate SSDL (Store Schema Definition Language)
            var schema = new XElement(_ssdlNs + "Schema",
                new XAttribute("Namespace", $"{_model.Namespace}.Store"),
                new XAttribute("Provider", providerName),
                new XAttribute("ProviderManifestToken", providerToken),
                new XAttribute("Alias", "Self"),
                new XAttribute(XNamespace.Xmlns + "store", _storeNs.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "customannotation", _customAnnotationNs.NamespaceName)
            );

            // Generate entity types for the storage model (tables) - PLURAL names
            foreach (var entityType in _model.EntityTypes)
            {
                var storageEntityType = GenerateStorageEntityType(entityType);
                schema.Add(storageEntityType);
            }
            
            // Generate associations in the storage model (foreign keys)
            foreach (var association in _model.Associations)
            {
                var storageAssociation = GenerateStorageAssociation(association);
                schema.Add(storageAssociation);
            }

            schema.Add(GenerateStorageEntityContainer());
            return schema;
        }

        /// <summary>
        /// Generates XML for an individual entity type in the storage model.
        /// </summary>
        /// <param name="entityType">The entity type to convert to XML.</param>
        /// <returns>An <see cref="XElement"/> representing the storage entity type.</returns>
        /// <remarks>
        /// Uses database-specific SQL types and includes store generation patterns.
        /// SSDL entity type names are PLURAL to match database table naming conventions.
        /// </remarks>
        private XElement GenerateStorageEntityType(EdmxEntityType entityType)
        {
            // SSDL EntityType names should be PLURAL - use EF Core's pluralization service
            var pluralEntityTypeName = _pluralizationService.Pluralize(entityType.Name);
            
            var element = new XElement(_ssdlNs + "EntityType",
                new XAttribute("Name", pluralEntityTypeName)
            );

            // Add documentation if available
            if (!string.IsNullOrWhiteSpace(entityType.Documentation))
            {
                element.Add(new XElement(_ssdlNs + "Documentation",
                    new XElement(_ssdlNs + "Summary", entityType.Documentation)
                ));
            }

            // Add key - need to map key names to their store column names
            var keyElement = new XElement(_ssdlNs + "Key");
            foreach (var key in entityType.Keys)
            {
                // Find the property with this key name to get its store column name
                var keyProperty = entityType.Properties.FirstOrDefault(p => p.Name == key);
                var keyColumnName = keyProperty != null && !string.IsNullOrEmpty(keyProperty.StoreColumnName) 
                    ? keyProperty.StoreColumnName 
                    : key;
                keyElement.Add(new XElement(_ssdlNs + "PropertyRef", new XAttribute("Name", keyColumnName)));
            }
            element.Add(keyElement);

            // Add properties with SQL types
            foreach (var property in entityType.Properties)
            {
                element.Add(GenerateStorageProperty(property));
            }

            return element;
        }

        /// <summary>
        /// Generates XML for an individual property in the storage model.
        /// </summary>
        /// <param name="property">The property to convert to XML.</param>
        /// <returns>An <see cref="XElement"/> representing the storage property.</returns>
        /// <remarks>
        /// Uses database-specific SQL types and includes store generation patterns and default values.
        /// </remarks>
        private XElement GenerateStorageProperty(EdmxProperty property)
        {
            var sqlType = MapToSqlType(property);

            // Use StoreColumnName if available, otherwise fall back to Name
            var columnName = !string.IsNullOrEmpty(property.StoreColumnName) ? property.StoreColumnName : property.Name;

            var element = new XElement(_ssdlNs + "Property",
                new XAttribute("Name", columnName),
                new XAttribute("Type", sqlType),
                new XAttribute("Nullable", property.Nullable.ToString().ToLower())
            );

            // Add SQL-specific attributes
            if (property.MaxLength.HasValue)
            {
                element.Add(new XAttribute("MaxLength", property.MaxLength.Value));
            }

            if (property.Precision.HasValue)
                element.Add(new XAttribute("Precision", property.Precision.Value));

            if (property.Scale.HasValue)
                element.Add(new XAttribute("Scale", property.Scale.Value));

            if (property.IsFixedLength.HasValue && property.IsFixedLength.Value)
                element.Add(new XAttribute("FixedLength", "true"));

            //// Include store generated pattern in storage model
            //if (!string.IsNullOrWhiteSpace(property.StoreGeneratedPattern) &&
            //    property.StoreGeneratedPattern != "None")
            //{
            //    element.Add(new XAttribute("StoreGeneratedPattern", property.StoreGeneratedPattern));
            //}

            return element;
        }

        /// <summary>
        /// Maps property types to standard SQL Server types for EDMX SSDL compatibility.
        /// </summary>
        /// <param name="property">The property to map.</param>
        /// <returns>The appropriate SQL type string for EDMX SSDL.</returns>
        /// <remarks>
        /// EDMX files are code generation helpers, not functional databases.
        /// Always use SQL Server types in SSDL regardless of source database to ensure
        /// EDMX compatibility and prevent "Type X is not qualified with a namespace" errors.
        /// The actual database scaffolding handles source database-specific types correctly.
        /// </remarks>
        private string MapToSqlType(EdmxProperty property)
        {
            var typeToMap = property.Type;
            
            // Always map to SQL Server types for EDMX SSDL compatibility
            // This prevents "Type X is not qualified with a namespace" errors
            return typeToMap switch
            {
                "String" => "nvarchar",
                "Int32" => "int",
                "Int64" => "bigint",
                "Int16" => "smallint",
                "Boolean" => "bit",
                "Decimal" => "decimal",
                "Double" => "float",
                "Single" => "real",
                "DateTime" => "datetime",
                "DateTimeOffset" => "datetimeoffset",
                "DateOnly" => "date",
                "TimeOnly" => "time",
                "TimeSpan" => "time",
                "Guid" => "uniqueidentifier",
                "Byte[]" => "varbinary",
                _ => "nvarchar"  // Default fallback
            };
        }

        /// <summary>
        /// Generates XML for an association in the storage model.
        /// </summary>
        /// <param name="association">The association to convert to XML.</param>
        /// <returns>An <see cref="XElement"/> representing the storage association.</returns>
        /// <remarks>
        /// Storage associations represent foreign key constraints and use FK_ naming prefix.
        /// For self-referencing relationships, uses unique role names to avoid duplicate symbol errors.
        /// </remarks>
        private XElement GenerateStorageAssociation(EdmxAssociation association)
        {
            // For storage model, use FK_ prefix with proper naming convention
            var name = association.Name.StartsWith("FK_") ? association.Name : $"FK_{association.Name}";
            
            var associationElement = new XElement(_ssdlNs + "Association",
                new XAttribute("Name", name)
            );

            // Check if this is a self-referencing relationship
            var isSelfReferential = association.End1.Type == association.End2.Type;
            
            string end1Role, end2Role;
            var end1PluralType = _pluralizationService.Pluralize(association.End1.Type);
            var end2PluralType = _pluralizationService.Pluralize(association.End2.Type);
            
            if (isSelfReferential)
            {
                // For self-referencing relationships, use unique role names in storage model
                // to avoid "The symbol 'EntityName.EntityName' has already been defined" errors
                // Determine which is principal (0..1) and which is dependent (*)
                if (association.End1.Multiplicity == "0..1" || association.End1.Multiplicity == "1")
                {
                    end1Role = $"{end1PluralType}_Principal"; // Principal (parent) side
                    end2Role = $"{end2PluralType}_Dependent"; // Dependent (child) side
                }
                else
                {
                    end1Role = $"{end1PluralType}_Dependent"; // Dependent (child) side
                    end2Role = $"{end2PluralType}_Principal"; // Principal (parent) side
                }
            }
            else
            {
                // For regular relationships, use standard pluralized entity type names
                end1Role = end1PluralType;
                end2Role = end2PluralType;
            }
            
            associationElement.Add(new XElement(_ssdlNs + "End",
                new XAttribute("Role", end1Role),
                new XAttribute("Type", $"Self.{end1PluralType}"),
                new XAttribute("Multiplicity", association.End1.Multiplicity)
            ));

            associationElement.Add(new XElement(_ssdlNs + "End",
                new XAttribute("Role", end2Role),
                new XAttribute("Type", $"Self.{end2PluralType}"),
                new XAttribute("Multiplicity", association.End2.Multiplicity)
            ));

            // Add referential constraint
            if (association.ReferentialConstraint is not null)
            {
                var constraintElement = new XElement(_ssdlNs + "ReferentialConstraint");
                
                // Map conceptual role names to storage role names
                var principalStorageRole = association.ReferentialConstraint.Principal.Role == association.End1.Role 
                    ? end1Role : end2Role;
                    
                var dependentStorageRole = association.ReferentialConstraint.Dependent.Role == association.End1.Role 
                    ? end1Role : end2Role;
                
                // Principal role
                var principalElement = new XElement(_ssdlNs + "Principal",
                    new XAttribute("Role", principalStorageRole)
                );
                
                // Find the principal entity type to map property names to store column names
                var principalEntityType = _model.EntityTypes.FirstOrDefault(e => e.Name == association.End1.Type);
                if (principalEntityType == null)
                {
                    principalEntityType = _model.EntityTypes.FirstOrDefault(e => e.Name == association.End2.Type);
                }
                
                foreach (var propertyRef in association.ReferentialConstraint.Principal.PropertyRefs)
                {
                    // Map property name to store column name
                    var property = principalEntityType?.Properties.FirstOrDefault(p => p.Name == propertyRef);
                    var columnName = property != null && !string.IsNullOrEmpty(property.StoreColumnName) 
                        ? property.StoreColumnName 
                        : propertyRef;
                    
                    principalElement.Add(new XElement(_ssdlNs + "PropertyRef",
                        new XAttribute("Name", columnName)
                    ));
                }
                
                // Dependent role
                var dependentElement = new XElement(_ssdlNs + "Dependent",
                    new XAttribute("Role", dependentStorageRole)
                );
                
                // Find the dependent entity type to map property names to store column names
                var dependentEntityType = _model.EntityTypes.FirstOrDefault(e => e.Name == association.End2.Type);
                if (association.End1.Role == association.ReferentialConstraint.Dependent.Role)
                {
                    dependentEntityType = _model.EntityTypes.FirstOrDefault(e => e.Name == association.End1.Type);
                }
                
                foreach (var propertyRef in association.ReferentialConstraint.Dependent.PropertyRefs)
                {
                    // Map property name to store column name
                    var property = dependentEntityType?.Properties.FirstOrDefault(p => p.Name == propertyRef);
                    var columnName = property != null && !string.IsNullOrEmpty(property.StoreColumnName) 
                        ? property.StoreColumnName 
                        : propertyRef;
                    
                    dependentElement.Add(new XElement(_ssdlNs + "PropertyRef",
                        new XAttribute("Name", columnName)
                    ));
                }
                
                constraintElement.Add(principalElement);
                constraintElement.Add(dependentElement);
                associationElement.Add(constraintElement);
            }

            return associationElement;
        }

        /// <summary>
        /// Generates the entity container section of the storage model.
        /// </summary>
        /// <returns>An <see cref="XElement"/> representing the storage entity container.</returns>
        /// <remarks>
        /// The storage entity container groups all entity sets and association sets for the storage model.
        /// In SSDL: EntitySets are plural and reference plural EntityTypes.
        /// </remarks>
        private XElement GenerateStorageEntityContainer()
        {
            var container = new XElement(_ssdlNs + "EntityContainer",
                new XAttribute("Name", $"{_model.ContainerName}StoreContainer")
            );

            // Add all entity sets - PLURAL names referencing PLURAL entity types
            foreach (var entitySet in _model.EntitySets)
            {
                var pluralEntityTypeName = _pluralizationService.Pluralize(entitySet.EntityTypeName);
                
                container.Add(new XElement(_ssdlNs + "EntitySet",
                    new XAttribute("Name", entitySet.Name), // Already plural
                    new XAttribute("EntityType", $"Self.{pluralEntityTypeName}"), // Plural entity type
                    new XAttribute("Schema", entitySet.Schema),
                    new XAttribute(_storeNs + "Type", "Tables")
                ));
            }

            // Add all association sets
            foreach (var associationSet in _model.AssociationSets)
            {
                var associationName = associationSet.Association.StartsWith("FK_") ? 
                    associationSet.Association : $"FK_{associationSet.Association}";

                var assocSetElement = new XElement(_ssdlNs + "AssociationSet",
                    new XAttribute("Name", associationSet.Name),
                    new XAttribute("Association", $"Self.{associationName}")
                );

                // Use consistent role names matching the association generation
                var association = _model.Associations.FirstOrDefault(a => a.Name == associationSet.Association);
                if (association is not null)
                {
                    var end1PluralType = _pluralizationService.Pluralize(association.End1.Type);
                    var end2PluralType = _pluralizationService.Pluralize(association.End2.Type);
                    
                    // Check if this is a self-referencing relationship
                    var isSelfReferential = association.End1.Type == association.End2.Type;
                    
                    string end1Role, end2Role;
                    if (isSelfReferential)
                    {
                        // Use same unique role naming logic as in association generation
                        if (association.End1.Multiplicity == "0..1" || association.End1.Multiplicity == "1")
                        {
                            end1Role = $"{end1PluralType}_Principal";
                            end2Role = $"{end2PluralType}_Dependent";
                        }
                        else
                        {
                            end1Role = $"{end1PluralType}_Dependent";
                            end2Role = $"{end2PluralType}_Principal";
                        }
                    }
                    else
                    {
                        end1Role = end1PluralType;
                        end2Role = end2PluralType;
                    }
                    
                    assocSetElement.Add(new XElement(_ssdlNs + "End",
                        new XAttribute("Role", end1Role),
                        new XAttribute("EntitySet", associationSet.End1.EntitySet)
                    ));

                    assocSetElement.Add(new XElement(_ssdlNs + "End",
                        new XAttribute("Role", end2Role),
                        new XAttribute("EntitySet", associationSet.End2.EntitySet)
                    ));
                }

                container.Add(assocSetElement);
            }

            return container;
        }

        /// <summary>
        /// Generates the mappings section of the EDMX XML.
        /// </summary>
        /// <returns>An <see cref="XElement"/> representing the mapping section.</returns>
        /// <remarks>
        /// The mappings section defines how the conceptual model maps to the storage model.
        /// This implementation generates entity set mappings that connect CSDL entities (singular)
        /// to SSDL EntitySets (plural).
        /// </remarks>
        private XElement GenerateMappings()
        {
            var mapping = new XElement(_mappingNs + "Mapping",
                new XAttribute("Space", "C-S")
            );

            var containerMapping = new XElement(_mappingNs + "EntityContainerMapping",
                new XAttribute("StorageEntityContainer", $"{_model.ContainerName}StoreContainer"),
                new XAttribute("CdmEntityContainer", _model.ContainerName)
            );

            // Generate entity set mappings
            foreach (var entityType in _model.EntityTypes)
            {
                var entitySetMapping = GenerateEntitySetMapping(entityType);
                if (entitySetMapping is not null)
                {
                    containerMapping.Add(entitySetMapping);
                }
            }

            mapping.Add(containerMapping);
            return mapping;
        }

        /// <summary>
        /// Generates entity set mapping for a single entity type.
        /// </summary>
        /// <param name="entityType">The entity type to map.</param>
        /// <returns>An <see cref="XElement"/> representing the entity set mapping.</returns>
        /// <remarks>
        /// Maps CSDL entities (singular) to SSDL EntitySets (plural).
        /// </remarks>
        private XElement GenerateEntitySetMapping(EdmxEntityType entityType)
        {
            var entitySet = _model.EntitySets.FirstOrDefault(es => es.EntityTypeName == entityType.Name);
            if (entitySet is null)
            {
                return null;
            }

            var entitySetMapping = new XElement(_mappingNs + "EntitySetMapping",
                new XAttribute("Name", entitySet.Name) // Plural entity set name
            );

            var entityTypeMapping = new XElement(_mappingNs + "EntityTypeMapping",
                new XAttribute("TypeName", $"{_model.Namespace}.{entityType.Name}") // Singular entity type name
            );

            // StoreEntitySet should reference the SSDL EntitySet name (plural)
            var mappingFragment = new XElement(_mappingNs + "MappingFragment",
                new XAttribute("StoreEntitySet", entitySet.Name) // Plural entity set name
            );

            // Map scalar properties - CLR property names to database column names
            foreach (var property in entityType.Properties)
            {
                // Use StoreColumnName for the database column, Name for the CLR property
                var columnName = !string.IsNullOrEmpty(property.StoreColumnName) ? property.StoreColumnName : property.Name;
                mappingFragment.Add(new XElement(_mappingNs + "ScalarProperty",
                    new XAttribute("Name", property.Name),
                    new XAttribute("ColumnName", columnName)
                ));
            }

            entityTypeMapping.Add(mappingFragment);
            entitySetMapping.Add(entityTypeMapping);
            return entitySetMapping;
        }

        /// <summary>
        /// Generates the Designer section of the EDMX XML.
        /// </summary>
        /// <returns>An <see cref="XElement"/> representing the designer section.</returns>
        /// <remarks>
        /// The Designer section contains Visual Studio Entity Designer metadata, connection settings,
        /// and EasyAF-specific extensions including the complete OnModelCreating method.
        /// </remarks>
        private XElement GenerateDesignerSection()
        {
            var designer = new XElement(_edmxNs + "Designer",
                new XAttribute("xmlns", "http://schemas.microsoft.com/ado/2009/11/edmx"),
                new XAttribute(XNamespace.Xmlns + "easyaf", _easyafNs.NamespaceName)
            );

            // Add connection settings
            designer.Add(GenerateConnectionSettings());

            // Add designer options
            designer.Add(GenerateDesignerOptions());

            // Add EasyAF extensions
            designer.Add(GenerateEasyAFExtensions());

            // Note: Diagrams section omitted when empty to comply with EDMX schema validation
            // The EDMX schema expects Designer child elements to be in "##other" namespace
            // Empty Diagrams elements can cause validation warnings, so we omit them

            return designer;
        }

        /// <summary>
        /// Generates the connection settings section of the Designer.
        /// </summary>
        /// <returns>An <see cref="XElement"/> representing the connection settings.</returns>
        private XElement GenerateConnectionSettings()
        {
            var defaultNs = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edmx");
            
            return new XElement(defaultNs + "Connection",
                new XElement(defaultNs + "DesignerInfoPropertySet",
                    new XElement(defaultNs + "DesignerProperty",
                        new XAttribute("Name", "MetadataArtifactProcessing"),
                        new XAttribute("Value", "EmbedInOutputAssembly")
                    )
                )
            );
        }

        /// <summary>
        /// Generates the designer options section of the Designer.
        /// </summary>
        /// <returns>An <see cref="XElement"/> representing the designer options.</returns>
        private XElement GenerateDesignerOptions()
        {
            var defaultNs = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edmx");
            
            return new XElement(defaultNs + "Options",
                new XElement(defaultNs + "DesignerInfoPropertySet",
                    new XElement(defaultNs + "DesignerProperty",
                        new XAttribute("Name", "ValidateOnBuild"),
                        new XAttribute("Value", "true")
                    ),
                    new XElement(defaultNs + "DesignerProperty",
                        new XAttribute("Name", "EnablePluralization"),
                        new XAttribute("Value", "true")
                    ),
                    new XElement(defaultNs + "DesignerProperty",
                        new XAttribute("Name", "IncludeForeignKeysInModel"),
                        new XAttribute("Value", "true")
                    ),
                    new XElement(defaultNs + "DesignerProperty",
                        new XAttribute("Name", "UseLegacyProvider"),
                        new XAttribute("Value", "false")
                    ),
                    new XElement(defaultNs + "DesignerProperty",
                        new XAttribute("Name", "CodeGenerationStrategy"),
                        new XAttribute("Value", "None")
                    )
                )
            );
        }

        /// <summary>
        /// Generates the EasyAF extensions section containing OnModelCreating method body.
        /// </summary>
        /// <returns>An <see cref="XElement"/> representing the EasyAF extensions.</returns>
        private XElement GenerateEasyAFExtensions()
        {
            var extensions = new XElement(_easyafNs + "Extensions");

            // Add OnModelCreating method body if available
            if (!string.IsNullOrWhiteSpace(_model.OnModelCreatingBody))
            {
                extensions.Add(new XElement(_easyafNs + "OnModelCreating",
                    new XCData(_model.OnModelCreatingBody)
                ));
            }

            return extensions;
        }


        #endregion

    }

}
