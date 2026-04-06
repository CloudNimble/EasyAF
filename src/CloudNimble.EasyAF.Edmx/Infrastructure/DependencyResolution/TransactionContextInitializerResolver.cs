// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    internal class TransactionContextInitializerResolver : IDbDependencyResolver
    {
        private readonly ConcurrentDictionary<Type, object> _initializers =
            new();

        public object GetService(Type type, object key)
        {
            Check.NotNull(type, "type");

            var contextType = type.TryGetElementType(typeof(IDatabaseInitializer<>));
            if (contextType is not null
                && typeof(TransactionContext).IsAssignableFrom(contextType))
            {
                return _initializers.GetOrAdd(contextType, CreateInitializerInstance);
            }

            return null;
        }

        private object CreateInitializerInstance(Type type)
        {
            var transactionContextInitializerTypeDefinition = typeof(TransactionContextInitializer<>);
            var transactionContextInitializerType = transactionContextInitializerTypeDefinition.MakeGenericType([type]);
            return Activator.CreateInstance(transactionContextInitializerType);
        }

        public Collections.Generic.IEnumerable<object> GetServices(Type type, object key)
        {
            return this.GetServiceAsServices(type, key);
        }
    }
}
