// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm
{
    internal static class MetadataItemHelper
    {
        internal const string SchemaErrorsMetadataPropertyName = "EdmSchemaErrors";
        internal const string SchemaInvalidMetadataPropertyName = "EdmSchemaInvalid";

        public static bool IsInvalid(MetadataItem instance)
        {
            Debug.Assert(instance is not null, "instance is not null");

            if (!instance.MetadataProperties.TryGetValue(SchemaInvalidMetadataPropertyName, false, out var property)
                || property is null)
            {
                return false;
            }

            return (bool)property.Value;
        }

        public static bool HasSchemaErrors(MetadataItem instance)
        {
            Debug.Assert(instance is not null, "instance is not null");

            return instance.MetadataProperties.Contains(SchemaErrorsMetadataPropertyName);
        }

        public static IEnumerable<EdmSchemaError> GetSchemaErrors(MetadataItem instance)
        {
            Debug.Assert(instance is not null, "instance is not null");

            if (!instance.MetadataProperties.TryGetValue(SchemaErrorsMetadataPropertyName, false, out var property)
                || property is null)
            {
                return Enumerable.Empty<EdmSchemaError>();
            }

            return (IEnumerable<EdmSchemaError>)property.Value;
        }
    }
}
