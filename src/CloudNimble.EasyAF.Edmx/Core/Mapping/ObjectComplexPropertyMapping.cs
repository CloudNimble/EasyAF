// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping
{
    // <summary>
    // Mapping metadata for complex member maps.
    // </summary>
    internal class ObjectComplexPropertyMapping : ObjectPropertyMapping
    {
        // <summary>
        // Constrcut a new member mapping metadata object
        // </summary>
        internal ObjectComplexPropertyMapping(EdmProperty edmProperty, EdmProperty clrProperty)
            : base(edmProperty, clrProperty)
        {
        }

        // <summary>
        // return the member mapping kind
        // </summary>
        internal override MemberMappingKind MemberMappingKind
        {
            get { return MemberMappingKind.ComplexPropertyMapping; }
        }
    }
}
