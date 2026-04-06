// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Objects.ELinq
{
    internal sealed partial class ExpressionConverter
    {
        internal sealed partial class MemberAccessTranslator
            : TypedTranslator<MemberExpression>
        {
            private sealed class SpatialPropertyTranslator : PropertyTranslator
            {
                private readonly Dictionary<PropertyInfo, string> propertyFunctionRenames = GetRenamedPropertyFunctions();

                internal SpatialPropertyTranslator()
                    : base(GetSupportedProperties())
                {
                }

                private static PropertyInfo GetProperty<T, TResult>(Expression<Func<T, TResult>> lambda)
                {
                    var memberEx = (MemberExpression)lambda.Body;
                    var property = (PropertyInfo)memberEx.Member;
                    Debug.Assert(
                        property.Getter().IsPublic &&
                        !property.Getter().IsStatic &&
                        (property.DeclaringType == typeof(DbGeography) || property.DeclaringType == typeof(DbGeometry)),
                        "GetProperty<T, TResult> should only be used to bind to public instance spatial properties");
                    return property;
                }

                private static IEnumerable<PropertyInfo> GetSupportedProperties()
                {
                    yield return GetProperty((DbGeography geo) => geo.CoordinateSystemId);
                    yield return GetProperty((DbGeography geo) => geo.SpatialTypeName);
                    yield return GetProperty((DbGeography geo) => geo.Dimension);
                    yield return GetProperty((DbGeography geo) => geo.IsEmpty);
                    yield return GetProperty((DbGeography geo) => geo.ElementCount);
                    yield return GetProperty((DbGeography geo) => geo.Latitude);
                    yield return GetProperty((DbGeography geo) => geo.Longitude);
                    yield return GetProperty((DbGeography geo) => geo.Elevation);
                    yield return GetProperty((DbGeography geo) => geo.Measure);
                    yield return GetProperty((DbGeography geo) => geo.Length);
                    yield return GetProperty((DbGeography geo) => geo.StartPoint);
                    yield return GetProperty((DbGeography geo) => geo.EndPoint);
                    yield return GetProperty((DbGeography geo) => geo.IsClosed);
                    yield return GetProperty((DbGeography geo) => geo.PointCount);
                    yield return GetProperty((DbGeography geo) => geo.Area);
                    yield return GetProperty((DbGeometry geo) => geo.CoordinateSystemId);
                    yield return GetProperty((DbGeometry geo) => geo.SpatialTypeName);
                    yield return GetProperty((DbGeometry geo) => geo.Dimension);
                    yield return GetProperty((DbGeometry geo) => geo.Envelope);
                    yield return GetProperty((DbGeometry geo) => geo.IsEmpty);
                    yield return GetProperty((DbGeometry geo) => geo.IsSimple);
                    yield return GetProperty((DbGeometry geo) => geo.Boundary);
                    yield return GetProperty((DbGeometry geo) => geo.IsValid);
                    yield return GetProperty((DbGeometry geo) => geo.ConvexHull);
                    yield return GetProperty((DbGeometry geo) => geo.ElementCount);
                    yield return GetProperty((DbGeometry geo) => geo.XCoordinate);
                    yield return GetProperty((DbGeometry geo) => geo.YCoordinate);
                    yield return GetProperty((DbGeometry geo) => geo.Elevation);
                    yield return GetProperty((DbGeometry geo) => geo.Measure);
                    yield return GetProperty((DbGeometry geo) => geo.Length);
                    yield return GetProperty((DbGeometry geo) => geo.StartPoint);
                    yield return GetProperty((DbGeometry geo) => geo.EndPoint);
                    yield return GetProperty((DbGeometry geo) => geo.IsClosed);
                    yield return GetProperty((DbGeometry geo) => geo.IsRing);
                    yield return GetProperty((DbGeometry geo) => geo.PointCount);
                    yield return GetProperty((DbGeometry geo) => geo.Area);
                    yield return GetProperty((DbGeometry geo) => geo.Centroid);
                    yield return GetProperty((DbGeometry geo) => geo.PointOnSurface);
                    yield return GetProperty((DbGeometry geo) => geo.ExteriorRing);
                    yield return GetProperty((DbGeometry geo) => geo.InteriorRingCount);
                }

                private static Dictionary<PropertyInfo, string> GetRenamedPropertyFunctions()
                {
                    var result = new Dictionary<PropertyInfo, string>
                    {
                        { GetProperty((DbGeography geo) => geo.CoordinateSystemId), "CoordinateSystemId" },
                        { GetProperty((DbGeography geo) => geo.SpatialTypeName), "SpatialTypeName" },
                        { GetProperty((DbGeography geo) => geo.Dimension), "SpatialDimension" },
                        { GetProperty((DbGeography geo) => geo.IsEmpty), "IsEmptySpatial" },
                        { GetProperty((DbGeography geo) => geo.ElementCount), "SpatialElementCount" },
                        { GetProperty((DbGeography geo) => geo.Latitude), "Latitude" },
                        { GetProperty((DbGeography geo) => geo.Longitude), "Longitude" },
                        { GetProperty((DbGeography geo) => geo.Elevation), "Elevation" },
                        { GetProperty((DbGeography geo) => geo.Measure), "Measure" },
                        { GetProperty((DbGeography geo) => geo.Length), "SpatialLength" },
                        { GetProperty((DbGeography geo) => geo.StartPoint), "StartPoint" },
                        { GetProperty((DbGeography geo) => geo.EndPoint), "EndPoint" },
                        { GetProperty((DbGeography geo) => geo.IsClosed), "IsClosedSpatial" },
                        { GetProperty((DbGeography geo) => geo.PointCount), "PointCount" },
                        { GetProperty((DbGeography geo) => geo.Area), "Area" },
                        { GetProperty((DbGeometry geo) => geo.CoordinateSystemId), "CoordinateSystemId" },
                        { GetProperty((DbGeometry geo) => geo.SpatialTypeName), "SpatialTypeName" },
                        { GetProperty((DbGeometry geo) => geo.Dimension), "SpatialDimension" },
                        { GetProperty((DbGeometry geo) => geo.Envelope), "SpatialEnvelope" },
                        { GetProperty((DbGeometry geo) => geo.IsEmpty), "IsEmptySpatial" },
                        { GetProperty((DbGeometry geo) => geo.IsSimple), "IsSimpleGeometry" },
                        { GetProperty((DbGeometry geo) => geo.Boundary), "SpatialBoundary" },
                        { GetProperty((DbGeometry geo) => geo.IsValid), "IsValidGeometry" },
                        { GetProperty((DbGeometry geo) => geo.ConvexHull), "SpatialConvexHull" },
                        { GetProperty((DbGeometry geo) => geo.ElementCount), "SpatialElementCount" },
                        { GetProperty((DbGeometry geo) => geo.XCoordinate), "XCoordinate" },
                        { GetProperty((DbGeometry geo) => geo.YCoordinate), "YCoordinate" },
                        { GetProperty((DbGeometry geo) => geo.Elevation), "Elevation" },
                        { GetProperty((DbGeometry geo) => geo.Measure), "Measure" },
                        { GetProperty((DbGeometry geo) => geo.Length), "SpatialLength" },
                        { GetProperty((DbGeometry geo) => geo.StartPoint), "StartPoint" },
                        { GetProperty((DbGeometry geo) => geo.EndPoint), "EndPoint" },
                        { GetProperty((DbGeometry geo) => geo.IsClosed), "IsClosedSpatial" },
                        { GetProperty((DbGeometry geo) => geo.IsRing), "IsRing" },
                        { GetProperty((DbGeometry geo) => geo.PointCount), "PointCount" },
                        { GetProperty((DbGeometry geo) => geo.Area), "Area" },
                        { GetProperty((DbGeometry geo) => geo.Centroid), "Centroid" },
                        { GetProperty((DbGeometry geo) => geo.PointOnSurface), "PointOnSurface" },
                        { GetProperty((DbGeometry geo) => geo.ExteriorRing), "ExteriorRing" },
                        { GetProperty((DbGeometry geo) => geo.InteriorRingCount), "InteriorRingCount" }
                    };
                    return result;
                }

                // Translator for spatial properties into canonical functions. Both static and instance properties are handled.
                // Unless a canonical function name is explicitly specified for a property, the mapping from property name to
                // canonical function name consists simply of applying the 'ST' prefix. Then, translation proceeds as follows:
                //      object.PropertyName  -> CanonicalFunctionName(object)
                //      Type.PropertyName  -> CanonicalFunctionName()
                internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
                {
                    var property = (PropertyInfo)call.Member;
                    if (!propertyFunctionRenames.TryGetValue(property, out var canonicalFunctionName))
                    {
                        canonicalFunctionName = "ST" + property.Name;
                    }

                    Debug.Assert(call.Expression is not null, "No static spatial properties currently map to canonical functions");
                    DbExpression result = parent.TranslateIntoCanonicalFunction(canonicalFunctionName, call, call.Expression);
                    return result;
                }
            }
        }
    }
}
