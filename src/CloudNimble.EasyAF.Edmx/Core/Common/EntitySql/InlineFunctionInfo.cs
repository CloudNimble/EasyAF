// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.Entity.Core.Common.EntitySql
{
    internal abstract class InlineFunctionInfo
    {
        internal InlineFunctionInfo(AST.FunctionDefinition functionDef, List<DbVariableReferenceExpression> parameters)
        {
            FunctionDefAst = functionDef;
            Parameters = parameters;
        }

        internal readonly AST.FunctionDefinition FunctionDefAst;
        internal readonly List<DbVariableReferenceExpression> Parameters;

        internal abstract DbLambda GetLambda(SemanticResolver sr);
    }
}
