// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Entity.Utilities
{
    internal static class DbConnectionExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static string GetProviderInvariantName(this DbConnection connection)
        {
            DebugCheck.NotNull(connection);

            return DbConfiguration.DependencyResolver.GetService<IProviderInvariantName>(DbProviderServices.GetProviderFactory(connection)).Name;
        }

        public static DbProviderInfo GetProviderInfo(
            this DbConnection connection, out DbProviderManifest providerManifest)
        {
            DebugCheck.NotNull(connection);

            var providerManifestToken = DbConfiguration
                .DependencyResolver
                .GetService<IManifestTokenResolver>()
                .ResolveManifestToken(connection);

            var providerInfo = new DbProviderInfo(connection.GetProviderInvariantName(), providerManifestToken);

            providerManifest = DbProviderServices.GetProviderServices(connection).GetProviderManifest(providerManifestToken);

            return providerInfo;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static DbProviderFactory GetProviderFactory(this DbConnection connection)
        {
            DebugCheck.NotNull(connection);

            return DbConfiguration.DependencyResolver.GetService<IDbProviderFactoryResolver>().ResolveProviderFactory(connection);
        }
    }
}
