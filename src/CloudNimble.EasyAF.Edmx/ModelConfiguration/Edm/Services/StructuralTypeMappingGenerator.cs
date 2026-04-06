// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    internal abstract class StructuralTypeMappingGenerator
    {
        protected readonly DbProviderManifest _providerManifest;

        protected StructuralTypeMappingGenerator(DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(providerManifest);

            _providerManifest = providerManifest;
        }

        protected EdmProperty MapTableColumn(
            EdmProperty property,
            string columnName,
            bool isInstancePropertyOnDerivedType)
        {
            DebugCheck.NotNull(property);
            DebugCheck.NotEmpty(columnName);

            var underlyingTypeUsage
                = TypeUsage.Create(property.UnderlyingPrimitiveType, property.TypeUsage.Facets);

            var storeTypeUsage = _providerManifest.GetStoreType(underlyingTypeUsage);

            var tableColumnMetadata
                = new EdmProperty(columnName, storeTypeUsage)
                    {
                        Nullable = isInstancePropertyOnDerivedType || property.Nullable
                    };

            if (tableColumnMetadata.IsPrimaryKeyColumn)
            {
                tableColumnMetadata.Nullable = false;
            }

            var storeGeneratedPattern = property.GetStoreGeneratedPattern();

            if (storeGeneratedPattern is not null)
            {
                tableColumnMetadata.StoreGeneratedPattern = storeGeneratedPattern.Value;
            }

            MapPrimitivePropertyFacets(property, tableColumnMetadata, storeTypeUsage);

            return tableColumnMetadata;
        }

        internal static void MapPrimitivePropertyFacets(
            EdmProperty property, EdmProperty column, TypeUsage typeUsage)
        {
            DebugCheck.NotNull(property);
            DebugCheck.NotNull(column);
            DebugCheck.NotNull(typeUsage);

            if (IsValidFacet(typeUsage, XmlConstants.FixedLengthElement)
                && property.IsFixedLength is not null)
            {
                column.IsFixedLength = property.IsFixedLength;
            }

            if (IsValidFacet(typeUsage, XmlConstants.MaxLengthElement))
            {
                column.IsMaxLength = property.IsMaxLength;

                if (!column.IsMaxLength || property.MaxLength is not null)
                {
                    column.MaxLength = property.MaxLength;
                }
            }

            if (IsValidFacet(typeUsage, XmlConstants.UnicodeElement)
                && property.IsUnicode is not null)
            {
                column.IsUnicode = property.IsUnicode;
            }

            if (IsValidFacet(typeUsage, XmlConstants.PrecisionElement)
                && property.Precision is not null)
            {
                column.Precision = property.Precision;
            }

            if (IsValidFacet(typeUsage, XmlConstants.ScaleElement)
                && property.Scale is not null)
            {
                column.Scale = property.Scale;
            }
        }

        private static bool IsValidFacet(TypeUsage typeUsage, string name)
        {
            DebugCheck.NotNull(typeUsage);
            DebugCheck.NotEmpty(name);


            return typeUsage.Facets.TryGetValue(name, false, out var facet)
                   && !facet.Description.IsConstant;
        }

        protected static EntityTypeMapping GetEntityTypeMappingInHierarchy(
            DbDatabaseMapping databaseMapping, EntityType entityType)
        {
            DebugCheck.NotNull(databaseMapping);
            DebugCheck.NotNull(entityType);

            var entityTypeMapping = databaseMapping.GetEntityTypeMapping(entityType);

            if (entityTypeMapping is null)
            {
                var entitySetMapping =
                    databaseMapping.GetEntitySetMapping(databaseMapping.Model.GetEntitySet(entityType));

                if (entitySetMapping is not null)
                {
                    entityTypeMapping = entitySetMapping
                        .EntityTypeMappings
                        .First(
                            etm => entityType.DeclaredProperties.All(
                                dp => etm.MappingFragments.First()
                                         .ColumnMappings.Select(pm => pm.PropertyPath.First()).Contains(dp)));
                }
            }

            Debug.Assert(entityTypeMapping is not null);

            return entityTypeMapping;
        }
    }
}
