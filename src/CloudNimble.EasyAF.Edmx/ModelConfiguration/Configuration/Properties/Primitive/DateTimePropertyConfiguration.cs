// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    // <summary>
    // Used to configure a <see cref="DateTime" /> property of an entity type or
    // complex type.
    // </summary>
    internal class DateTimePropertyConfiguration : PrimitivePropertyConfiguration
    {
        // <summary>
        // Gets or sets the precision of the property.
        // </summary>
        public byte? Precision { get; set; }

        // <summary>
        // Initializes a new instance of the DateTimePropertyConfiguration class.
        // </summary>
        public DateTimePropertyConfiguration()
        {
        }

        private DateTimePropertyConfiguration(DateTimePropertyConfiguration source)
            : base(source)
        {
            DebugCheck.NotNull(source);

            Precision = source.Precision;
        }

        internal override PrimitivePropertyConfiguration Clone()
        {
            return new DateTimePropertyConfiguration(this);
        }

        protected override void ConfigureProperty(EdmProperty property)
        {
            base.ConfigureProperty(property);

            if (Precision is not null)
            {
                property.Precision = Precision;
            }
        }

        internal override void Configure(EdmProperty column, FacetDescription facetDescription)
        {
            base.Configure(column, facetDescription);

            switch (facetDescription.FacetName)
            {
                case XmlConstants.PrecisionElement:
                    column.Precision = facetDescription.IsConstant ? null : Precision ?? column.Precision;
                    break;
            }
        }

        internal override void CopyFrom(PrimitivePropertyConfiguration other)
        {
            base.CopyFrom(other);
            var strConfigRhs = other as DateTimePropertyConfiguration;
            if (strConfigRhs is not null)
            {
                Precision = strConfigRhs.Precision;
            }
        }

        internal override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            base.FillFrom(other, inCSpace);
            var strConfigRhs = other as DateTimePropertyConfiguration;
            if (strConfigRhs is not null
                && Precision is null)
            {
                Precision = strConfigRhs.Precision;
            }
        }

        internal override void MakeCompatibleWith(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            DebugCheck.NotNull(other);

            base.MakeCompatibleWith(other, inCSpace);

            var dateTimePropertyConfiguration = other as DateTimePropertyConfiguration;

            if (dateTimePropertyConfiguration is null) return;
            if (dateTimePropertyConfiguration.Precision is not null) Precision = null;
        }

        internal override bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
        {
            var dateRhs = other as DateTimePropertyConfiguration;

            var baseIsCompatible = base.IsCompatible(other, inCSpace, out errorMessage);
            var precisionIsCompatible = dateRhs is null || IsCompatible(c => c.Precision, dateRhs, ref errorMessage);

            return baseIsCompatible &&
                   precisionIsCompatible;
        }
    }
}
