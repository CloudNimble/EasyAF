// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees
{
    // <summary>
    // A computed expression. Defined by a VarDefOp
    // </summary>
    internal sealed class ComputedVar : Var
    {
        internal ComputedVar(int id, TypeUsage type)
            : base(id, VarType.Computed, type)
        {
        }
    }
}
