// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    internal class DefaultInvariantNameResolver : IDbDependencyResolver
    {
        public virtual object GetService(Type type, object key)
        {
            if (type == typeof(IProviderInvariantName))
            {
                var factory = key as DbProviderFactory;

                if (factory is null)
                {
                    throw new ArgumentException(
                        Strings.DbDependencyResolver_InvalidKey(typeof(DbProviderFactory).Name, typeof(IProviderInvariantName)));
                }

                return new ProviderInvariantName(factory.GetProviderInvariantName());
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            return this.GetServiceAsServices(type, key);
        }
    }
}
