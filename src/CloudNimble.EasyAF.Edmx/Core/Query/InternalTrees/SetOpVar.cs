// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees
{
    // <summary>
    // A SetOp Var - used as the output var for set operations (Union, Intersect, Except)
    // </summary>
    internal sealed class SetOpVar : Var
    {
        internal SetOpVar(int id, TypeUsage type)
            : base(id, VarType.SetOp, type)
        {
        }
    }
}
