namespace CloudNimble.EasyAF.EFCoreToEdmx.Models
{
    /// <summary>
    /// Represents a scalar property in an EDMX entity type, corresponding to a database column.
    /// </summary>
    public class EdmxProperty
    {

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the CLR type for the conceptual model (e.g., "String", "Int32").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SQL type for the storage model (e.g., "nvarchar", "int").
        /// </summary>
        public string SqlType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the property can contain null values.
        /// </summary>
        public bool Nullable { get; set; }

        /// <summary>
        /// Gets or sets the maximum length for string and binary properties.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the precision for decimal and numeric properties.
        /// </summary>
        public int? Precision { get; set; }

        /// <summary>
        /// Gets or sets the scale for decimal and numeric properties.
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property has a fixed length.
        /// </summary>
        public bool? IsFixedLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether string properties support Unicode characters.
        /// </summary>
        public bool? IsUnicode { get; set; }

        /// <summary>
        /// Gets or sets the documentation comment for the property.
        /// </summary>
        public string Documentation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the default value for the property.
        /// </summary>
        public string DefaultValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the store-generated pattern for the property.
        /// </summary>
        public string StoreGeneratedPattern { get; set; } = "None";

        /// <summary>
        /// Gets or sets whether this property should include default values in the conceptual model.
        /// </summary>
        public bool IncludeDefaultInConceptual { get; set; } = false;

        /// <summary>
        /// Gets or sets the actual database column name for the storage model.
        /// This may differ from the Name property which represents the CLR property name.
        /// </summary>
        public string StoreColumnName { get; set; } = string.Empty;

    }

}
