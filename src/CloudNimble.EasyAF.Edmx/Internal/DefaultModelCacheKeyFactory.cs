// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Infrastructure;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Internal
{
    internal sealed class DefaultModelCacheKeyFactory
    {
        public IDbModelCacheKey Create(DbContext context)
        {
            Check.NotNull(context, "context");

            string customKey = null;

            var modelCacheKeyProvider = context as IDbModelCacheKeyProvider;

            if (modelCacheKeyProvider is not null)
            {
                customKey = modelCacheKeyProvider.CacheKey;
            }

            return new DefaultModelCacheKey(
                context.GetType(),
                context.InternalContext.ProviderName,
                context.InternalContext.ProviderFactory.GetType(),
                customKey);
        }
    }
}
