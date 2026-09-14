using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.EFCoreToEdmx
{

    /// <summary>
    /// Copies the SQL Server <c>EasyAF_LongDescription</c> extended property into CSDL LongDescription,
    /// and preserves hand-authored CSDL docs when the catalog property is missing.
    /// </summary>
    internal static class EasyAFLongDescription
    {

        public const string PropertyName = "EasyAF_LongDescription";

        private const string Query = """
            SELECT
                OBJECT_SCHEMA_NAME(ep.major_id) AS [Schema],
                OBJECT_NAME(ep.major_id) AS [Table],
                COL_NAME(ep.major_id, ep.minor_id) AS [Column],
                CONVERT(nvarchar(max), ep.value) AS [Value]
            FROM sys.extended_properties AS ep
            WHERE ep.class = 1
              AND ep.name = N'EasyAF_LongDescription'
            """;

        /// <summary>
        /// One round-trip: every <c>EasyAF_LongDescription</c> on tables and columns.
        /// Table-level rows use a null column key.
        /// </summary>
        public static Dictionary<(string Table, string Column), string> Load(DbConnection connection)
        {
            ArgumentNullException.ThrowIfNull(connection);

            var shouldClose = connection.State != ConnectionState.Open;
            if (shouldClose)
            {
                connection.Open();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = Query;
                using var reader = command.ExecuteReader();
                var catalog = new Dictionary<(string Table, string Column), string>();
                while (reader.Read())
                {
                    var table = reader.IsDBNull(1) ? null : reader.GetString(1);
                    var column = reader.IsDBNull(2) ? null : reader.GetString(2);
                    var value = reader.IsDBNull(3) ? null : reader.GetString(3);
                    if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    catalog[Key(table, column)] = value;
                }

                return catalog;
            }
            finally
            {
                if (shouldClose && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Stamps catalog values onto the model by looking each entity/property up in memory.
        /// </summary>
        public static void Apply(EdmxModel model, IReadOnlyDictionary<(string Table, string Column), string> catalog)
        {
            ArgumentNullException.ThrowIfNull(model);
            if (catalog is null || catalog.Count == 0)
            {
                return;
            }

            catalog = catalog.ToDictionary(kv => Key(kv.Key.Table, kv.Key.Column), kv => kv.Value);
            var entitiesByName = model.EntityTypes.ToDictionary(e => e.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var entitySet in model.EntitySets)
            {
                if (!entitiesByName.TryGetValue(entitySet.EntityTypeName, out var entity))
                {
                    continue;
                }

                var table = entitySet.Name;
                if (catalog.TryGetValue(Key(table, null), out var tableDoc))
                {
                    entity.LongDescription = tableDoc;
                }

                foreach (var property in entity.Properties)
                {
                    var column = string.IsNullOrEmpty(property.StoreColumnName) ? property.Name : property.StoreColumnName;
                    if (catalog.TryGetValue(Key(table, column), out var columnDoc))
                    {
                        property.LongDescription = columnDoc;
                    }
                }
            }
        }

        /// <summary>
        /// Builds an in-memory catalog from rows (tests / callers that already have the set) and applies it.
        /// </summary>
        public static void Apply(EdmxModel model, IEnumerable<(string Table, string Column, string Value)> rows)
        {
            ArgumentNullException.ThrowIfNull(model);
            if (rows is null)
            {
                return;
            }

            var catalog = new Dictionary<(string Table, string Column), string>();
            foreach (var (table, column, value) in rows)
            {
                if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(value))
                {
                    continue;
                }

                catalog[Key(table, column)] = value;
            }

            Apply(model, catalog);
        }

        /// <summary>
        /// Loads the full catalog in one query, then stamps the model from memory.
        /// Best-effort: callers should catch failures so generate still succeeds.
        /// </summary>
        public static void ApplyFromConnection(EdmxModel model, DbConnection connection)
        {
            Apply(model, Load(connection));
        }

        /// <summary>
        /// Fills empty Summary / LongDescription from an existing EDMX. Does not overwrite catalog values already set.
        /// </summary>
        public static void PreserveExisting(EdmxModel model, string existingEdmx)
        {
            ArgumentNullException.ThrowIfNull(model);
            if (string.IsNullOrWhiteSpace(existingEdmx))
            {
                return;
            }

            var doc = XDocument.Parse(existingEdmx);
            var conceptual = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "ConceptualModels");
            if (conceptual is null)
            {
                return;
            }

            foreach (var entityElement in conceptual.Descendants().Where(x => x.Name.LocalName == "EntityType"))
            {
                var name = entityElement.Attribute("Name")?.Value;
                var entity = model.EntityTypes.FirstOrDefault(e => e.Name == name);
                if (entity is null)
                {
                    continue;
                }

                var (summary, longDescription) = ReadDocs(entityElement);
                if (string.IsNullOrEmpty(entity.Documentation) && !string.IsNullOrEmpty(summary))
                {
                    entity.Documentation = summary;
                }

                if (string.IsNullOrEmpty(entity.LongDescription) && !string.IsNullOrEmpty(longDescription))
                {
                    entity.LongDescription = longDescription;
                }

                foreach (var propertyElement in entityElement.Elements().Where(x => x.Name.LocalName == "Property"))
                {
                    var propertyName = propertyElement.Attribute("Name")?.Value;
                    var property = entity.Properties.FirstOrDefault(p => p.Name == propertyName);
                    if (property is null)
                    {
                        continue;
                    }

                    var (propSummary, propLong) = ReadDocs(propertyElement);
                    if (string.IsNullOrEmpty(property.Documentation) && !string.IsNullOrEmpty(propSummary))
                    {
                        property.Documentation = propSummary;
                    }

                    if (string.IsNullOrEmpty(property.LongDescription) && !string.IsNullOrEmpty(propLong))
                    {
                        property.LongDescription = propLong;
                    }
                }
            }
        }

        private static (string Summary, string LongDescription) ReadDocs(XElement owner)
        {
            var documentation = owner.Elements().FirstOrDefault(x => x.Name.LocalName == "Documentation");
            if (documentation is null)
            {
                return (null, null);
            }

            var summary = documentation.Elements().FirstOrDefault(x => x.Name.LocalName == "Summary")?.Value;
            var longDescription = documentation.Elements().FirstOrDefault(x => x.Name.LocalName == "LongDescription")?.Value;
            return (summary, longDescription);
        }

        private static (string Table, string Column) Key(string table, string column)
        {
            return (table.ToLowerInvariant(), column?.ToLowerInvariant());
        }

    }

}
