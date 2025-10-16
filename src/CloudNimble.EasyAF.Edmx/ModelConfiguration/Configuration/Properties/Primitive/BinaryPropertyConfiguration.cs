// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    // <summary>
    // Used to configure a <see cref="T:Byte[]" /> property of an entity type or
    // complex type.
    // </summary>
    internal class BinaryPropertyConfiguration : LengthPropertyConfiguration
    {
        // <summary>
        // Gets or sets a value indicating whether the property is a row version in the
        // database.
        // </summary>
        public bool? IsRowVersion { get; set; }

        // <summary>
        // Initializes a new instance of the BinaryPropertyConfiguration class.
        // </summary>
        public BinaryPropertyConfiguration()
        {
        }

        private BinaryPropertyConfiguration(BinaryPropertyConfiguration source)
            : base(source)
        {
            DebugCheck.NotNull(source);

            IsRowVersion = source.IsRowVersion;
        }

        internal override PrimitivePropertyConfiguration Clone()
        {
            return new BinaryPropertyConfiguration(this);
        }

        protected override void ConfigureProperty(EdmProperty property)
        {
            if (IsRowVersion is not null
                && IsRowVersion.Value)
            {
                ConcurrencyMode = ConcurrencyMode ?? Core.Metadata.Edm.ConcurrencyMode.Fixed;
                DatabaseGeneratedOption
                    = DatabaseGeneratedOption
                      ?? ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed;
                IsNullable = IsNullable ?? false;
                MaxLength = MaxLength ?? 8;
            }

            base.ConfigureProperty(property);
        }

        protected override void ConfigureColumn(EdmProperty column, EntityType table, DbProviderManifest providerManifest)
        {
            if (IsRowVersion is not null
                && IsRowVersion.Value)
            {
                ColumnType = ColumnType ?? "rowversion";
            }

            base.ConfigureColumn(column, table, providerManifest);

            if (IsRowVersion is not null
                && IsRowVersion.Value)
            {
                column.MaxLength = null;
            }
        }

        internal override void CopyFrom(PrimitivePropertyConfiguration other)
        {
            base.CopyFrom(other);
            var strConfigRhs = other as BinaryPropertyConfiguration;
            if (strConfigRhs is not null)
            {
                IsRowVersion = strConfigRhs.IsRowVersion;
            }
        }

        internal override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            base.FillFrom(other, inCSpace);
            var strConfigRhs = other as BinaryPropertyConfiguration;
            if (strConfigRhs is not null
                && IsRowVersion is null)
            {
                IsRowVersion = strConfigRhs.IsRowVersion;
            }
        }

        internal override void MakeCompatibleWith(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            DebugCheck.NotNull(other);

            base.MakeCompatibleWith(other, inCSpace);

            var binaryPropertyConfiguration = other as BinaryPropertyConfiguration;

            if (binaryPropertyConfiguration is null) return;
            if (binaryPropertyConfiguration.IsRowVersion is not null) IsRowVersion = null;
        }
        
        internal override bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
        {
            var binaryRhs = other as BinaryPropertyConfiguration;

            var baseIsCompatible = base.IsCompatible(other, inCSpace, out errorMessage);
            var isRowVersionIsCompatible = binaryRhs is null
                                           || IsCompatible(c => c.IsRowVersion, binaryRhs, ref errorMessage);

            return baseIsCompatible &&
                   isRowVersionIsCompatible;
        }
    }
}
