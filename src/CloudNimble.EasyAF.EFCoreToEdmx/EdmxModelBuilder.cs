using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudNimble.EasyAF.EFCoreToEdmx
{

    /// <summary>
    /// Builds EDMX model structure from Entity Framework Core metadata.
    /// </summary>
    /// <remarks>
    /// This class is responsible for extracting metadata from EF Core's <see cref="IModel"/> 
    /// and converting it into a structured EDMX model representation. It handles entity types,
    /// properties, relationships, keys, and all associated metadata including documentation
    /// and database-specific annotations. Uses EF Core's pluralization service for consistent
    /// entity set naming.
    /// </remarks>
    public class EdmxModelBuilder
    {

        #region Fields

        private readonly IPluralizer _pluralizer;
        private readonly Dictionary<string, string> _pluralizationOverrides;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmxModelBuilder"/> class.
        /// </summary>
        /// <param name="pluralizer">The pluralization service for proper entity set naming. If null, EF Core's design-time service will be used.</param>
        /// <param name="pluralizationOverrides">Optional dictionary mapping table names to desired entity names, overriding default pluralization behavior.</param>
        public EdmxModelBuilder(IPluralizer pluralizer = null, Dictionary<string, string> pluralizationOverrides = null)
        {
            _pluralizer = pluralizer ?? CreateEFCoreDesignTimePluralizer();
            _pluralizationOverrides = pluralizationOverrides ?? new Dictionary<string, string>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Builds a complete EDMX model from the specified Entity Framework Core model.
        /// </summary>
        /// <param name="efModel">The EF Core model to extract metadata from. Must not be null.</param>
        /// <param name="namespace">The namespace for the EDMX model.</param>
        /// <param name="name">The container name for the EDMX model.</param>
        /// <param name="providerType">The database provider type for the storage model.</param>
        /// <param name="tableInfos">Optional dictionary mapping entity type names to actual table names.</param>
        /// <param name="onModelCreatingBody">Optional complete OnModelCreating method from scaffolding.</param>
        /// <returns>A complete <see cref="EdmxModel"/> containing all extracted metadata.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="efModel"/> is null.</exception>
        /// <remarks>
        /// This method performs a comprehensive extraction of all model metadata including:
        /// - Entity types and their properties
        /// - Primary and foreign key relationships
        /// - Navigation properties
        /// - Property attributes (length, precision, scale, etc.)
        /// - Documentation and comments
        /// - Database-specific configurations
        /// Uses EF Core's pluralization service for consistent entity set naming.
        /// </remarks>
        public EdmxModel BuildEdmxModel(
            IModel efModel, 
            string @namespace = "DefaultNamespace", 
            string name = "DefaultContainer",
            DatabaseProviderType providerType = DatabaseProviderType.Unknown,
            Dictionary<string, string> tableInfos = null,
            string onModelCreatingBody = "")
        {

            ArgumentNullException.ThrowIfNull(efModel, nameof(efModel));

            var edmxModel = new EdmxModel
            {
                Namespace = @namespace,
                ContainerName = name,
                EntityTypes = [],
                Associations = [],
                EntitySets = [],
                AssociationSets = [],
                OnModelCreatingBody = onModelCreatingBody ?? string.Empty
            };

            // Build entity types and corresponding entity sets
            foreach (var entityType in efModel.GetEntityTypes())
            {

                var edmxEntityType = BuildEntityType(entityType);
                edmxModel.EntityTypes.Add(edmxEntityType);

                var edmxEntitySet = new EdmxEntitySet
                {

                    Name = GetEntitySetName(entityType),
                    EntityTypeName = edmxEntityType.Name

                };
                edmxModel.EntitySets.Add(edmxEntitySet);

            }

            // Build associations from foreign key relationships
            BuildAssociations(efModel, edmxModel);

            return edmxModel;

        }

        ///// <summary>
        ///// Builds a complete EDMX model from the specified Entity Framework Core model (legacy method).
        ///// </summary>
        ///// <param name="efModel">The EF Core model to extract metadata from. Must not be null.</param>
        ///// <param name="namespace">The namespace for the EDMX model.</param>
        ///// <param name="name">The container name for the EDMX model.</param>
        ///// <returns>A complete <see cref="EdmxModel"/> containing all extracted metadata.</returns>
        ///// <exception cref="ArgumentNullException">Thrown when <paramref name="efModel"/> is null.</exception>
        ///// <remarks>
        ///// This is a legacy overload maintained for backward compatibility.
        ///// New code should use the overload that accepts providerType and tableInfos parameters.
        ///// </remarks>
        //public EdmxModel BuildEdmxModel(IModel efModel, string @namespace = "DefaultNamespace", string name = "DefaultContainer")
        //{
        //    return BuildEdmxModel(efModel, @namespace, name, DatabaseProviderType.Unknown, null, string.Empty);
        //}

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
        private static IPluralizer CreateEFCoreDesignTimePluralizer()
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
        /// Builds an EDMX entity type from an EF Core entity type.
        /// </summary>
        /// <param name="entityType">The EF Core entity type to convert.</param>
        /// <returns>An <see cref="EdmxEntityType"/> containing all entity metadata.</returns>
        /// <remarks>
        /// Extracts all properties including scalar properties and navigation properties,
        /// identifies primary key properties, and preserves all property-level metadata.
        /// </remarks>
        private EdmxEntityType BuildEntityType(IEntityType entityType)
        {

            var edmxEntityType = new EdmxEntityType
            {

                Name = entityType.ClrType.Name,
                Documentation = GetEntityDocumentation(entityType),
                Properties = [],
                NavigationProperties = [],
                Keys = []

            };

            // Add scalar properties
            foreach (var property in entityType.GetProperties())
            {

                var edmxProperty = BuildProperty(property);
                edmxEntityType.Properties.Add(edmxProperty);

                // Check if this property is part of the primary key
                if (entityType.FindPrimaryKey()?.Properties.Contains(property) == true)
                {

                    edmxEntityType.Keys.Add(property.Name);

                }

            }

            // Add navigation properties
            foreach (var navigation in entityType.GetNavigations())
            {

                var edmxNavigation = BuildNavigationProperty(navigation);
                edmxEntityType.NavigationProperties.Add(edmxNavigation);

            }

            return edmxEntityType;

        }

        /// <summary>
        /// Builds an EDMX property from an EF Core property.
        /// </summary>
        /// <param name="property">The EF Core property to convert.</param>
        /// <returns>An <see cref="EdmxProperty"/> containing all property metadata.</returns>
        /// <remarks>
        /// Extracts all property attributes including type information, nullability,
        /// length constraints, precision and scale for numeric types, Unicode settings,
        /// default values, and generation patterns (Identity, Computed, etc.).
        /// Does not include default values in conceptual model following EDMX best practices.
        /// </remarks>
        private EdmxProperty BuildProperty(IProperty property)
        {
            // Get the actual database column name (may differ from CLR property name)
            string storeColumnName = property.Name; // Default to property name
            try
            {
                // Try to get the actual column name from the database mapping
                // GetColumnName() returns the database column name which may differ from the CLR property name
                var columnName = property.GetColumnName();
                if (!string.IsNullOrEmpty(columnName))
                {
                    storeColumnName = columnName;
                }
            }
            catch
            {
                // If GetColumnName() is not supported (e.g., non-relational provider), use property name
                storeColumnName = property.Name;
            }

            return new EdmxProperty
            {
                Name = property.Name,
                Type = GetClrTypeName(property),
                Nullable = property.IsNullable,
                MaxLength = property.GetMaxLength(),
                Precision = property.GetPrecision(),
                Scale = property.GetScale(),
                IsFixedLength = property.IsFixedLength(),
                IsUnicode = property.IsUnicode(),
                Documentation = GetPropertyDocumentation(property),
                // Don't include default values in conceptual model following EDMX best practices
                StoreGeneratedPattern = GetStoreGeneratedPattern(property),
                IncludeDefaultInConceptual = false, // Default to false per EDMX best practices
                StoreColumnName = storeColumnName // Store the actual database column name
            };
        }

        /// <summary>
        /// Builds an EDMX navigation property from an EF Core navigation property.
        /// </summary>
        /// <param name="navigation">The EF Core navigation property to convert.</param>
        /// <returns>An <see cref="EdmxNavigationProperty"/> containing navigation metadata.</returns>
        /// <remarks>
        /// Creates the navigation property with proper relationship references and role assignments
        /// based on whether the navigation is on the dependent or principal side of the relationship.
        /// For self-referencing relationships, improves upon EF Core's default naming conventions
        /// to provide more semantic and intuitive navigation property names.
        /// </remarks>
        private static EdmxNavigationProperty BuildNavigationProperty(INavigation navigation)
        {

            return new EdmxNavigationProperty
            {

                Name = GetImprovedNavigationPropertyName(navigation),
                Relationship = GetRelationshipName(navigation.ForeignKey),
                FromRole = GetFromRole(navigation),
                ToRole = GetToRole(navigation)

            };

        }

        /// <summary>
        /// Builds associations and association sets from foreign key relationships in the EF Core model.
        /// </summary>
        /// <param name="efModel">The EF Core model containing the relationships.</param>
        /// <param name="edmxModel">The EDMX model to add associations to.</param>
        /// <remarks>
        /// Processes all foreign key relationships in the model, creating corresponding associations
        /// and association sets. Each foreign key relationship results in one association that
        /// defines the relationship structure and constraints.
        /// </remarks>
        private void BuildAssociations(IModel efModel, EdmxModel edmxModel)
        {

            var processedForeignKeys = new HashSet<IForeignKey>();

            foreach (var entityType in efModel.GetEntityTypes())
            {

                foreach (var foreignKey in entityType.GetForeignKeys())
                {

                    if (processedForeignKeys.Contains(foreignKey))
                        continue;

                    var association = BuildAssociation(foreignKey);
                    edmxModel.Associations.Add(association);

                    var associationSet = BuildAssociationSet(foreignKey, edmxModel);
                    edmxModel.AssociationSets.Add(associationSet);

                    processedForeignKeys.Add(foreignKey);

                }

            }

        }

        /// <summary>
        /// Builds an EDMX association from an EF Core foreign key relationship.
        /// </summary>
        /// <param name="foreignKey">The EF Core foreign key to convert.</param>
        /// <returns>An <see cref="EdmxAssociation"/> representing the relationship.</returns>
        /// <remarks>
        /// Creates a complete association definition including both ends of the relationship,
        /// multiplicity constraints, and referential constraints that define how the
        /// foreign key properties map to primary key properties.
        /// </remarks>
        private static EdmxAssociation BuildAssociation(IForeignKey foreignKey)
        {

            return new EdmxAssociation
            {

                Name = GetRelationshipName(foreignKey),
                End1 = new EdmxAssociationEnd
                {

                    Role = GetPrincipalRoleName(foreignKey),
                    Type = foreignKey.PrincipalEntityType.ClrType.Name,
                    Multiplicity = GetPrincipalMultiplicity(foreignKey)

                },
                End2 = new EdmxAssociationEnd
                {

                    Role = GetDependentRoleName(foreignKey),
                    Type = foreignKey.DeclaringEntityType.ClrType.Name,
                    Multiplicity = GetDependentMultiplicity(foreignKey)

                },
                ReferentialConstraint = BuildReferentialConstraint(foreignKey)

            };

        }

        /// <summary>
        /// Builds an EDMX association set from an EF Core foreign key relationship.
        /// </summary>
        /// <param name="foreignKey">The EF Core foreign key to convert.</param>
        /// <param name="edmxModel">The EDMX model containing entity set references.</param>
        /// <returns>An <see cref="EdmxAssociationSet"/> representing the relationship instance.</returns>
        /// <remarks>
        /// Creates an association set that connects the association definition to the actual
        /// entity sets, defining which entity sets participate in the relationship.
        /// </remarks>
        private EdmxAssociationSet BuildAssociationSet(IForeignKey foreignKey, EdmxModel edmxModel)
        {

            return new EdmxAssociationSet
            {

                Name = GetRelationshipName(foreignKey) + "Set",
                Association = GetRelationshipName(foreignKey),
                End1 = new EdmxAssociationSetEnd
                {

                    Role = GetPrincipalRoleName(foreignKey),
                    EntitySet = GetEntitySetName(foreignKey.PrincipalEntityType)

                },
                End2 = new EdmxAssociationSetEnd
                {

                    Role = GetDependentRoleName(foreignKey),
                    EntitySet = GetEntitySetName(foreignKey.DeclaringEntityType)

                }

            };

        }

        /// <summary>
        /// Builds a referential constraint from an EF Core foreign key relationship.
        /// </summary>
        /// <param name="foreignKey">The EF Core foreign key to convert.</param>
        /// <returns>An <see cref="EdmxReferentialConstraint"/> defining the key relationships.</returns>
        /// <remarks>
        /// Creates the referential constraint that specifies which properties on the principal
        /// entity correspond to which properties on the dependent entity, ensuring referential
        /// integrity in the conceptual model.
        /// </remarks>
        private static EdmxReferentialConstraint BuildReferentialConstraint(IForeignKey foreignKey)
        {

            return new EdmxReferentialConstraint
            {

                Principal = new EdmxReferentialConstraintRole
                {

                    Role = GetPrincipalRoleName(foreignKey),
                    PropertyRefs = foreignKey.PrincipalKey.Properties.Select(p => p.Name).ToList()

                },
                Dependent = new EdmxReferentialConstraintRole
                {

                    Role = GetDependentRoleName(foreignKey),
                    PropertyRefs = foreignKey.Properties.Select(p => p.Name).ToList()

                }

            };

        }

        /// <summary>
        /// Maps a CLR type from an EF Core property to the corresponding CLR type name.
        /// </summary>
        /// <param name="property">The EF Core property containing type information.</param>
        /// <returns>A string representing the CLR type (e.g., "String", "Int32").</returns>
        /// <remarks>
        /// Handles both nullable and non-nullable types, mapping them to appropriate CLR type names.
        /// For EF6 compatibility, uses CLR type names without the "Edm." prefix.
        /// Includes special handling for PostgreSQL timestamp columns to map them to DateTimeOffset.
        /// </remarks>
        private string GetClrTypeName(IProperty property)
        {
            var clrType = property.ClrType;
            var underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;

            // Handle enums by getting their underlying type
            if (underlyingType.IsEnum)
            {
                underlyingType = Enum.GetUnderlyingType(underlyingType);
            }

            // Special handling for PostgreSQL timestamp with time zone columns
            // Check the store type (column type) from the database to determine if this should be DateTimeOffset
            // Only works for relational providers, so we need to safely handle non-relational providers like InMemory
            try
            {
                var storeType = property.GetColumnType();

                if (underlyingType == typeof(DateTime) && !string.IsNullOrEmpty(storeType))
                {
                    // For PostgreSQL timestamp with time zone columns, use DateTimeOffset in conceptual model
                    if (storeType.Equals("timestamp with time zone", StringComparison.OrdinalIgnoreCase) ||
                        storeType.Equals("timestamptz", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Converting PostgreSQL timestamptz column '{property.Name}' from DateTime to DateTimeOffset in conceptual model");
                        return "DateTimeOffset";
                    }

                    // Also check for variations that might include additional modifiers
                    if (storeType.Contains("timestamp with time zone", StringComparison.OrdinalIgnoreCase) ||
                        storeType.Contains("timestamptz", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Converting PostgreSQL timestamptz column '{property.Name}' (store type: {storeType}) from DateTime to DateTimeOffset in conceptual model");
                        return "DateTimeOffset";
                    }
                }

                // Special handling for PostgreSQL ltree type - map to String
                if (!string.IsNullOrEmpty(storeType) &&
                    storeType.Equals("ltree", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Converting PostgreSQL ltree column '{property.Name}' to String in conceptual model");
                    return "String";
                }
            }
            catch (InvalidCastException)
            {
                // Non-relational provider (like InMemory) - GetColumnType() is not supported
                // Fall through to use the original CLR type
            }

            return underlyingType.Name;
        }


        /// <summary>
        /// Determines the store-generated pattern for a property based on its value generation configuration.
        /// </summary>
        /// <param name="property">The EF Core property to analyze.</param>
        /// <returns>A string indicating the generation pattern: "Identity", "Computed", or "None".</returns>
        /// <remarks>
        /// Maps EF Core's ValueGenerated enumeration to EDMX store-generated pattern values.
        /// "Identity" indicates the value is generated on insert, "Computed" indicates the value
        /// is generated on insert and update, and "None" indicates no automatic generation.
        /// </remarks>
        private static string GetStoreGeneratedPattern(IProperty property)
        {
            return property.ValueGenerated switch
            {
                ValueGenerated.OnAdd => "Identity",
                ValueGenerated.OnAddOrUpdate => "Computed",
                _ => "None"
            };
        }

        /// <summary>
        /// Extracts documentation comments from property annotations.
        /// </summary>
        /// <param name="property">The EF Core property to extract documentation from.</param>
        /// <returns>The documentation string, or empty string if no documentation is found.</returns>
        /// <remarks>
        /// Searches for documentation in various annotation sources including generic relational
        /// comments, SQL Server-specific comments, and PostgreSQL-specific comments. This ensures
        /// compatibility across different database providers.
        /// </remarks>
        private static string GetPropertyDocumentation(IProperty property)
        {
            try
            {
                // Use EF Core's official GetComment() extension method
                // This properly retrieves comments set via HasComment() in OnModelCreating
                // Note: Requires the design-time model (IDesignTimeModel), not the read-optimized runtime model
                var comment = property.GetComment();
                if (!string.IsNullOrEmpty(comment))
                {
                    return comment;
                }
            }
            catch (InvalidOperationException)
            {
                // GetComment() requires the design-time model. If we have a read-optimized model,
                // fall back to checking annotations directly
            }

            // Fall back to checking annotations directly
            var annotation = property.FindAnnotation("Relational:Comment");
            return annotation?.Value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Extracts documentation comments from entity type annotations.
        /// </summary>
        /// <param name="entityType">The EF Core entity type to extract documentation from.</param>
        /// <returns>The documentation string, or empty string if no documentation is found.</returns>
        /// <remarks>
        /// Searches for documentation in various annotation sources including generic relational
        /// comments, SQL Server-specific comments, and PostgreSQL-specific comments. This ensures
        /// compatibility across different database providers and captures table-level comments.
        /// </remarks>
        private static string GetEntityDocumentation(IEntityType entityType)
        {
            try
            {
                // Use EF Core's official GetComment() extension method
                // This properly retrieves table comments set via HasComment() in OnModelCreating
                // Note: Requires the design-time model (IDesignTimeModel), not the read-optimized runtime model
                var comment = entityType.GetComment();
                if (!string.IsNullOrEmpty(comment))
                {
                    return comment;
                }
            }
            catch (InvalidOperationException)
            {
                // GetComment() requires the design-time model. If we have a read-optimized model,
                // fall back to checking annotations directly
            }

            // Fall back to checking annotations directly
            var annotation = entityType.FindAnnotation("Relational:Comment");
            return annotation?.Value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Generates an entity set name from an entity type using pluralization overrides, table name, or EF Core's pluralization service.
        /// </summary>
        /// <param name="entityType">The EF Core entity type.</param>
        /// <returns>The entity set name for the EDMX model.</returns>
        /// <remarks>
        /// Priority order for entity set naming:
        /// 1. Pluralization overrides (if table name exists in the override dictionary)
        /// 2. Actual database table name (if available)
        /// 3. EF Core's pluralization service applied to the entity type name
        /// This ensures custom naming takes precedence while maintaining backward compatibility.
        /// </remarks>
        private string GetEntitySetName(IEntityType entityType)
        {
            // First check for pluralization overrides using the actual table name
            var tableName = entityType.GetTableName();
            if (!string.IsNullOrWhiteSpace(tableName) && _pluralizationOverrides.TryGetValue(tableName, out var overrideName))
            {
                return overrideName;
            }

            // Use actual table name if available
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                return tableName;
            }

            // Use EF Core's pluralization service for consistent naming
            return _pluralizer.Pluralize(entityType.ClrType.Name);
        }

        /// <summary>
        /// Generates a unique relationship name from a foreign key.
        /// </summary>
        /// <param name="foreignKey">The EF Core foreign key relationship.</param>
        /// <returns>A unique name for the association.</returns>
        /// <remarks>
        /// Creates a composite name using the principal entity type, dependent entity type,
        /// and foreign key property names to ensure uniqueness across all relationships.
        /// For self-referential relationships, uses semantic names like "Parent" and "Children" for clarity.
        /// </remarks>
        private static string GetRelationshipName(IForeignKey foreignKey)
        {
            var isSelfReferential = IsSelfReferential(foreignKey);
            
            if (isSelfReferential)
            {
                // For self-referential relationships, use semantic names for better readability
                return $"{foreignKey.DeclaringEntityType.ClrType.Name}_Parent_Children_{string.Join("_", foreignKey.Properties.Select(p => p.Name))}";
            }

            return $"{foreignKey.PrincipalEntityType.ClrType.Name}_{foreignKey.DeclaringEntityType.ClrType.Name}_{string.Join("_", foreignKey.Properties.Select(p => p.Name))}";
        }

        /// <summary>
        /// Gets the principal role name from a foreign key relationship.
        /// </summary>
        /// <param name="foreignKey">The EF Core foreign key relationship.</param>
        /// <returns>The role name for the principal (referenced) entity.</returns>
        /// <remarks>
        /// For self-referential relationships, uses semantic names like "Parent" to provide
        /// clearer role identification in the EDMX output.
        /// </remarks>
        private static string GetPrincipalRoleName(IForeignKey foreignKey)
        {
            var isSelfReferential = IsSelfReferential(foreignKey);
            
            if (isSelfReferential)
            {
                // For self-referential relationships, use semantic role names
                return $"{foreignKey.PrincipalEntityType.ClrType.Name}_Parent";
            }

            return foreignKey.PrincipalEntityType.ClrType.Name;
        }

        /// <summary>
        /// Gets the dependent role name from a foreign key relationship.
        /// </summary>
        /// <param name="foreignKey">The EF Core foreign key relationship.</param>
        /// <returns>The role name for the dependent (referencing) entity.</returns>
        /// <remarks>
        /// For self-referential relationships, uses semantic names like "Children" to provide
        /// clearer role identification in the EDMX output.
        /// </remarks>
        private static string GetDependentRoleName(IForeignKey foreignKey)
        {
            var isSelfReferential = IsSelfReferential(foreignKey);
            
            if (isSelfReferential)
            {
                // For self-referential relationships, use semantic role names
                return $"{foreignKey.DeclaringEntityType.ClrType.Name}_Children";
            }

            return foreignKey.DeclaringEntityType.ClrType.Name;
        }

        /// <summary>
        /// Determines if a foreign key represents a self-referential relationship.
        /// </summary>
        /// <param name="foreignKey">The foreign key to check.</param>
        /// <returns>True if the relationship is self-referential, false otherwise.</returns>
        /// <remarks>
        /// A self-referential relationship occurs when the principal and dependent entity types are the same.
        /// This is common in hierarchical data structures like parent-child relationships on the same table.
        /// </remarks>
        private static bool IsSelfReferential(IForeignKey foreignKey)
        {
            return foreignKey.PrincipalEntityType == foreignKey.DeclaringEntityType;
        }

        /// <summary>
        /// Gets an improved navigation property name for EDMX generation, with special handling for self-referencing relationships.
        /// </summary>
        /// <param name="navigation">The EF Core navigation property.</param>
        /// <returns>An improved navigation property name that is more semantic and user-friendly.</returns>
        /// <remarks>
        /// For self-referencing relationships, EF Core's reverse engineering often generates poor names like "InverseParent".
        /// This method replaces them with more intuitive names:
        /// - Reference navigation (0..1): "Parent" 
        /// - Collection navigation (*): "Children"
        /// 
        /// For regular relationships, the original navigation property name is preserved.
        /// </remarks>
        private static string GetImprovedNavigationPropertyName(INavigation navigation)
        {
            var foreignKey = navigation.ForeignKey;
            
            // Only improve names for self-referencing relationships
            if (!IsSelfReferential(foreignKey))
            {
                return navigation.Name;
            }

            // For self-referencing relationships, provide better semantic names
            if (navigation.IsCollection)
            {
                // Collection navigation - use "Children" instead of confusing names like "InverseParent"
                return "Children";
            }
            else
            {
                // Reference navigation - use "Parent" (simple and clear)
                return "Parent";
            }
        }

        /// <summary>
        /// Determines the "from" role for a navigation property based on its direction.
        /// </summary>
        /// <param name="navigation">The EF Core navigation property.</param>
        /// <returns>The role name for the "from" side of the navigation.</returns>
        /// <remarks>
        /// Returns the dependent role if the navigation is on the dependent side,
        /// otherwise returns the principal role.
        /// </remarks>
        private static string GetFromRole(INavigation navigation)
        {
            return navigation.IsOnDependent ? 
                GetDependentRoleName(navigation.ForeignKey) : 
                GetPrincipalRoleName(navigation.ForeignKey);
        }

        /// <summary>
        /// Determines the "to" role for a navigation property based on its direction.
        /// </summary>
        /// <param name="navigation">The EF Core navigation property.</param>
        /// <returns>The role name for the "to" side of the navigation.</returns>
        /// <remarks>
        /// Returns the principal role if the navigation is on the dependent side,
        /// otherwise returns the dependent role.
        /// </remarks>
        private static string GetToRole(INavigation navigation)
        {
            return navigation.IsOnDependent ? 
                GetPrincipalRoleName(navigation.ForeignKey) : 
                GetDependentRoleName(navigation.ForeignKey);
        }

        /// <summary>
        /// Determines the multiplicity for the principal side of a relationship.
        /// </summary>
        /// <param name="foreignKey">The EF Core foreign key relationship.</param>
        /// <returns>The multiplicity string ("0..1" or "1") for the principal side.</returns>
        /// <remarks>
        /// For one-to-many relationships:
        /// - If FK is required (not nullable): returns "1" (dependent must reference exactly one principal)
        /// - If FK is nullable: returns "0..1" (dependent can exist without referencing any principal)
        /// For one-to-one relationships:
        /// - If FK is required: returns "1" (required one-to-one relationship)
        /// - If FK is nullable: returns "0..1" (optional one-to-one relationship)
        /// </remarks>
        private static string GetPrincipalMultiplicity(IForeignKey foreignKey)
        {
            // When FK is nullable, the dependent can exist without referencing the principal
            // Therefore, the principal side should be "0..1" regardless of relationship type
            if (!foreignKey.IsRequired)
            {
                return "0..1";
            }
            
            // When FK is required, the dependent must reference exactly one principal
            return "1";
        }

        /// <summary>
        /// Determines the multiplicity for the dependent side of a relationship.
        /// </summary>
        /// <param name="foreignKey">The EF Core foreign key relationship.</param>
        /// <returns>The multiplicity string ("*", "0..1", or "1") for the dependent side.</returns>
        /// <remarks>
        /// For one-to-many relationships (not unique): always returns "*" (many dependents can reference the same principal)
        /// For one-to-one relationships (unique):
        /// - If FK is required: returns "1" (required one-to-one)
        /// - If FK is nullable: returns "0..1" (optional one-to-one)
        /// </remarks>
        private static string GetDependentMultiplicity(IForeignKey foreignKey)
        {
            // If it's not unique, it's a one-to-many relationship
            if (!foreignKey.IsUnique)
            {
                // Many dependents can reference the same principal
                return "*";
            }
            
            // For unique relationships (one-to-one), check if it's required
            return foreignKey.IsRequired ? "1" : "0..1";
        }

        #endregion

    }

}
