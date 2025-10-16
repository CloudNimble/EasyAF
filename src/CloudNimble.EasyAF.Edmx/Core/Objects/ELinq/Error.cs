// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Resources;

namespace System.Linq.Expressions.Internal
{
    internal static class Error
    {
        internal static Exception UnhandledExpressionType(ExpressionType expressionType)
        {
            return new NotSupportedException(Strings.ELinq_UnhandledExpressionType(expressionType));
        }

        internal static Exception UnhandledBindingType(MemberBindingType memberBindingType)
        {
            return new NotSupportedException(Strings.ELinq_UnhandledBindingType(memberBindingType));
        }
    }
}
