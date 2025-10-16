// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees
{
    // <summary>
    // Represents a conditional operation - and, or, in, not, is null
    // </summary>
    internal sealed class ConditionalOp : ScalarOp
    {
        #region constructors

        internal ConditionalOp(OpType optype, TypeUsage type)
            : base(optype, type)
        {
        }

        private ConditionalOp(OpType opType)
            : base(opType)
        {
        }

        #endregion

        #region public methods

        // <summary>
        // Patterns for use in transformation rules
        // </summary>
        internal static readonly ConditionalOp PatternAnd = new(OpType.And);

        internal static readonly ConditionalOp PatternOr = new(OpType.Or);
        internal static readonly ConditionalOp PatternIn = new(OpType.In);
        internal static readonly ConditionalOp PatternNot = new(OpType.Not);
        internal static readonly ConditionalOp PatternIsNull = new(OpType.IsNull);

        // <summary>
        // Visitor pattern method
        // </summary>
        // <param name="v"> The BasicOpVisitor that is visiting this Op </param>
        // <param name="n"> The Node that references this Op </param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n)
        {
            v.Visit(this, n);
        }

        // <summary>
        // Visitor pattern method for visitors with a return value
        // </summary>
        // <param name="v"> The visitor </param>
        // <param name="n"> The node in question </param>
        // <returns> An instance of TResultType </returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
        {
            return v.Visit(this, n);
        }

        #endregion
    }
}
