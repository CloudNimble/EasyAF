// --------------------------------------------------------------------------------------------
// <copyright file="EdmTypeConverter.cs" company="Effort Team">
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

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.TypeConversion
{
    using System;
    using System.Collections.Generic;
#if !EFOLD
    using System.Data.Entity.Core.Metadata.Edm;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.Common;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.TypeGeneration;
#else
    using System.Data.Metadata.Edm;
#endif

    internal class EdmTypeConverter
    {
        private ITypeConverter converter;

        public EdmTypeConverter(ITypeConverter converter)
        {
            this.converter = converter;
        }

        public Type Convert(TypeUsage type)
        {
            var facets = GetTypeFacets(type);
            return ConvertWithFacets(type, facets);
        }

        public Type ConvertNotNull(TypeUsage type)
        {
            var facets = new FacetInfo();

            return ConvertWithFacets(type, facets);
        }

        public Type GetElementType(TypeUsage type)
        {
            var collectionType = type.EdmType as CollectionType;

            if (collectionType is null)
            {
                throw new ArgumentException("type");
            }

            return this.Convert(collectionType.TypeUsage);
        }

        public FacetInfo GetTypeFacets(TypeUsage type)
        {
            var facets = new FacetInfo();
            Facet facet = null;

            if (type.Facets.TryGetValue("Nullable", false, out facet))
            {
                facets.Nullable = (bool)facet.Value == true;
            }

            if (type.Facets.TryGetValue("FixedLength", false, out facet))
            {
                if (!facet.IsUnbounded && facet.Value is not null)
                {
                    facets.FixedLength = (bool)facet.Value == true;
                }
            }

            if (type.Facets.TryGetValue("StoreGeneratedPattern", false, out facet))
            {
                switch ((StoreGeneratedPattern)facet.Value)
                {
                    case StoreGeneratedPattern.Computed:
                        facets.Computed = true;
                        break;
                    case StoreGeneratedPattern.Identity:
                        facets.Identity = true;
                        break;
                }
            }

            if (type.Facets.TryGetValue("MaxLength", false, out facet))
            {
                if (facet.IsUnbounded)
                {
                    facets.LimitedLength = false;
                }
                else if (facet.Value is not null)
                {
                    facets.MaxLength = (int)facet.Value;
                    facets.LimitedLength = true;
                }
            }

            return facets;
        }

        private Type ConvertWithFacets(TypeUsage type, FacetInfo facets)
        {
            if (type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
            {
                return CreatePrimitiveType(type.EdmType as PrimitiveType, facets);
            }
            else if (type.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType)
            {
                return CreateRowType(type.EdmType as RowType, facets);
            }
            else if (type.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType)
            {
                return CreateCollectionType(type.EdmType as CollectionType, facets);
            }

            throw new NotSupportedException();
        }

        private Type CreatePrimitiveType(PrimitiveType primitiveType, FacetInfo facets)
        {
            Type result = null;
            if (converter.TryConvertEdmType(primitiveType, facets, out result))
            {
                return result;
            }

            result = primitiveType.ClrEquivalentType;

            if (facets.Nullable && result.IsValueType)
            {
                result = typeof(Nullable<>).MakeGenericType(result);
            }

            return result;
        }

        private Type CreateRowType(RowType rowType, FacetInfo facets)
        {
            var members = new Dictionary<string, Type>();

            foreach (EdmMember member in rowType.Members)
            {
                members.Add(member.GetColumnName(), this.Convert(member.TypeUsage));
            }

            var result = DataRowFactory.Create(members);

            return result;
        }

        private Type CreateCollectionType(CollectionType collectionType, FacetInfo facets)
        {
            var elementType = this.ConvertWithFacets(collectionType.TypeUsage, facets);

            return elementType.MakeArrayType();
        }
    }
}