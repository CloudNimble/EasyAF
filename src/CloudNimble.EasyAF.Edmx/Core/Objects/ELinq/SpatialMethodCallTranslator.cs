// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Spatial;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Objects.ELinq
{
    internal sealed partial class ExpressionConverter
    {
        internal sealed partial class MethodCallTranslator
            : TypedTranslator<MethodCallExpression>
        {
            private sealed class SpatialMethodCallTranslator : CallTranslator
            {
                private static readonly Dictionary<MethodInfo, string> _methodFunctionRenames = GetRenamedMethodFunctions();

                internal SpatialMethodCallTranslator()
                    : base(GetSupportedMethods())
                {
                }

                private static MethodInfo GetStaticMethod<TResult>(Expression<Func<TResult>> lambda)
                {
                    var method = ((MethodCallExpression)lambda.Body).Method;
                    Debug.Assert(
                        method.IsStatic && method.IsPublic &&
                        (method.DeclaringType == typeof(DbGeography) || method.DeclaringType == typeof(DbGeometry)),
                        "Supported static spatial methods should be public static methods declared by a spatial type");
                    return method;
                }

                private static MethodInfo GetInstanceMethod<T, TResult>(Expression<Func<T, TResult>> lambda)
                {
                    var method = ((MethodCallExpression)lambda.Body).Method;
                    Debug.Assert(
                        !method.IsStatic && method.IsPublic &&
                        (method.DeclaringType == typeof(DbGeography) || method.DeclaringType == typeof(DbGeometry)),
                        "Supported instance spatial methods should be public instance methods declared by a spatial type");
                    return method;
                }

                private static IEnumerable<MethodInfo> GetSupportedMethods()
                {
                    yield return GetStaticMethod(() => DbGeography.FromText(default(string)));
                    yield return GetStaticMethod(() => DbGeography.FromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.PointFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.LineFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.PolygonFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.MultiPointFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.MultiLineFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.MultiPolygonFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.GeographyCollectionFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.FromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.FromBinary(default(byte[])));
                    yield return GetStaticMethod(() => DbGeography.PointFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.LineFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.PolygonFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.MultiPointFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.MultiLineFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.MultiPolygonFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.GeographyCollectionFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.FromGml(default(string)));
                    yield return GetStaticMethod(() => DbGeography.FromGml(default(string), default(int)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.AsBinary());
                    yield return GetInstanceMethod((DbGeography geo) => geo.AsGml());
                    yield return GetInstanceMethod((DbGeography geo) => geo.AsText());
                    yield return GetInstanceMethod((DbGeography geo) => geo.SpatialEquals(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Disjoint(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Intersects(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Buffer(default(double)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Distance(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Intersection(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Union(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Difference(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.SymmetricDifference(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.ElementAt(default(int)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.PointAt(default(int)));
                    yield return GetStaticMethod(() => DbGeometry.FromText(default(string)));
                    yield return GetStaticMethod(() => DbGeometry.FromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.PointFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.LineFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.PolygonFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.MultiPointFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.MultiLineFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.MultiPolygonFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.GeometryCollectionFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.FromBinary(default(byte[])));
                    yield return GetStaticMethod(() => DbGeometry.FromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.PointFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.LineFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.PolygonFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.MultiPointFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.MultiLineFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.MultiPolygonFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.GeometryCollectionFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.FromGml(default(string)));
                    yield return GetStaticMethod(() => DbGeometry.FromGml(default(string), default(int)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.AsBinary());
                    yield return GetInstanceMethod((DbGeometry geo) => geo.AsGml());
                    yield return GetInstanceMethod((DbGeometry geo) => geo.AsText());
                    yield return GetInstanceMethod((DbGeometry geo) => geo.SpatialEquals(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Disjoint(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Intersects(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Touches(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Crosses(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Within(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Contains(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Overlaps(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Relate(default(DbGeometry), default(string)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Buffer(default(double)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Distance(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Intersection(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Union(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Difference(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.SymmetricDifference(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.ElementAt(default(int)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.PointAt(default(int)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.InteriorRingAt(default(int)));
                }

                private static Dictionary<MethodInfo, string> GetRenamedMethodFunctions()
                {
                    var result = new Dictionary<MethodInfo, string>
                    {
                        { GetStaticMethod(() => DbGeography.FromText(default(string))), "GeographyFromText" },
                        { GetStaticMethod(() => DbGeography.FromText(default(string), default(int))), "GeographyFromText" },
                        { GetStaticMethod(() => DbGeography.PointFromText(default(string), default(int))), "GeographyPointFromText" },
                        { GetStaticMethod(() => DbGeography.LineFromText(default(string), default(int))), "GeographyLineFromText" },
                        { GetStaticMethod(() => DbGeography.PolygonFromText(default(string), default(int))), "GeographyPolygonFromText" },
                        { GetStaticMethod(() => DbGeography.MultiPointFromText(default(string), default(int))), "GeographyMultiPointFromText" },
                        { GetStaticMethod(() => DbGeography.MultiLineFromText(default(string), default(int))), "GeographyMultiLineFromText" },
                        {
                            GetStaticMethod(() => DbGeography.MultiPolygonFromText(default(string), default(int))),
                            "GeographyMultiPolygonFromText"
                        },
                        {
                            GetStaticMethod(() => DbGeography.GeographyCollectionFromText(default(string), default(int))),
                            "GeographyCollectionFromText"
                        },
                        { GetStaticMethod(() => DbGeography.FromBinary(default(byte[]), default(int))), "GeographyFromBinary" },
                        { GetStaticMethod(() => DbGeography.FromBinary(default(byte[]))), "GeographyFromBinary" },
                        { GetStaticMethod(() => DbGeography.PointFromBinary(default(byte[]), default(int))), "GeographyPointFromBinary" },
                        { GetStaticMethod(() => DbGeography.LineFromBinary(default(byte[]), default(int))), "GeographyLineFromBinary" },
                        { GetStaticMethod(() => DbGeography.PolygonFromBinary(default(byte[]), default(int))), "GeographyPolygonFromBinary" },
                        {
                            GetStaticMethod(() => DbGeography.MultiPointFromBinary(default(byte[]), default(int))),
                            "GeographyMultiPointFromBinary"
                        },
                        {
                            GetStaticMethod(() => DbGeography.MultiLineFromBinary(default(byte[]), default(int))),
                            "GeographyMultiLineFromBinary"
                        },
                        {
                            GetStaticMethod(() => DbGeography.MultiPolygonFromBinary(default(byte[]), default(int))),
                            "GeographyMultiPolygonFromBinary"
                        },
                        {
                            GetStaticMethod(() => DbGeography.GeographyCollectionFromBinary(default(byte[]), default(int))),
                            "GeographyCollectionFromBinary"
                        },
                        { GetStaticMethod(() => DbGeography.FromGml(default(string))), "GeographyFromGml" },
                        { GetStaticMethod(() => DbGeography.FromGml(default(string), default(int))), "GeographyFromGml" },
                        { GetInstanceMethod((DbGeography geo) => geo.AsBinary()), "AsBinary" },
                        { GetInstanceMethod((DbGeography geo) => geo.AsGml()), "AsGml" },
                        { GetInstanceMethod((DbGeography geo) => geo.AsText()), "AsText" },
                        { GetInstanceMethod((DbGeography geo) => geo.SpatialEquals(default(DbGeography))), "SpatialEquals" },
                        { GetInstanceMethod((DbGeography geo) => geo.Disjoint(default(DbGeography))), "SpatialDisjoint" },
                        { GetInstanceMethod((DbGeography geo) => geo.Intersects(default(DbGeography))), "SpatialIntersects" },
                        { GetInstanceMethod((DbGeography geo) => geo.Buffer(default(double))), "SpatialBuffer" },
                        { GetInstanceMethod((DbGeography geo) => geo.Distance(default(DbGeography))), "Distance" },
                        { GetInstanceMethod((DbGeography geo) => geo.Intersection(default(DbGeography))), "SpatialIntersection" },
                        { GetInstanceMethod((DbGeography geo) => geo.Union(default(DbGeography))), "SpatialUnion" },
                        { GetInstanceMethod((DbGeography geo) => geo.Difference(default(DbGeography))), "SpatialDifference" },
                        { GetInstanceMethod((DbGeography geo) => geo.SymmetricDifference(default(DbGeography))), "SpatialSymmetricDifference" },
                        { GetInstanceMethod((DbGeography geo) => geo.ElementAt(default(int))), "SpatialElementAt" },
                        { GetInstanceMethod((DbGeography geo) => geo.PointAt(default(int))), "PointAt" },
                        { GetStaticMethod(() => DbGeometry.FromText(default(string))), "GeometryFromText" },
                        { GetStaticMethod(() => DbGeometry.FromText(default(string), default(int))), "GeometryFromText" },
                        { GetStaticMethod(() => DbGeometry.PointFromText(default(string), default(int))), "GeometryPointFromText" },
                        { GetStaticMethod(() => DbGeometry.LineFromText(default(string), default(int))), "GeometryLineFromText" },
                        { GetStaticMethod(() => DbGeometry.PolygonFromText(default(string), default(int))), "GeometryPolygonFromText" },
                        { GetStaticMethod(() => DbGeometry.MultiPointFromText(default(string), default(int))), "GeometryMultiPointFromText" },
                        { GetStaticMethod(() => DbGeometry.MultiLineFromText(default(string), default(int))), "GeometryMultiLineFromText" },
                        {
                            GetStaticMethod(() => DbGeometry.MultiPolygonFromText(default(string), default(int))),
                            "GeometryMultiPolygonFromText"
                        },
                        {
                            GetStaticMethod(() => DbGeometry.GeometryCollectionFromText(default(string), default(int))),
                            "GeometryCollectionFromText"
                        },
                        { GetStaticMethod(() => DbGeometry.FromBinary(default(byte[]))), "GeometryFromBinary" },
                        { GetStaticMethod(() => DbGeometry.FromBinary(default(byte[]), default(int))), "GeometryFromBinary" },
                        { GetStaticMethod(() => DbGeometry.PointFromBinary(default(byte[]), default(int))), "GeometryPointFromBinary" },
                        { GetStaticMethod(() => DbGeometry.LineFromBinary(default(byte[]), default(int))), "GeometryLineFromBinary" },
                        { GetStaticMethod(() => DbGeometry.PolygonFromBinary(default(byte[]), default(int))), "GeometryPolygonFromBinary" },
                        {
                            GetStaticMethod(() => DbGeometry.MultiPointFromBinary(default(byte[]), default(int))),
                            "GeometryMultiPointFromBinary"
                        },
                        { GetStaticMethod(() => DbGeometry.MultiLineFromBinary(default(byte[]), default(int))), "GeometryMultiLineFromBinary" },
                        {
                            GetStaticMethod(() => DbGeometry.MultiPolygonFromBinary(default(byte[]), default(int))),
                            "GeometryMultiPolygonFromBinary"
                        },
                        {
                            GetStaticMethod(() => DbGeometry.GeometryCollectionFromBinary(default(byte[]), default(int))),
                            "GeometryCollectionFromBinary"
                        },
                        { GetStaticMethod(() => DbGeometry.FromGml(default(string))), "GeometryFromGml" },
                        { GetStaticMethod(() => DbGeometry.FromGml(default(string), default(int))), "GeometryFromGml" },
                        { GetInstanceMethod((DbGeometry geo) => geo.AsBinary()), "AsBinary" },
                        { GetInstanceMethod((DbGeometry geo) => geo.AsGml()), "AsGml" },
                        { GetInstanceMethod((DbGeometry geo) => geo.AsText()), "AsText" },
                        { GetInstanceMethod((DbGeometry geo) => geo.SpatialEquals(default(DbGeometry))), "SpatialEquals" },
                        { GetInstanceMethod((DbGeometry geo) => geo.Disjoint(default(DbGeometry))), "SpatialDisjoint" },
                        { GetInstanceMethod((DbGeometry geo) => geo.Intersects(default(DbGeometry))), "SpatialIntersects" },
                        { GetInstanceMethod((DbGeometry geo) => geo.Touches(default(DbGeometry))), "SpatialTouches" },
                        { GetInstanceMethod((DbGeometry geo) => geo.Crosses(default(DbGeometry))), "SpatialCrosses" },
                        { GetInstanceMethod((DbGeometry geo) => geo.Within(default(DbGeometry))), "SpatialWithin" },
                        { GetInstanceMethod((DbGeometry geo) => geo.Contains(default(DbGeometry))), "SpatialContains" },
                        { GetInstanceMethod((DbGeometry geo) => geo.Overlaps(default(DbGeometry))), "SpatialOverlaps" },
                        { GetInstanceMethod((DbGeometry geo) => geo.Relate(default(DbGeometry), default(string))), "SpatialRelate" },
                        { GetInstanceMethod((DbGeometry geo) => geo.Buffer(default(double))), "SpatialBuffer" },
                        { GetInstanceMethod((DbGeometry geo) => geo.Distance(default(DbGeometry))), "Distance" },
                        { GetInstanceMethod((DbGeometry geo) => geo.Intersection(default(DbGeometry))), "SpatialIntersection" },
                        { GetInstanceMethod((DbGeometry geo) => geo.Union(default(DbGeometry))), "SpatialUnion" },
                        { GetInstanceMethod((DbGeometry geo) => geo.Difference(default(DbGeometry))), "SpatialDifference" },
                        { GetInstanceMethod((DbGeometry geo) => geo.SymmetricDifference(default(DbGeometry))), "SpatialSymmetricDifference" },
                        { GetInstanceMethod((DbGeometry geo) => geo.ElementAt(default(int))), "SpatialElementAt" },
                        { GetInstanceMethod((DbGeometry geo) => geo.PointAt(default(int))), "PointAt" },
                        { GetInstanceMethod((DbGeometry geo) => geo.InteriorRingAt(default(int))), "InteriorRingAt" }
                    };
                    return result;
                }

                // Translator for spatial methods into canonical functions. Both static and instance methods are handled.
                // Unless a canonical function name is explicitly specified for a method, the mapping from method name to
                // canonical function name consists simply of applying the 'ST' prefix. Then, translation proceeds as follows:
                //      object.MethodName(args...)  -> CanonicalFunctionName(object, args...)
                //      Type.MethodName(args...)  -> CanonicalFunctionName(args...)
                internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    var method = call.Method;
                    if (!_methodFunctionRenames.TryGetValue(method, out var canonicalFunctionName))
                    {
                        canonicalFunctionName = "ST" + method.Name;
                    }

                    Expression[] arguments;
                    if (method.IsStatic)
                    {
                        Debug.Assert(call.Object is null, "Static method call with instance argument?");
                        arguments = call.Arguments.ToArray();
                    }
                    else
                    {
                        Debug.Assert(call.Object is not null, "Instance method call with no instance argument?");
                        arguments = new[] { call.Object }.Concat(call.Arguments).ToArray();
                    }

                    DbExpression result = parent.TranslateIntoCanonicalFunction(canonicalFunctionName, call, arguments);
                    return result;
                }
            }
        }
    }
}
