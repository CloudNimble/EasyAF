// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Infrastructure;

namespace System.Data.Entity.Internal
{
    internal interface IDbEnumerator<out T> : IEnumerator<T>
#if !NET40
                                              , IDbAsyncEnumerator<T>
#endif
    {
        new T Current { get; }
    }
}
