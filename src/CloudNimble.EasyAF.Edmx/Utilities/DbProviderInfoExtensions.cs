// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Infrastructure;

namespace System.Data.Entity.Utilities
{
    internal static class DbProviderInfoExtensions
    {
        public static bool IsSqlCe(this DbProviderInfo providerInfo)
        {
            DebugCheck.NotNull(providerInfo);

            return !string.IsNullOrWhiteSpace(providerInfo.ProviderInvariantName) &&
                   providerInfo.ProviderInvariantName.StartsWith(
                       "System.Data.SqlServerCe", StringComparison.OrdinalIgnoreCase);
        }
    }
}
