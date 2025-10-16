// --------------------------------------------------------------------------------------------
// <copyright file="DbCommandActionHelper.cs" company="Effort Team">
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

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.CommandActions
{
    using System;
    using System.Collections.Generic;
#if !EFOLD
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
#else
    using System.Data.Common.CommandTrees;
    using System.Data.Metadata.Edm;
#endif
    using System.Linq;
    using System.Linq.Expressions;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbCommandTreeTransformation;
    using NMemory.Tables;
    using NMemory.StoredProcedures;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbManagement;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.Common;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.TypeConversion;

    internal static class DbCommandActionHelper
    {
        public static FieldDescription[] GetReturningFields(
            DbExpression returning)
        {
            // Find the returning properties
            var returnExpression = returning as DbNewInstanceExpression;

            if (returnExpression == null)
            {
                throw new NotSupportedException(
                    "The type of the Returning properties is not DbNewInstanceExpression");
            }

            var result = new List<FieldDescription>();

            // Add the returning property names
            foreach (DbPropertyExpression propertyExpression in returnExpression.Arguments)
            {
                var propertyType =
                    propertyExpression.ResultType.EdmType as PrimitiveType;

                var name = propertyExpression.Property.GetColumnName();
                Type type = propertyType.ClrEquivalentType;

                result.Add(new FieldDescription(name, type));
            }

            return result.ToArray();
        }

        public static ITable GetTable(
            DbModificationCommandTree commandTree,
            DbContainer container)
        {
            return commandTree.Target.Expression is not DbScanExpression source
                ? throw new NotSupportedException(
                    "The type of the Target property is not DbScanExpression")
                : (ITable)container.GetTable(source.Target.GetFullTableName());
        }

        public static List<ITable> GetAllTables(
           DbContainer container)
        {
            return (List<ITable>)container.GetAllTables();
        }

        public static IDictionary<string, DbExpression> GetSetClauseExpressions(
            IList<DbModificationClause> clauses)
        {
            IDictionary<string, DbExpression> result = new Dictionary<string, DbExpression>();

            foreach (DbSetClause setClause in clauses.Cast<DbSetClause>())
            {
                var property = setClause.Property as DbPropertyExpression;

                if (property == null)
                {
                    throw new NotSupportedException(
                        setClause.Property.ExpressionKind.ToString() + " is not supported");
                }

                result.Add(property.Property.GetColumnName(), setClause.Value);
            }

            return result;
        }

        public static Expression GetEnumeratorExpression(
            DbExpression predicate,
            DbModificationCommandTree commandTree,
            DbContainer container,
            out ITable table)
        {
            var visitor = new TransformVisitor(container);
            visitor.TableProvider = container;

            // Get the source expression
            var source =
                visitor.Visit(commandTree.Target.Expression) as ConstantExpression;

            // This should be a constant expression
            if (source == null)
            {
                throw new InvalidOperationException();
            }

            table = source.Value as ITable;

            // Get the the type of the elements of the table
            var elementType = TypeHelper.GetElementType(source.Type);

            // Create context
            var context = Expression.Parameter(elementType, "context");
            using (visitor.CreateVariable(context, commandTree.Target.VariableName))
            {
                // Create the predicate expression
                var predicateExpression =
                    Expression.Lambda(
                        visitor.Visit(predicate),
                        context);

                // Create Where expression
                var queryMethodBuilder =
                    new LinqMethodExpressionBuilder();

                return queryMethodBuilder.Where(source, predicateExpression);
            }
        }

        public static Dictionary<string, object> CreateReturningEntity(
            ActionContext context,
            FieldDescription[] returningFields,
            object entity)
        {
            var entityReturningValues = new Dictionary<string, object>();

            for (var i = 0; i < returningFields.Length; i++)
            {
                var property = returningFields[i].Name;

                var value = entity.GetType().GetProperty(property).GetValue(entity, null);

                entityReturningValues[property] = context
                    .DbContainer
                    .TypeConverter
                    .ConvertClrObject(value, returningFields[i].Type);
            }

            return entityReturningValues;
        }

        public static IDictionary<string, object> FormatParameters(
            IList<CommandActionParameter> source,
            IList<ParameterDescription> description,
            ITypeConverter converter)
        {
            // Determine parameter values
            var result = new Dictionary<string, object>();

            foreach (var param in source)
            {
                var name = param.Name;
                var value = param.Value;

                // Find the description of the parameter
                var expectedParam =
                    description.FirstOrDefault(p => p.Name == name);

                // Custom conversion
                value = converter.ConvertClrObject(value, expectedParam.Type);

                result.Add(name, value);
            }

            return result;
        }
    }
}
