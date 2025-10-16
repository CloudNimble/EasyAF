// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm
{
    internal abstract class AssemblyCacheEntry
    {
        internal abstract IList<EdmType> TypesInAssembly { get; }
        internal abstract IList<Assembly> ClosureAssemblies { get; }

        internal bool TryGetEdmType(string typeName, out EdmType edmType)
        {
            edmType = null;
            foreach (var loadedEdmType in TypesInAssembly)
            {
                if (loadedEdmType.Identity == typeName)
                {
                    edmType = loadedEdmType;
                    break;
                }
            }
            return (edmType is not null);
        }

        internal bool ContainsType(string typeName)
        {
            return TryGetEdmType(typeName, out var edmType);
        }
    }
}
