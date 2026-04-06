// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping
{
    // <summary>
    // Mapping metadata for all OC member maps.
    // </summary>
    internal class ObjectPropertyMapping : ObjectMemberMapping
    {
        // <summary>
        // Constrcut a new member mapping metadata object
        // </summary>
        internal ObjectPropertyMapping(EdmProperty edmProperty, EdmProperty clrProperty)
            :
                base(edmProperty, clrProperty)
        {
        }

        // <summary>
        // The PropertyMetadata object that represents the Clr member for which mapping is being specified
        // </summary>
        internal EdmProperty ClrProperty
        {
            get { return (EdmProperty)ClrMember; }
        }

        // <summary>
        // return the member mapping kind
        // </summary>
        internal override MemberMappingKind MemberMappingKind
        {
            get { return MemberMappingKind.ScalarPropertyMapping; }
        }
    }
}
