// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    internal class InvariantNameResolver : IDbDependencyResolver
    {
        private readonly IProviderInvariantName _invariantName;
        private readonly Type _providerFactoryType;

        public InvariantNameResolver(DbProviderFactory providerFactory, string invariantName)
        {
            DebugCheck.NotNull(providerFactory);
            DebugCheck.NotEmpty(invariantName);

            _invariantName = new ProviderInvariantName(invariantName);
            _providerFactoryType = providerFactory.GetType();
        }

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

                if (key.GetType() == _providerFactoryType)
                {
                    return _invariantName;
                }
            }

            return null;
        }

        // <summary>
        // Used for testing.
        // </summary>
        public override bool Equals(object obj)
        {
            var other = obj as InvariantNameResolver;
            if (other is null)
            {
                return false;
            }

            return _providerFactoryType == other._providerFactoryType
                   && _invariantName.Name == other._invariantName.Name;
        }

        // <summary>
        // Because Equals is overridden; not currently used.
        // </summary>
        public override int GetHashCode()
        {
            return _invariantName.Name.GetHashCode();
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            return this.GetServiceAsServices(type, key);
        }
    }
}
