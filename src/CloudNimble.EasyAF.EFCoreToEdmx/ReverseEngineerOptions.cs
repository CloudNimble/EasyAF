using System.Collections.Generic;

namespace CloudNimble.EasyAF.EFCoreToEdmx
{
    /// <summary>
    /// Options for reverse engineering databases into Entity Framework models.
    /// </summary>
    internal class ReverseEngineerOptions
    {
        /// <summary>
        /// Gets or sets the database connection string.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the generated DbContext class.
        /// </summary>
        public string ContextName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the namespace for the generated DbContext.
        /// </summary>
        public string ContextNamespace { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the namespace for the generated entity classes.
        /// </summary>
        public string ModelNamespace { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to disable pluralization of entity names.
        /// </summary>
        public bool NoPluralize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use data annotations instead of fluent API.
        /// </summary>
        public bool UseDataAnnotations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite existing files.
        /// </summary>
        public bool OverwriteFiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use database names directly.
        /// </summary>
        public bool UseDatabaseNames { get; set; }

        /// <summary>
        /// Gets or sets the list of tables to include in scaffolding.
        /// When null, all tables (except system tables) will be included.
        /// </summary>
        public IList<string> Tables { get; set; }

        /// <summary>
        /// Gets or sets the list of schemas to include in scaffolding.
        /// </summary>
        public IList<string> Schemas { get; set; }
    }
}
