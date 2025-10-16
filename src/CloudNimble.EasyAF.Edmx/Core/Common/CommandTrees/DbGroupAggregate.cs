// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Common.CommandTrees
{
    /// <summary>Represents a collection of elements that compose a group.  </summary>
    public sealed class DbGroupAggregate : DbAggregate
    {
        internal DbGroupAggregate(TypeUsage resultType, DbExpressionList arguments)
            : base(resultType, arguments)
        {
        }
    }
}
