// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Internal;
using System.Data.Entity.Utilities;
using System.Xml;
using System.Xml.Linq;

namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    /// Utility class for reading a metadata model from .edmx.
    /// </summary>
    public static class EdmxReader
    {
        /// <summary>
        /// Reads a metadata model from .edmx.
        /// </summary>
        /// <param name="reader">XML reader for the .edmx</param>
        /// <param name="defaultSchema">Default database schema used by the model.</param>
        /// <returns>The loaded metadata model.</returns>
        public static DbCompiledModel Read(XmlReader reader, string defaultSchema)
        {
            Check.NotNull(reader, "reader");

            var document = XDocument.Load(reader);

            var mappingItemCollection = document.GetStorageMappingItemCollection(out var providerInfo);

            return new DbCompiledModel(
                CodeFirstCachedMetadataWorkspace.Create(mappingItemCollection, providerInfo),
                defaultSchema);
        }
    }
}
