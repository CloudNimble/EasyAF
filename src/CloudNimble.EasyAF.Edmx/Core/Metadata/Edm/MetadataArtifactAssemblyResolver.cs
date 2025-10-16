// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace System.Data.Entity.Core.Metadata.Edm
{
    internal abstract class MetadataArtifactAssemblyResolver
    {
        internal abstract bool TryResolveAssemblyReference(AssemblyName refernceName, out Assembly assembly);
        internal abstract IEnumerable<Assembly> GetWildcardAssemblies();
    }
}
