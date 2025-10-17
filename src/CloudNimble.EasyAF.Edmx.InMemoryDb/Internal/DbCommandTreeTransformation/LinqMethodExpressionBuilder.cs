// --------------------------------------------------------------------------------------------
// <copyright file="LinqMethodExpressionBuilder.cs" company="Effort Team">
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

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbCommandTreeTransformation
{
    using System;
#if !EFOLD
#else
    using System.Data.Common.CommandTrees;
#endif
    using System.Linq.Expressions;
    using System.Reflection;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.Common;

    internal class LinqMethodExpressionBuilder
    {
        private LinqMethodProvider queryMethods;

        public LinqMethodExpressionBuilder()
        {
            queryMethods = LinqMethodProvider.Instance;
        }

        public Expression Select(Expression source, LambdaExpression selector)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);
            var selectorType = selector.Body.Type;

            var genericMethod = queryMethods.Select;
            var method = genericMethod.MakeGenericMethod(sourceType, selectorType);

            return Expression.Call(method, source, Expression.Quote(selector));
        }

        public Expression SelectMany(Expression source, LambdaExpression selector)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);
            var selectorType = selector.Body.Type;

            var genericMethod = queryMethods.SelectMany;
            var method = genericMethod.MakeGenericMethod(sourceType, selectorType);

            return Expression.Call(method, source, Expression.Quote(selector));
        }

        public Expression SelectMany(
            Expression first,
            LambdaExpression collectionSelector,
            LambdaExpression resultSelector)
        {
            var firstType = TypeHelper.GetElementType(first.Type);
            var collectionType = TypeHelper.GetElementType(collectionSelector.Body.Type);

            var resultType = resultSelector.Body.Type;

            var genericMethod = queryMethods.SelectManyWithResultSelector;

            var method =
                genericMethod.MakeGenericMethod(firstType, collectionType, resultType);

            return Expression.Call(
                method,
                first,
                Expression.Quote(collectionSelector),
                Expression.Quote(resultSelector));
        }

        public Expression Where(Expression source, LambdaExpression predicate)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);

            var genericMethod = queryMethods.Where;
            var method = genericMethod.MakeGenericMethod(sourceType);

            return Expression.Call(method, source, Expression.Quote(predicate));
        }

        public Expression Take(Expression source, Expression count)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);

            var genericMethod = queryMethods.Take;
            var method = genericMethod.MakeGenericMethod(sourceType);

            return Expression.Call(method, source, count);
        }

        public Expression Skip(Expression source, Expression count)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);

            var genericMethod = queryMethods.Skip;
            var method = genericMethod.MakeGenericMethod(sourceType);

            return Expression.Call(method, source, count);
        }

        public Expression OrderBy(Expression source, LambdaExpression selector)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);
            var selectorType = selector.Body.Type;

            var genericMethod = queryMethods.OrderBy;
            var method = genericMethod.MakeGenericMethod(sourceType, selectorType);

            return Expression.Call(method, source, Expression.Quote(selector));
        }

        public Expression OrderByDescending(Expression source, LambdaExpression selector)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);
            var selectorType = selector.Body.Type;

            var genericMethod = queryMethods.OrderByDescending;
            var method = genericMethod.MakeGenericMethod(sourceType, selectorType);

            return Expression.Call(method, source, Expression.Quote(selector));
        }

        public Expression ThenBy(Expression source, LambdaExpression selector)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);
            var selectorType = selector.Body.Type;

            var genericMethod = queryMethods.ThenBy;
            var method = genericMethod.MakeGenericMethod(sourceType, selectorType);

            return Expression.Call(method, source, Expression.Quote(selector));
        }

        public Expression ThenByDescending(Expression source, LambdaExpression selector)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);
            var selectorType = selector.Body.Type;

            var genericMethod = queryMethods.ThenByDescending;
            var method = genericMethod.MakeGenericMethod(sourceType, selectorType);

            return Expression.Call(method, source, Expression.Quote(selector));
        }

        public Expression GroupBy(Expression source, LambdaExpression selector)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);
            var selectorType = selector.Body.Type;

            var genericMethod = queryMethods.GroupBy;
            var method = genericMethod.MakeGenericMethod(sourceType, selectorType);

            return Expression.Call(method, source, Expression.Quote(selector));
        }

        public Expression Distinct(Expression source)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);

            var genericMethod = queryMethods.Distinct;
            var method = genericMethod.MakeGenericMethod(sourceType);

            return Expression.Call(method, source);
        }

        public Expression FirstOrDefault(Expression source)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);

            var genericMethod = queryMethods.FirstOrDefault;
            var method = genericMethod.MakeGenericMethod(sourceType);

            return Expression.Call(method, source);
        }

        public Expression First(Expression source)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);

            var genericMethod = queryMethods.First;
            var method = genericMethod.MakeGenericMethod(sourceType);

            return Expression.Call(method, source);
        }

        public Expression Any(Expression source)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);

            var genericMethod = queryMethods.Any;
            var method = genericMethod.MakeGenericMethod(sourceType);

            return Expression.Call(method, source);
        }

        public Expression DefaultIfEmpty(Expression source)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);
            var genericMethod = queryMethods.DefaultIfEmpty;

            var method = genericMethod.MakeGenericMethod(sourceType);

            return Expression.Call(method, source);
        }

        public Expression AsQueryable(Expression source)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);
            var genericMethod = queryMethods.AsQueryable;

            var method = genericMethod.MakeGenericMethod(sourceType);

            return Expression.Call(method, source);
        }

        public Expression Except(Expression first, Expression second)
        {
            var firstType = TypeHelper.GetElementType(first.Type);

            var genericMethod = queryMethods.Except;
            var method = genericMethod.MakeGenericMethod(firstType);

            return Expression.Call(method, first, second);
        }

        public Expression Intersect(Expression first, Expression second)
        {
            var firstType = TypeHelper.GetElementType(first.Type);

            var genericMethod = queryMethods.Intersect;
            var method = genericMethod.MakeGenericMethod(firstType);

            return Expression.Call(method, first, second);
        }

        public Expression Union(Expression first, Expression second)
        {
            var firstType = TypeHelper.GetElementType(first.Type);

            var genericMethod = queryMethods.Union;
            var method = genericMethod.MakeGenericMethod(firstType);

            return Expression.Call(method, first, second);
        }

        public Expression Concat(Expression first, Expression second)
        {
            var firstType = TypeHelper.GetElementType(first.Type);

            var genericMethod = queryMethods.Concat;
            var method = genericMethod.MakeGenericMethod(firstType);

            return Expression.Call(method, first, second);
        }

        public Expression Count(Expression source)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);

            var genericMethod = queryMethods.Count;
            var method = genericMethod.MakeGenericMethod(sourceType);

            return Expression.Call(method, source);
        }

        public Expression LongCount(Expression source)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);

            var genericMethod = queryMethods.Count;
            var method = genericMethod.MakeGenericMethod(sourceType);

            return Expression.Call(method, source);
        }

        public Expression Max(Expression source, LambdaExpression selector)
        {
            var group = queryMethods.Max;
            Func<MethodInfo> generic = () => queryMethods.MaxGeneric;

            var method = GetAggregationMethod(source, selector, group, generic);

            return Expression.Call(method, source, selector);
        }

        public Expression Min(Expression source, LambdaExpression selector)
        {
            var group = queryMethods.Min;
            Func<MethodInfo> generic = () => queryMethods.MinGeneric;

            var method = GetAggregationMethod(source, selector, group, generic);

            return Expression.Call(method, source, selector);
        }

        public Expression Average(Expression source, LambdaExpression selector)
        {
            var group = queryMethods.Average;
            Func<MethodInfo> generic = () => queryMethods.AverageGeneric;

            var method = GetAggregationMethod(source, selector, group, generic);

            return Expression.Call(method, source, selector);
        }

        public Expression Sum(Expression source, LambdaExpression selector)
        {
            var group = queryMethods.Sum;

            var method = GetAggregationMethod(source, selector, group, null);

            return Expression.Call(method, source, selector);
        }

        private static MethodInfo GetAggregationMethod(
            Expression source,
            LambdaExpression selector,
            MethodInfoGroup group,
            Func<MethodInfo> generic)
        {
            var sourceType = TypeHelper.GetElementType(source.Type);
            var selectorType = selector.Body.Type;

            var genericMethod = group[selectorType];
            MethodInfo method = null;

            if (genericMethod is null)
            {
                genericMethod = generic.Invoke();
                method = genericMethod.MakeGenericMethod(sourceType, selectorType);
            }
            else
            {
                method = genericMethod.MakeGenericMethod(sourceType);
            }

            return method;
        }
    }
}
