// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Edm
{
    internal static class EnumTypeExtensions
    {
        public static Type GetClrType(this EnumType enumType)
        {
            DebugCheck.NotNull(enumType);

            return enumType.Annotations.GetClrType();
        }

        public static void SetClrType(this EnumType enumType, Type type)
        {
            DebugCheck.NotNull(enumType);
            DebugCheck.NotNull(type);

            enumType.GetMetadataProperties().SetClrType(type);
        }
    }
}
