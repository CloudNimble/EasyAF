// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using WrapperBoolExpr = System.Data.Entity.Core.Common.Utils.Boolean.BoolExpr<System.Data.Entity.Core.Mapping.ViewGeneration.Structures.LeftCellWrapper>;
using WrapperTreeExpr = System.Data.Entity.Core.Common.Utils.Boolean.TreeExpr<System.Data.Entity.Core.Mapping.ViewGeneration.Structures.LeftCellWrapper>;
using WrapperAndExpr = System.Data.Entity.Core.Common.Utils.Boolean.AndExpr<System.Data.Entity.Core.Mapping.ViewGeneration.Structures.LeftCellWrapper>;
using WrapperOrExpr = System.Data.Entity.Core.Common.Utils.Boolean.OrExpr<System.Data.Entity.Core.Mapping.ViewGeneration.Structures.LeftCellWrapper>;
using WrapperNotExpr = System.Data.Entity.Core.Common.Utils.Boolean.NotExpr<System.Data.Entity.Core.Mapping.ViewGeneration.Structures.LeftCellWrapper>;
using WrapperTermExpr = System.Data.Entity.Core.Common.Utils.Boolean.TermExpr<System.Data.Entity.Core.Mapping.ViewGeneration.Structures.LeftCellWrapper>;
using WrapperTrueExpr = System.Data.Entity.Core.Common.Utils.Boolean.TrueExpr<System.Data.Entity.Core.Mapping.ViewGeneration.Structures.LeftCellWrapper>;
using WrapperFalseExpr = System.Data.Entity.Core.Common.Utils.Boolean.FalseExpr<System.Data.Entity.Core.Mapping.ViewGeneration.Structures.LeftCellWrapper>;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation
{
    // A superclass for constraint errors. It also contains useful constraint
    // checking methods
    internal abstract class ConstraintBase : InternalBase
    {
        // effects: Returns an error log record with this constraint's information
        internal abstract ErrorLog.Record GetErrorRecord();
    }
}
