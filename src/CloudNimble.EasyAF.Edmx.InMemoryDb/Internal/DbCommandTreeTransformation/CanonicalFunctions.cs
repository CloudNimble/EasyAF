// --------------------------------------------------------------------------------------------
// <copyright file="CanonicalFunctions.cs" company="Effort Team">
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
    using System.Collections.Generic;
#if !EFOLD
    using System.Data.Entity.Core.Metadata.Edm;
#else
    using System.Data.Metadata.Edm;
#endif
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbCommandTreeTransformation.Functions;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbManagement;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.TypeConversion;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.Common;

    internal class CanonicalFunctionMapper
    {
        private readonly Dictionary<string, Func<EdmFunction, Expression[], Expression>> mappings;
        private EdmTypeConverter converter;

        public CanonicalFunctionMapper(ITypeConverter converter, DbContainer container)
        {
            this.converter = new EdmTypeConverter(converter);
            mappings = new Dictionary<string, Func<EdmFunction, Expression[], Expression>>();

            AddStringMappings(container);
            AddDateTimeMappings();
            AddMathMappings();
            AddBitwiseMappings();
            AddMiscMappings();
        }

        private void AddMiscMappings()
        {
            mappings["Edm.NewGuid"] = (f, args) =>
                Expression.Call(null, ReflectionHelper.GetMethodInfo(() => Guid.NewGuid()));
        }

        private void AddBitwiseMappings()
        {
            mappings["Edm.BitwiseOr"] = (f, args) =>
                Expression.Or(args[0], args[1]);

            mappings["Edm.BitwiseAnd"] = (f, args) =>
                Expression.And(args[0], args[1]);

            mappings["Edm.BitwiseXor"] = (f, args) =>
                Expression.ExclusiveOr(args[0], args[1]);

            mappings["Edm.BitwiseNot"] = (f, args) =>
                Expression.Not(args[0]);
        }

        private void AddMathMappings()
        {
            Map("Edm.Power", DoubleFunctions.Pow);

            MapMath("Edm.Ceiling",
                DecimalFunctions.Ceiling,
                DoubleFunctions.Ceiling);

            MapMath("Edm.Truncate",
                DecimalFunctions.Truncate,
                DoubleFunctions.Truncate);

            MapMath("Edm.Floor",
                DecimalFunctions.Floor,
                DoubleFunctions.Floor);

            mappings["Edm.Round"] = (f, args) => MapRound(f, args);

            mappings["Edm.Abs"] = (f, args) => MapAbs(f, args);
        }

        private void AddStringMappings(DbContainer container)
        {
            if (container.IsCaseSensitive)
            {
                Map("Edm.Contains", StringFunctions.Contains);

                Map("Edm.IndexOf", StringFunctions.IndexOf);

                Map("Edm.StartsWith", StringFunctions.StartsWith);

                Map("Edm.EndsWith", StringFunctions.EndsWith);
            }
            else
            {
                Map("Edm.Contains", StringFunctions.ContainsCaseInsensitive);

                Map("Edm.IndexOf", StringFunctions.IndexOfCaseInsensitive);

                Map("Edm.StartsWith", StringFunctions.StartsWithCaseInsensitive);

                Map("Edm.EndsWith", StringFunctions.EndsWithCaseInsensitive);
            }

            Map("Edm.Concat", StringFunctions.Concat);

            Map("Edm.Left", StringFunctions.Left);

            Map("Edm.Length", StringFunctions.Length);

            Map("Edm.LTrim", StringFunctions.LTrim);

            Map("Edm.Replace", StringFunctions.Replace);

            Map("Edm.Reverse", StringFunctions.ReverseString);

            Map("Edm.Right", StringFunctions.Right);

            Map("Edm.RTrim", StringFunctions.RTrim);

            Map("Edm.Substring", StringFunctions.Substring);

            Map("Edm.ToLower", StringFunctions.ToLower);

            Map("Edm.ToUpper", StringFunctions.ToUpper);

            Map("Edm.Trim", StringFunctions.Trim);
        }

        private void AddDateTimeMappings()
        {
            Map("Edm.CurrentDateTime",
                DateTimeFunctions.Current);

            Map("Edm.CurrentUtcDateTime",
                DateTimeFunctions.CurrentUtc);

            Map("Edm.CurrentDateTimeOffset",
                DateTimeOffsetFunctions.Current);

            Map("Edm.CreateDateTime",
                DateTimeFunctions.CreateDateTime);

            Map("Edm.CreateDateTimeOffset",
                DateTimeOffsetFunctions.CreateDateTimeOffset);

            Map("Edm.CreateTime",
                TimeFunctions.CreateTime);

            Map("Edm.GetTotalOffsetMinutes",
                DateTimeOffsetFunctions.GetTotalOffsetMinutes);

            MapDate("Edm.Year",
                DateTimeFunctions.GetYear,
                DateTimeOffsetFunctions.GetYear,
                null);

            MapDate("Edm.Month",
                DateTimeFunctions.GetMonth,
                DateTimeOffsetFunctions.GetMonth,
                null);

            MapDate("Edm.Day",
                DateTimeFunctions.GetDay,
                DateTimeOffsetFunctions.GetDay,
                null);

            MapDate("Edm.Hour",
                DateTimeFunctions.GetHour,
                DateTimeOffsetFunctions.GetHour,
                TimeFunctions.GetHour);

            MapDate("Edm.Minute",
                DateTimeFunctions.GetMinute,
                DateTimeOffsetFunctions.GetMinute,
                TimeFunctions.GetMinute);

            MapDate("Edm.Second",
                DateTimeFunctions.GetSecond,
                DateTimeOffsetFunctions.GetSecond,
                TimeFunctions.GetSecond);

            MapDate("Edm.Millisecond",
                DateTimeFunctions.GetMillisecond,
                DateTimeOffsetFunctions.GetMillisecond,
                TimeFunctions.GetMillisecond);

            MapDate("Edm.AddYears",
                DateTimeFunctions.AddYears,
                DateTimeOffsetFunctions.AddYears,
                null);

            MapDate("Edm.AddMonths",
                DateTimeFunctions.AddMonths,
                DateTimeOffsetFunctions.AddMonths,
                null);

            MapDate("Edm.AddDays",
                DateTimeFunctions.AddDays,
                DateTimeOffsetFunctions.AddDays,
                null);

            MapDate("Edm.AddHours",
                DateTimeFunctions.AddHours,
                DateTimeOffsetFunctions.AddHours,
                TimeFunctions.AddHours);

            MapDate("Edm.AddMinutes",
                DateTimeFunctions.AddMinutes,
                DateTimeOffsetFunctions.AddMinutes,
                TimeFunctions.AddMinutes);

            MapDate("Edm.AddSeconds",
                DateTimeFunctions.AddSeconds,
                DateTimeOffsetFunctions.AddSeconds,
                TimeFunctions.AddSeconds);

            MapDate("Edm.AddMilliseconds",
                DateTimeFunctions.AddMilliseconds,
                DateTimeOffsetFunctions.AddMilliseconds,
                TimeFunctions.AddMilliseconds);

            MapDate("Edm.AddMicroseconds",
                DateTimeFunctions.AddMicroseconds,
                DateTimeOffsetFunctions.AddMicroseconds,
                TimeFunctions.AddMicroseconds);

            MapDate("Edm.AddNanoseconds",
                DateTimeFunctions.AddNanoseconds,
                DateTimeOffsetFunctions.AddNanoseconds,
                TimeFunctions.AddNanoseconds);

            MapDate("Edm.DiffYears",
                DateTimeFunctions.DiffYears,
                DateTimeOffsetFunctions.DiffYears,
                null);

            MapDate("Edm.DiffMonths",
                DateTimeFunctions.DiffMonths,
                DateTimeOffsetFunctions.DiffMonths,
                null);

            MapDate("Edm.DiffDays",
                DateTimeFunctions.DiffDays,
                DateTimeOffsetFunctions.DiffDays,
                null);

            MapDate("Edm.DiffHours",
                DateTimeFunctions.DiffHours,
                DateTimeOffsetFunctions.DiffHours,
                TimeFunctions.DiffHours);

            MapDate("Edm.DiffMinutes",
                DateTimeFunctions.DiffMinutes,
                DateTimeOffsetFunctions.DiffMinutes,
                TimeFunctions.DiffMinutes);

            MapDate("Edm.DiffSeconds",
                DateTimeFunctions.DiffSeconds,
                DateTimeOffsetFunctions.DiffSeconds,
                TimeFunctions.DiffSeconds);

            MapDate("Edm.DiffMilliseconds",
                DateTimeFunctions.DiffMilliseconds,
                DateTimeOffsetFunctions.DiffMilliseconds,
                TimeFunctions.DiffMilliseconds);

            MapDate("Edm.DiffMicroseconds",
                DateTimeFunctions.DiffMicroseconds,
                DateTimeOffsetFunctions.DiffMicroseconds,
                TimeFunctions.DiffMicroseconds);

            MapDate("Edm.DiffNanoseconds",
                DateTimeFunctions.DiffNanoseconds,
                DateTimeOffsetFunctions.DiffNanoseconds,
                TimeFunctions.DiffNanoseconds);

            MapDate("Edm.TruncateTime",
                DateTimeFunctions.TruncateTime,
                DateTimeOffsetFunctions.TruncateTime,
                null);

            MapDate("Edm.DayOfYear",
                DateTimeFunctions.DayOfYear,
                DateTimeOffsetFunctions.DayOfYear,
                null);
        }

        private static MethodCallExpression MapRound(EdmFunction f, Expression[] args)
        {
            MethodInfo method = null;

            switch (args.Length)
            {
                case 1:
                    method = IsDecimal(f.Parameters[0]) ?
                        DecimalFunctions.Round :
                        DoubleFunctions.Round;
                    break;
                case 2:
                    method = IsDecimal(f.Parameters[0]) ?
                        DecimalFunctions.RoundDigits :
                        DoubleFunctions.RoundDigits;
                    break;
            }

            if (method == null)
            {
                throw new NotSupportedException(
                    string.Format(
                        "'{0}' function with {1} args is not supported",
                        f.FullName,
                        args.Length));
            }

            return Expression.Call(null, method, args);
        }

        private static MethodCallExpression MapAbs(EdmFunction f, Expression[] args)
        {
            return Expression.Call(
                null,
                GetAbsMethod(f.Parameters[0]),
                args[0]);
        }

        private static MethodInfo GetAbsMethod(FunctionParameter param)
        {
            var primitive = param.TypeUsage.EdmType as PrimitiveType;

            if (primitive == null)
            {
                return DoubleFunctions.Abs;
            }

            Type clrType = primitive.ClrEquivalentType;

            if (clrType == typeof(decimal))
            {
                return DecimalFunctions.Abs;
            }
            else if (clrType == typeof(long))
            {
                return IntegerFunctions.Abs64;
            }
            else if (clrType == typeof(int))
            {
                return IntegerFunctions.Abs32;
            }
            else if (clrType == typeof(short))
            {
                return IntegerFunctions.Abs16;
            }
            else if (clrType == typeof(sbyte))
            {
                return IntegerFunctions.Abs8;
            }

            return DoubleFunctions.Abs;
        }

        private void Map(string name, MethodInfo method)
        {
            Map(
                name,
                (f, args) => 0,
                method);
        }

        private void MapDate(string name,
            MethodInfo dateTime,
            MethodInfo dateTimeOffset,
            MethodInfo time)
        {
            Map(
                name,
                (f, args) => SelectDateTimeMethod(f, args),
                dateTime,
                dateTimeOffset,
                time);
        }

        private void MapMath(string name,
            MethodInfo decimalMethod,
            MethodInfo doubleMethod)
        {
            Map(
                name,
                (f, args) => SelectMathMethod(f, args),
                decimalMethod,
                doubleMethod);
        }

        private void Map(
            string name,
            Func<EdmFunction, Expression[], int> methodSelector,
            params MethodInfo[] methods)
        {
            mappings[name] = (f, args) =>
            {
                var i = methodSelector(f, args);

                if (i < 0 && methods.Length <= i)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Invalid method selector for '{0}' edm function",
                            f.FullName));
                }

                var method = methods[i];

                if (method == null)
                {
                    throw new NotSupportedException(
                        string.Format(
                            "'{0}' function is not supported with signature ({1})",
                            f.FullName,
                            ""));
                }

                args = FixArguments(method, args);

                return Expression.Call(null, method, args);
            };
        }

        private static bool IsDecimal(FunctionParameter param)
        {
            var primitive = param.TypeUsage.EdmType as PrimitiveType;

            if (primitive == null)
            {
                return false;
            }

            return primitive.ClrEquivalentType == typeof(decimal);
        }

        private static int SelectMathMethod(EdmFunction function, Expression[] args)
        {
            if (IsDecimal(function.Parameters[0]))
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        private static int SelectDateTimeMethod(EdmFunction function, Expression[] args)
        {
            var firstArg = TypeHelper.MakeNotNullable(args[0].Type);

            if (firstArg == typeof(DateTime))
            {
                return 0;
            }
            else if (firstArg == typeof(DateTimeOffset))
            {
                return 1;
            }
            else if (firstArg == typeof(TimeSpan))
            {
                return 2;
            }

            throw new NotSupportedException(
                string.Format("Type '{2}' is not supported for '{0}' date function ",
                    function.FullName,
                    firstArg.Name));
        }

        internal static Type GetTypeBinary(Type leftType, Type rightType)
        {
            var leftTypeCode = Type.GetTypeCode(leftType);
            var rightTypeCode = Type.GetTypeCode(rightType);

            if (leftTypeCode < rightTypeCode)
            {
                var backupLeftType = leftType;
                var backupLeftTypeCode = leftTypeCode;

                leftType = rightType;
                leftTypeCode = rightTypeCode;

                rightType = backupLeftType;
                rightTypeCode = backupLeftTypeCode;
            }

            Type type = null;

            if (leftTypeCode == TypeCode.Object || rightTypeCode == TypeCode.Object)
            {
                type = typeof(object);
            }
            else if (leftTypeCode == rightTypeCode && leftTypeCode != TypeCode.Char && leftTypeCode != TypeCode.SByte && leftTypeCode != TypeCode.Byte && leftTypeCode != TypeCode.Int16 && leftTypeCode != TypeCode.UInt16)
            {
                type = leftType.IsEnum && rightType.IsEnum ? rightType : leftType.IsEnum ? rightType : leftType;
            }
            else
            {
                switch (leftTypeCode)
                {
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                        switch (rightTypeCode)
                        {
                            case TypeCode.Char:
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                                type = typeof(int);
                                break;
                        }
                        break;
                    case TypeCode.UInt32:
                        switch (rightTypeCode)
                        {
                            case TypeCode.Char:
                            case TypeCode.Byte:
                            case TypeCode.UInt16:
                                type = typeof(uint);
                                break;
                            case TypeCode.SByte:
                            case TypeCode.Int16:
                            case TypeCode.Int32:
                                type = typeof(long);
                                break;
                        }
                        break;
                    case TypeCode.Int64:
                        switch (rightTypeCode)
                        {
                            case TypeCode.Char:
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                                type = typeof(long);
                                break;
                        }
                        break;
                    case TypeCode.UInt64:
                        switch (rightTypeCode)
                        {
                            case TypeCode.Char:
                            case TypeCode.Byte:
                            case TypeCode.UInt16:
                            case TypeCode.UInt32:
                                type = typeof(ulong);
                                break;
                        }
                        break;
                    case TypeCode.Single:
                        switch (rightTypeCode)
                        {
                            case TypeCode.Char:
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                type = typeof(float);
                                break;
                        }
                        break;
                    case TypeCode.Double:
                        switch (rightTypeCode)
                        {
                            case TypeCode.Char:
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                                type = typeof(double);
                                break;
                        }
                        break;
                    case TypeCode.Decimal:
                        switch (rightTypeCode)
                        {
                            case TypeCode.Char:
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                                type = typeof(decimal);
                                break;
                        }
                        break;
                    case TypeCode.DateTime:
                    case TypeCode.String:
                        type = typeof(string);
                        break;
                }
            }

            return type;
        }

        private static Expression[] FixArguments(MethodInfo method, Expression[] args)
        {
            var converted = new Expression[args.Length];
            var methodParams = method.GetParameters();

            for (var i = 0; i < args.Length; i++)
            {
                var expr = args[i];
                var expected = methodParams[i].ParameterType;

                if (expr.Type != expected)
                {
                    expr = Expression.Convert(expr, expected);
                }

                converted[i] = expr;
            }
            return converted;
        }


        public Expression CreateMethodCall(EdmFunction function, Expression[] arguments)
        {
            Func<EdmFunction, Expression[], Expression> mapper = null;

            if (!mappings.TryGetValue(function.FullName, out mapper))
            {
                throw new NotSupportedException(
                    string.Format(
                        "Missing mapping for '{0}' function",
                        function.FullName));
            }

            if (function.Name == "BitwiseOr" || function.Name == "BitwiseAnd" || function.Name == "BitwiseXor" || function.Name == "BitwiseNot")
            {
                var leftExpression = arguments[0];
                var rightExpression = arguments[1];

                var type = GetTypeBinary(leftExpression.Type, rightExpression.Type);

                if (type == typeof(object))
                {
                    if (leftExpression.Type.IsGenericType && leftExpression.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        if (!rightExpression.Type.IsGenericType || leftExpression.Type.GetGenericTypeDefinition() != rightExpression.Type.GetGenericTypeDefinition())
                        {
                            rightExpression = Expression.Convert(rightExpression, leftExpression.Type);
                        }
                    }
                    else if (rightExpression.Type.IsGenericType && rightExpression.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        if (!leftExpression.Type.IsGenericType || leftExpression.Type.GetGenericTypeDefinition() != rightExpression.Type.GetGenericTypeDefinition())
                        {
                            leftExpression = Expression.Convert(leftExpression, rightExpression.Type);
                        }
                    }
                }

                arguments[0] = leftExpression;
                arguments[1] = rightExpression;
            }

            return mapper(function, arguments);
        }
    }
}
