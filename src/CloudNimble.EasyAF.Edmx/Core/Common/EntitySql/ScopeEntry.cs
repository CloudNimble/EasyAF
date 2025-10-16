// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.Entity.Core.Common.EntitySql
{
    // <summary>
    // Represents an entry in the scope.
    // </summary>
    internal abstract class ScopeEntry
    {
        private readonly ScopeEntryKind _scopeEntryKind;

        internal ScopeEntry(ScopeEntryKind scopeEntryKind)
        {
            _scopeEntryKind = scopeEntryKind;
        }

        internal ScopeEntryKind EntryKind
        {
            get { return _scopeEntryKind; }
        }

        // <summary>
        // Returns CQT expression corresponding to the scope entry.
        // </summary>
        internal abstract DbExpression GetExpression(string refName, ErrorContext errCtx);
    }
}
