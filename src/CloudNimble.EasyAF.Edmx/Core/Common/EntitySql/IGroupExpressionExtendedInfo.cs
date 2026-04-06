// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.Entity.Core.Common.EntitySql
{
    internal interface IGroupExpressionExtendedInfo
    {
        // <summary>
        // Returns <see cref="DbGroupExpressionBinding.GroupVariable" /> based expression during the
        // <see
        //     cref="DbGroupByExpression" />
        // construction process, otherwise null.
        // </summary>
        DbExpression GroupVarBasedExpression { get; }

        // <summary>
        // Returns <see cref="DbGroupAggregate" /> based expression during the <see cref="DbGroupByExpression" /> construction process, otherwise null.
        // </summary>
        DbExpression GroupAggBasedExpression { get; }
    }
}
