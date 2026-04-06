// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    internal class DatabaseInitializerResolver : IDbDependencyResolver
    {
        private readonly ConcurrentDictionary<Type, object> _initializers =
            new();

        public virtual object GetService(Type type, object key)
        {
            var contextType = type.TryGetElementType(typeof(IDatabaseInitializer<>));
            if (contextType is not null)
            {
                if (_initializers.TryGetValue(contextType, out var initializer))
                {
                    return initializer;
                }
            }

            return null;
        }

        public virtual void SetInitializer(Type contextType, object initializer)
        {
            DebugCheck.NotNull(contextType);
            DebugCheck.NotNull(initializer);

            _initializers.AddOrUpdate(contextType, initializer, (c, i) => initializer);
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            return this.GetServiceAsServices(type, key);
        }
    }
}
