// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.Interception
{
    internal class DbConfigurationDispatcher
    {
        private readonly InternalDispatcher<IDbConfigurationInterceptor> _internalDispatcher
            = new();

        public InternalDispatcher<IDbConfigurationInterceptor> InternalDispatcher
        {
            get { return _internalDispatcher; }
        }

        public virtual void Loaded(DbConfigurationLoadedEventArgs loadedEventArgs, DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(loadedEventArgs);
            DebugCheck.NotNull(interceptionContext);

            var clonedInterceptionContext = new DbConfigurationInterceptionContext(interceptionContext);

            _internalDispatcher.Dispatch(i => i.Loaded(loadedEventArgs, clonedInterceptionContext));
        }
    }
}
