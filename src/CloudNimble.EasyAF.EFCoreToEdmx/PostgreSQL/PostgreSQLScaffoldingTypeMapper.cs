using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;

namespace CloudNimble.EasyAF.EFCoreToEdmx.PostgreSQL
{
    /// <summary>
    /// Custom relational type mapping source for PostgreSQL that ensures timestamp with time zone 
    /// columns are mapped to DateTimeOffset instead of DateTime.
    /// </summary>
    /// <remarks>
    /// This mapper provides correct type mapping for PostgreSQL timestamp with time zone columns, 
    /// which should be DateTimeOffset in C# to preserve timezone information.
    /// </remarks>
    public class PostgreSQLRelationalTypeMappingSource : RelationalTypeMappingSource
    {
        private readonly IRelationalTypeMappingSource _defaultSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSQLRelationalTypeMappingSource"/> class.
        /// </summary>
        /// <param name="dependencies">The type mapping source dependencies.</param>
        /// <param name="relationalDependencies">The relational type mapping source dependencies.</param>
        /// <param name="defaultSource">The default type mapping source to wrap.</param>
        public PostgreSQLRelationalTypeMappingSource(
            TypeMappingSourceDependencies dependencies,
            RelationalTypeMappingSourceDependencies relationalDependencies,
            IRelationalTypeMappingSource defaultSource)
            : base(dependencies, relationalDependencies)
        {
            _defaultSource = defaultSource ?? throw new ArgumentNullException(nameof(defaultSource));
        }


        /// <summary>
        /// Finds a type mapping for the given CLR type.
        /// </summary>
        /// <param name="type">The CLR type to find a mapping for.</param>
        /// <returns>The type mapping, or null if none was found.</returns>
        public override RelationalTypeMapping FindMapping(Type type)
        {
            try
            {
                return _defaultSource.FindMapping(type);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PostgreSQL type mapping for type '{type?.Name}': {ex.Message}");
                return base.FindMapping(type);
            }
        }

        /// <summary>
        /// Finds a type mapping for the given database store type name.
        /// </summary>
        /// <param name="storeTypeName">The store type name to find a mapping for.</param>
        /// <returns>The type mapping, or null if none was found.</returns>
        public override RelationalTypeMapping FindMapping(string storeTypeName)
        {
            try
            {
                // Handle PostgreSQL timestamp with time zone -> DateTimeOffset
                if (IsTimestampWithTimeZone(storeTypeName))
                {
                    Console.WriteLine($"PostgreSQL type mapping: Mapping store type '{storeTypeName}' to DateTimeOffset");
                    var dateTimeOffsetMapping = _defaultSource.FindMapping(typeof(DateTimeOffset));
                    if (dateTimeOffsetMapping is not null)
                    {
                        return dateTimeOffsetMapping;
                    }
                    Console.WriteLine($"Warning: Could not find DateTimeOffset mapping for '{storeTypeName}', falling back to default");
                }

                return _defaultSource.FindMapping(storeTypeName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PostgreSQL type mapping for '{storeTypeName}': {ex.Message}");
                // Fall back to base implementation if there's any error
                return base.FindMapping(storeTypeName);
            }
        }

        /// <summary>
        /// Finds a type mapping for the given CLR type and database store type name.
        /// </summary>
        /// <param name="type">The CLR type to find a mapping for.</param>
        /// <param name="storeTypeName">The store type name to find a mapping for.</param>
        /// <returns>The type mapping, or null if none was found.</returns>
        public RelationalTypeMapping FindMapping(Type type, string storeTypeName)
        {
            try
            {
                // Handle PostgreSQL timestamp with time zone -> DateTimeOffset
                if (IsTimestampWithTimeZone(storeTypeName))
                {
                    Console.WriteLine($"PostgreSQL type mapping: Forcing DateTimeOffset for store type '{storeTypeName}' instead of {type?.Name}");
                    var mapping = _defaultSource.FindMapping(typeof(DateTimeOffset), storeTypeName);
                    if (mapping is not null)
                    {
                        return mapping;
                    }
                    Console.WriteLine($"Warning: Could not find DateTimeOffset mapping for '{storeTypeName}' with type override, falling back to default");
                }

                return _defaultSource.FindMapping(type, storeTypeName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PostgreSQL type mapping for type '{type?.Name}' and store type '{storeTypeName}': {ex.Message}");
                // Try base class implementation as fallback
                try 
                {
                    return base.FindMapping(storeTypeName);
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Finds a type mapping for the given entity property.
        /// </summary>
        /// <param name="property">The property to find a mapping for.</param>
        /// <returns>The type mapping, or null if none was found.</returns>
        public override RelationalTypeMapping FindMapping(IProperty property)
        {
            try
            {
                return _defaultSource.FindMapping(property);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PostgreSQL type mapping for property '{property?.Name}': {ex.Message}");
                var baseMapping = base.FindMapping(property);
                return baseMapping as RelationalTypeMapping;
            }
        }

        /// <summary>
        /// Finds a type mapping for the given CLR type and model.
        /// </summary>
        /// <param name="type">The CLR type to find a mapping for.</param>
        /// <param name="model">The entity model.</param>
        /// <param name="elementMapping">The element mapping for collection types.</param>
        /// <returns>The type mapping, or null if none was found.</returns>
        public override RelationalTypeMapping FindMapping(Type type, IModel model, CoreTypeMapping elementMapping = null)
        {
            try
            {
                return _defaultSource.FindMapping(type, model, elementMapping);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PostgreSQL type mapping for type '{type?.Name}' with model: {ex.Message}");
                var baseMapping = base.FindMapping(type, model, elementMapping);
                return baseMapping as RelationalTypeMapping;
            }
        }

        /// <summary>
        /// Finds a collection type mapping for the given mapping information.
        /// This override prevents null reference exceptions when EF Core tries to map PostgreSQL array types.
        /// </summary>
        /// <param name="info">The mapping information.</param>
        /// <param name="modelType">The model CLR type.</param>
        /// <param name="providerType">The provider CLR type (may be null for unknown types).</param>
        /// <param name="elementMapping">The element type mapping.</param>
        /// <returns>The collection type mapping, or null if not found.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "Required to fix null reference exception in collection mapping for PostgreSQL types.")]
        protected override RelationalTypeMapping FindCollectionMapping(
            RelationalTypeMappingInfo info, 
            Type modelType, 
            Type providerType, 
            CoreTypeMapping elementMapping)
        {
            try
            {
                // Check for null providerType which causes the original null reference exception
                if (providerType is null)
                {
                    Console.WriteLine($"PostgreSQL collection mapping: providerType is null for modelType '{modelType?.Name}', store type '{info.StoreTypeName}' - skipping collection mapping");
                    return null;
                }

                // Check if this is a PostgreSQL array type or user-defined type that we should handle specially
                if (!string.IsNullOrEmpty(info.StoreTypeName))
                {
                    var storeType = info.StoreTypeName.ToLowerInvariant();
                    
                    // Handle known PostgreSQL array types that might cause issues
                    if (storeType.Contains("[]") || storeType.Contains("array") || storeType.StartsWith("_"))
                    {
                        Console.WriteLine($"PostgreSQL collection mapping: Detected array type '{info.StoreTypeName}' for '{modelType?.Name}' - attempting safe mapping");
                        
                        // Try to get the base type mapping first
                        try
                        {
                            return base.FindCollectionMapping(info, modelType, providerType, elementMapping);
                        }
                        catch (Exception baseEx)
                        {
                            Console.WriteLine($"PostgreSQL collection mapping: Base collection mapping failed for '{info.StoreTypeName}': {baseEx.Message}");
                            return null;
                        }
                    }

                    // Handle user-defined types or enums
                    if (storeType.Contains("enum") || storeType.Contains("user-defined"))
                    {
                        Console.WriteLine($"PostgreSQL collection mapping: Detected user-defined type '{info.StoreTypeName}' - skipping collection mapping");
                        return null;
                    }
                }

                // For standard types, delegate to the base implementation
                return base.FindCollectionMapping(info, modelType, providerType, elementMapping);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PostgreSQL collection mapping for store type '{info.StoreTypeName}', model type '{modelType?.Name}': {ex.Message}");
                // Always return null instead of propagating the exception to prevent scaffolding failures
                return null;
            }
        }

        /// <summary>
        /// Determines if the given store type name represents a PostgreSQL timestamp with time zone type.
        /// </summary>
        /// <param name="storeTypeName">The store type name to check.</param>
        /// <returns>True if it's a timestamp with time zone type, false otherwise.</returns>
        private static bool IsTimestampWithTimeZone(string storeTypeName)
        {
            if (string.IsNullOrEmpty(storeTypeName))
                return false;

            // Exact matches
            if (string.Equals(storeTypeName, "timestamp with time zone", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(storeTypeName, "timestamptz", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Handle variations with precision modifiers like "timestamp(6) with time zone"
            if (storeTypeName.Contains("timestamp", StringComparison.OrdinalIgnoreCase) &&
                storeTypeName.Contains("with time zone", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
