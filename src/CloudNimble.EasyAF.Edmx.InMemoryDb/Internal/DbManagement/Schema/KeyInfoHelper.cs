// --------------------------------------------------------------------------------------------
// <copyright file="KeyInfoHelper.cs" company="Effort Team">
//     Copyright (C) Effort Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbManagement.Schema
{
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.Common;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbManagement.Engine.Services;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.TypeGeneration;
    using NMemory.Indexes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class KeyInfoHelper
    {
        public static IKeyInfo CreateKeyInfo(Type entityType, MemberInfo[] properties)
        {
            var primaryKeySelector = CreateSelectorExpression(entityType, properties);

            return CreateKeyInfo(primaryKeySelector);
        }

        public static IKeyInfo CreateKeyInfo(LambdaExpression selector)
        {
            var entityType = selector.Parameters[0].Type;
            var resultType = selector.Body.Type;

            var factory = ReflectionHelper
                .GetMethodInfo<IKeyInfoFactory>(f => f.Create<object, object>(null))
                .GetGenericMethodDefinition()
                .MakeGenericMethod(entityType, resultType);

            var result = factory.Invoke(
                    ExtendedKeyInfoFactory.Instance,
                    new object[] { selector }) as IKeyInfo;

            return result;
        }

        private static LambdaExpression CreateSelectorExpression(
            Type sourceType,
            MemberInfo[] selectorFields)
        {
            Expression body = null;

            var param = Expression.Parameter(sourceType);
            var memberSelectors = new Expression[selectorFields.Length];

            for (var i = 0; i < memberSelectors.Length; i++)
            {
                memberSelectors[i] = Expression.MakeMemberAccess(param, selectorFields[i]);
            }

            if (memberSelectors.Length == 1)
            {
                // Primitive key info
                body = memberSelectors[0];
            }
            else if (memberSelectors.Length < TupleTypeHelper.LargeTupleSize)
            {
                body = CreateTupleSelector(body, memberSelectors);
            }
            else
            {
                body = CreateDataRowSelector(body, memberSelectors);
            }

            return Expression.Lambda(body, param);
        }

        private static Expression CreateTupleSelector(
            Expression body,
            Expression[] memberSelectors)
        {
            var memberTypes = memberSelectors.Select(e => e.Type).ToArray();

            var tupleType = TupleTypeHelper.CreateTupleType(memberTypes);

            var helper = new TupleKeyInfoHelper(tupleType);
            body = helper.CreateKeyFactoryExpression(memberSelectors);
            return body;
        }

        private static Expression CreateDataRowSelector(
            Expression body,
            Expression[] memberSelectors)
        {
            var members = new Dictionary<string, Type>();

            var index = 0;
            foreach (var selector in memberSelectors)
            {
                members.Add(string.Format("Item{0:D2}", index), selector.Type);
                index++;
            }

            var rowType = DataRowFactory.Create(members);

            var helper = new DataRowKeyInfoHelper(rowType);
            body = helper.CreateKeyFactoryExpression(memberSelectors);

            return body;
        }
    }
}
