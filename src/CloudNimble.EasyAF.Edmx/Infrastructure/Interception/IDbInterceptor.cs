// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Data.Entity.Infrastructure.Interception
{
    /// <summary>
    /// This is the base interface for all interfaces that provide interception points for various
    /// different types and operations. For example, see <see cref="IDbCommandInterceptor" />.
    /// Interceptors are registered on the <see cref="DbInterception" /> class.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IDbInterceptor
    {
    }
}
