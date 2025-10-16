// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    // <summary>
    // Information about a Var and its replacement
    // </summary>
    internal abstract class VarInfo
    {
        // <summary>
        // Gets <see cref="VarInfoKind" /> for this <see cref="VarInfo" />.
        // </summary>
        internal abstract VarInfoKind Kind { get; }

        // <summary>
        // Get the list of new Vars introduced by this VarInfo
        // </summary>
        internal virtual List<Var> NewVars
        {
            get { return null; }
        }
    }
}
