// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Query.InternalTrees;
using QueryRule = System.Data.Entity.Core.Query.InternalTrees.Rule;

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    // <summary>
    // Transformation Rules for ConstrainedSortOp
    // </summary>
    internal static class ConstrainedSortOpRules
    {
        #region ConstrainedSortOpOverEmptySet

        internal static readonly SimpleRule Rule_ConstrainedSortOpOverEmptySet = new(
            OpType.ConstrainedSort, ProcessConstrainedSortOpOverEmptySet);

        // <summary>
        // If the ConstrainedSortOp's input is guaranteed to produce no rows, remove the ConstrainedSortOp completly:
        // CSort(EmptySet) => EmptySet
        // </summary>
        // <param name="context"> Rule processing context </param>
        // <param name="n"> current subtree </param>
        // <param name="newNode"> transformed subtree </param>
        // <returns> transformation status </returns>
        private static bool ProcessConstrainedSortOpOverEmptySet(RuleProcessingContext context, Node n, out Node newNode)
        {
            var nodeInfo = (context).Command.GetExtendedNodeInfo(n.Child0);

            //If the input has no rows, remove the ConstraintSortOp node completly
            if (nodeInfo.MaxRows
                == RowCount.Zero)
            {
                newNode = n.Child0;
                return true;
            }

            newNode = n;
            return false;
        }

        #endregion

        #region All ConstrainedSortOp Rules

        internal static readonly QueryRule[] Rules =
            [
                Rule_ConstrainedSortOpOverEmptySet,
            ];

        #endregion
    }
}
