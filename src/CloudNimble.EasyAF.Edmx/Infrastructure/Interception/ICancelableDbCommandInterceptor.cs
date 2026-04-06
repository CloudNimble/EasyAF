// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;

namespace System.Data.Entity.Infrastructure.Interception
{
    internal interface ICancelableDbCommandInterceptor : IDbInterceptor
    {
        bool CommandExecuting(DbCommand command, DbInterceptionContext interceptionContext);
    }
}
