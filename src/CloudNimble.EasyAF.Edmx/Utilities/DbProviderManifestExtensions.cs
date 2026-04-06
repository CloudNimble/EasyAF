// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Utilities
{
    internal static class DbProviderManifestExtensions
    {
        public static PrimitiveType GetStoreTypeFromName(this DbProviderManifest providerManifest, string name)
        {
            DebugCheck.NotNull(providerManifest);
            DebugCheck.NotEmpty(name);

            var primitiveType = providerManifest.GetStoreTypes()
                .SingleOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

            if (primitiveType is null)
            {
	            throw Error.StoreTypeNotFound(name, providerManifest.NamespaceName);
            }
            return primitiveType;
        }
    }
}
