// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.EntityClient;

namespace System.Data.Entity.Infrastructure.Interception
{
    internal interface ICancelableEntityConnectionInterceptor : IDbInterceptor
    {
        bool ConnectionOpening(EntityConnection connection, DbInterceptionContext interceptionContext);
    }
}
