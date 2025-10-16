// --------------------------------------------------------------------------------------------
// <copyright file="DbTableInfo.cs" company="Effort Team">
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
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Exceptions;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.Common;
    using NMemory.Indexes;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq.Expressions;
    using System.Reflection;


    /// <summary>
    /// 
    /// </summary>
    public class DbTableInfo
    {
        private FastLazy<Func<object[], object>> initializer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entitySet"></param>
        /// <param name="tableName"></param>
        /// <param name="entityType"></param>
        /// <param name="identityField"></param>
        /// <param name="properties"></param>
        /// <param name="primaryKeyInfo"></param>
        /// <param name="uniqueKeys"></param>
        /// <param name="foreignKeys"></param>
        /// <param name="constraintFactories"></param>
        public DbTableInfo(
            EntitySet entitySet,
            TableName tableName,
            Type entityType,
            MemberInfo identityField,
            PropertyInfo[] properties,
            IKeyInfo primaryKeyInfo,
            IKeyInfo[] uniqueKeys,
            IKeyInfo[] foreignKeys,
            object[] constraintFactories)
        {
            EntitySet = entitySet;
            TableName = tableName;
            EntityType = entityType;
            IdentityField = identityField;
            Properties = properties;
            ConstraintFactories = constraintFactories;
            PrimaryKeyInfo = primaryKeyInfo;
            UniqueKeys = uniqueKeys;
            ForeignKeys = foreignKeys;

            initializer = new FastLazy<Func<object[], object>>(CreateEntityInitializer);
        }

        /// <summary>
        /// 
        /// </summary>
        public EntitySet EntitySet { get; set; }

        /// <summary>
        ///
        /// </summary>
        public TableName TableName { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MemberInfo IdentityField { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public PropertyInfo[] Properties { get; private set; }

        /// <summary>
        /// NMemory.Constraints.IConstrain{TEntity} array
        /// </summary>
        public object[] ConstraintFactories { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IKeyInfo PrimaryKeyInfo { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IKeyInfo[] ForeignKeys { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IKeyInfo[] UniqueKeys { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Func<object[], object> EntityInitializer
        {
            get { return initializer.Value; }
        }

        private Func<object[], object> CreateEntityInitializer()
        {
            var parameter = Expression.Parameter(typeof(object[]));

            var result = Expression.Variable(EntityType);
            var blockElements = new List<Expression>();

            blockElements.Add(Expression.Assign(result, Expression.New(EntityType)));

            Expression<Action<Exception, PropertyInfo, object>> handleException =
                (exception, property, value) =>
                    HandleConvertException(exception, property, value);

            var caught = Expression.Parameter(typeof(Exception));
            var valueExpression = Expression.Variable(typeof(object), "value");

            for (var i = 0; i < Properties.Length; i++)
            {
                blockElements.Add(
                    Expression.TryCatch(
                        Expression.Block(typeof(void),
                            Expression.Assign(
                                valueExpression,
                                Expression.ArrayIndex(parameter, Expression.Constant(i))),
                            Expression.Assign(
                                Expression.Property(
                                    result,
                                    Properties[i]),
                                Expression.Convert(
                                    valueExpression,
                                    Properties[i].PropertyType))),
                        Expression.Catch(
                            caught,
                            Expression.Invoke(
                                handleException,
                                caught,
                                Expression.Constant(Properties[i]), valueExpression))));
            }

            blockElements.Add(result);

            Expression body =
                Expression.Block(
                    EntityType,
                    new ParameterExpression[] { result, valueExpression },
                    blockElements.ToArray());

            return Expression.Lambda<Func<object[], object>>(body, parameter).Compile();
        }

        private void HandleConvertException(Exception exception, PropertyInfo property, object value)
        {
            var message =
                string.Format(
                    ExceptionMessages.EntityPropertyAssignFailed,
                    value ?? "[null]",
                    property.Name,
                    property.PropertyType,
                    TableName);

            throw new EffortException(message, exception);
        }
    }
}
