// --------------------------------------------------------------------------------------------
// <copyright file="DataRowKeyInfoHelper.cs" company="Effort Team">
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
// -------------------------------------------------------------------------------------------

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbManagement.Engine.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.Common;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.TypeGeneration;
    using NMemory.Indexes;

    internal class DataRowKeyInfoHelper : IKeyInfoHelper
    {
        private readonly Type type;
        private readonly PropertyInfo[] properties;
        private readonly bool isLarge;
        private readonly ConstructorInfo ctor;

        public DataRowKeyInfoHelper(Type type)
        {
            this.type = type;

            properties = type
                .GetProperties()
                .Select(p => new
                {
                    Property = p,
                    Attribute = p.GetCustomAttributes(false)
                        .OfType<DataRowPropertyAttribute>()
                        .SingleOrDefault()
                })
                .Where(x => x.Attribute != null)
                .OrderBy(x => x.Attribute.Index)
                .Select(x => x.Property)
                .ToArray();

            ctor = type
                .GetConstructors()
                .Single();

            isLarge = this.type
                .GetCustomAttributes(false)
                .OfType<LargeDataRowAttribute>()
                .Any();
        }

        public Expression CreateKeyFactoryExpression(params Expression[] arguments)
        {
            var args = new Expression[properties.Length];

            if (args.Length != arguments.Length)
            {
                throw new ArgumentException("", "arguments");
            }

            for (var i = 0; i < args.Length; i++)
            {
                args[i] = arguments[i];

                var propertyType = properties[i].PropertyType;

                if (propertyType != args[i].Type)
                {
                    args[i] = Expression.Convert(args[i], propertyType);
                }

                if (isLarge)
                {
                    args[i] = Expression.Convert(args[i], typeof(object));
                }
            }

            if (isLarge)
            {
                Expression array = Expression.NewArrayInit(typeof(object), args);

                return Expression.New(ctor, array);
            }
            else
            {
                return Expression.New(ctor, args);
            }
        }

        public Expression CreateKeyMemberSelectorExpression(Expression source, int index)
        {
            return Expression.MakeMemberAccess(source, properties[index]);
        }

        public int GetMemberCount()
        {
            return properties.Length;
        }

        public bool TryParseKeySelectorExpression(
            Expression keySelector,
            bool strict,
            out MemberInfo[] result)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            if (keySelector.Type != type)
            {
                result = null;
                return false;
            }

            var resultCreator = keySelector as NewExpression;

            if (resultCreator == null)
            {
                result = null;
                return false;
            }

            var args = resultCreator.Arguments.ToArray();

            if (isLarge)
            {
                if (resultCreator.Arguments.Count != 1)
                {
                    result = null;
                    return false;
                }

                var array = resultCreator.Arguments[0] as NewArrayExpression;

                if (array == null)
                {
                    result = null;
                    return false;
                }

                args = array.Expressions.ToArray();
            }

            var resultList = new List<MemberInfo>();

            foreach (var arg in args)
            {
                var expr = arg;

                if (!strict || isLarge)
                {
                    expr = ExpressionHelper.SkipConversionNodes(expr);
                }

                var member = expr as MemberExpression;

                if (member == null)
                {
                    result = null;
                    return false;
                }

                resultList.Add(member.Member);
            }

            result = resultList.ToArray();
            return true;
        }
    }
}
