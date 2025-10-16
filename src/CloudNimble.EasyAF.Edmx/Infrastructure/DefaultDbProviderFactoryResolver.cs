// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


using System.Data.Common;
using System.Data.Entity.Utilities;

#if !NET40

namespace System.Data.Entity.Infrastructure
{
    internal class DefaultDbProviderFactoryResolver : IDbProviderFactoryResolver
    {
        public DbProviderFactory ResolveProviderFactory(DbConnection connection)
        {
            Check.NotNull(connection, "connection");

#if NETSTANDARD
            return DbProviderFactoriesCore.GetFactory(connection);
#else
            return DbProviderFactories.GetFactory(connection);
#endif
        }
    }
}

#endif
