// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Edm
{
    internal static class EdmTypeExtensions
    {
        public static Type GetClrType(this EdmType item)
        {
            DebugCheck.NotNull(item);

            var asEntityType = item as EntityType;
            if (asEntityType is not null)
            {
                return asEntityType.GetClrType();
            }

            var asEnumType = item as EnumType;
            if (asEnumType is not null)
            {
                return asEnumType.GetClrType();
            }

            var asComplexType = item as ComplexType;
            if (asComplexType is not null)
            {
                return asComplexType.GetClrType();
            }

            return null;
        }
    }
}
