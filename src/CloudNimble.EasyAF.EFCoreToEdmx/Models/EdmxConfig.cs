using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.EFCoreToEdmx.Models
{

    /// <summary>
    /// Configuration settings for EDMX generation from database scaffolding.
    /// </summary>
    /// <remarks>
    /// This configuration is stored in an .edmx.config file and contains all the settings
    /// needed to reproduce EDMX generation from a database schema. The configuration supports
    /// either table inclusion or exclusion lists, but not both simultaneously.
    /// </remarks>
    public class EdmxConfig
    {

        /// <summary>
        /// Gets or sets the source location for the database connection string.
        /// </summary>
        /// <value>
        /// A connection string source in the format "filename:section:key" 
        /// (e.g., "appsettings.json:ConnectionStrings:DefaultConnection").
        /// </value>
        /// <remarks>
        /// This specifies where to find the connection string in configuration files.
        /// The actual connection string is never stored in this configuration file.
        /// </remarks>
        public string ConnectionStringSource { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name for the generated DbContext class.
        /// </summary>
        /// <value>
        /// The class name for the generated DbContext.
        /// Defaults to "GeneratedDbContext".
        /// </value>
        /// <remarks>
        /// This is the name of the temporary DbContext class created during scaffolding.
        /// The final DbContext name in generated code will be determined by the code generation tools.
        /// </remarks>
        public string ContextName { get; set; } = "GeneratedDbContext";

        /// <summary>
        /// Gets or sets the namespace for the generated DbContext.
        /// </summary>
        /// <value>
        /// The namespace to use for the generated DbContext class.
        /// Defaults to an empty string, which will use the project's root namespace.
        /// </value>
        /// <remarks>
        /// This namespace will be used for the temporary DbContext class during EDMX generation.
        /// Typically matches the .Data project namespace (e.g., "MyApp.Data").
        /// </remarks>
        public string DbContextNamespace { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of tables to exclude during scaffolding.
        /// </summary>
        /// <value>
        /// A list of table names to exclude. These tables will be ignored during scaffolding.
        /// Defaults to null.
        /// </value>
        /// <remarks>
        /// Use either IncludedTables or ExcludedTables, but not both. When ExcludedTables
        /// is specified, all tables except the listed ones will be processed. System tables
        /// like __EFMigrationsHistory are automatically excluded and don't need to be listed here.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> ExcludedTables { get; set; }

        /// <summary>
        /// Gets or sets the list of tables to include during scaffolding.
        /// </summary>
        /// <value>
        /// A list of table names to include. If specified, only these tables will be scaffolded.
        /// Defaults to null, meaning all tables will be included.
        /// </value>
        /// <remarks>
        /// Use either IncludedTables or ExcludedTables, but not both. When IncludedTables
        /// is specified, only the listed tables will be processed. When null or empty,
        /// all tables except those in ExcludedTables will be processed.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> IncludedTables { get; set; }

        /// <summary>
        /// Gets or sets the namespace for the generated entity objects.
        /// </summary>
        /// <value>
        /// The namespace to use for generated entity classes.
        /// Defaults to an empty string, which will use the project's root namespace.
        /// </value>
        /// <remarks>
        /// This namespace will be used for the temporary entity classes during EDMX generation.
        /// Typically matches the .Core project namespace (e.g., "MyApp.Core").
        /// The final namespace in generated code will be determined by the code generation tools.
        /// </remarks>
        public string ObjectsNamespace { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the database provider type.
        /// </summary>
        /// <value>
        /// The provider identifier: "SqlServer" or "PostgreSQL".
        /// </value>
        /// <remarks>
        /// This determines which Entity Framework Core provider package and 
        /// provider-specific configurations will be used during scaffolding.
        /// </remarks>
        public string Provider { get; set; } = "SqlServer";

        /// <summary>
        /// Gets or sets a value indicating whether to use data annotations on generated entities.
        /// </summary>
        /// <value>
        /// <c>true</c> to generate data annotations; otherwise, <c>false</c> to use only fluent API.
        /// Defaults to <c>true</c>.
        /// </value>
        /// <remarks>
        /// When enabled, generates attributes like [Key], [Required], [MaxLength], etc. on entity properties.
        /// When disabled, all configuration is done through fluent API in OnModelCreating.
        /// </remarks>
        public bool UseDataAnnotations { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to use the Entity Framework pluralization service.
        /// </summary>
        /// <value>
        /// <c>true</c> to use pluralization for entity and property names; otherwise, <c>false</c>.
        /// Defaults to <c>true</c>.
        /// </value>
        /// <remarks>
        /// When enabled, table names like "Users" will generate entity classes named "User",
        /// and foreign key relationships will use appropriately pluralized navigation property names.
        /// </remarks>
        public bool UsePluralizer { get; set; } = true;

        /// <summary>
        /// Gets or sets a dictionary of pluralization overrides that map table names to desired entity names.
        /// </summary>
        /// <value>
        /// A dictionary where keys are table names and values are the desired entity names.
        /// Defaults to null, meaning no overrides are applied.
        /// </value>
        /// <remarks>
        /// This allows overriding the default pluralization behavior for specific tables.
        /// For example, mapping "FileMetadata" → "FileMetadata" prevents the pluralizer from 
        /// incorrectly converting it to "FileMetadatum", or "People" → "Person" overrides
        /// the standard pluralization when a different entity name is desired.
        /// These overrides take precedence over both the standard pluralizer and table naming.
        /// </remarks>
        /// <example>
        /// <code>
        /// {
        ///   "FileMetadata": "FileMetadata",
        ///   "People": "Person"
        /// }
        /// </code>
        /// </example>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string> PluralizationOverrides { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of property name overrides that map database column names to CLR property names.
        /// </summary>
        /// <value>
        /// A nested dictionary where the outer key is the entity/table name, and the inner dictionary
        /// maps database column names to desired CLR property names.
        /// Defaults to null, meaning no property name overrides are applied.
        /// </value>
        /// <remarks>
        /// This allows fine-grained control over property naming when the database column names
        /// don't match desired C# property naming conventions. When specified, the scaffolder will
        /// generate HasColumnName() calls in OnModelCreating to map the CLR properties to their
        /// database column names. The outer key should be the entity name (after pluralization),
        /// not the table name.
        /// </remarks>
        /// <example>
        /// <code>
        /// {
        ///   "NationalStockNumbers": {
        ///     "NIIN": "Niin",
        ///     "FSC": "Fsc",
        ///     "INC": "Inc",
        ///     "SOS": "Sos"
        ///   },
        ///   "Agents": {
        ///     "SSN": "Ssn",
        ///     "Person": "Persona"
        ///   }
        /// }
        /// </code>
        /// </example>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, Dictionary<string, string>> PropertyNameOverrides { get; set; }

    }

}
