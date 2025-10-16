// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.Interception
{
    internal class DbCommandTreeDispatcher
    {
        private readonly InternalDispatcher<IDbCommandTreeInterceptor> _internalDispatcher
            = new();

        public InternalDispatcher<IDbCommandTreeInterceptor> InternalDispatcher
        {
            get { return _internalDispatcher; }
        }

        public virtual DbCommandTree Created(DbCommandTree commandTree, DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(commandTree);
            DebugCheck.NotNull(interceptionContext);

            return _internalDispatcher.Dispatch(
                commandTree,
                new DbCommandTreeInterceptionContext(interceptionContext),
                (i, c) => i.TreeCreated(c));
        }
    }
}
