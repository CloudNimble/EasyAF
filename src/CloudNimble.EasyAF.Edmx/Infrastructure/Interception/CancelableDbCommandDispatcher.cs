// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.Interception
{
    internal class CancelableDbCommandDispatcher
    {
        private readonly InternalDispatcher<ICancelableDbCommandInterceptor> _internalDispatcher
            = new();

        public InternalDispatcher<ICancelableDbCommandInterceptor> InternalDispatcher
        {
            get { return _internalDispatcher; }
        }

        public virtual bool Executing(DbCommand command, DbInterceptionContext interceptionContext)
        {
            DebugCheck.NotNull(command);
            DebugCheck.NotNull(interceptionContext);

            return _internalDispatcher.Dispatch(true, (b, i) => i.CommandExecuting(command, interceptionContext) && b);
        }
    }
}
