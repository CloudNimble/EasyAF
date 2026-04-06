// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    // <summary>
    // Used to configure a <see cref="TimeOnly" /> property of an entity type or
    // complex type.
    // </summary>
    internal class TimeOnlyPropertyConfiguration : PrimitivePropertyConfiguration
    {
        // <summary>
        // Gets or sets the precision of the property.
        // </summary>
        public byte? Precision { get; set; }

        // <summary>
        // Initializes a new instance of the TimeOnlyPropertyConfiguration class.
        // </summary>
        public TimeOnlyPropertyConfiguration()
        {
        }

        private TimeOnlyPropertyConfiguration(TimeOnlyPropertyConfiguration source)
            : base(source)
        {
            DebugCheck.NotNull(source);

            Precision = source.Precision;
        }

        internal override PrimitivePropertyConfiguration Clone()
        {
            return new TimeOnlyPropertyConfiguration(this);
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
            var timeConfigRhs = other as TimeOnlyPropertyConfiguration;
            if (timeConfigRhs is not null)
            {
                Precision = timeConfigRhs.Precision;
            }
        }

        internal override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            base.FillFrom(other, inCSpace);
            var timeConfigRhs = other as TimeOnlyPropertyConfiguration;
            if (timeConfigRhs is not null
                && Precision is null)
            {
                Precision = timeConfigRhs.Precision;
            }
        }

        internal override void MakeCompatibleWith(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            DebugCheck.NotNull(other);

            base.MakeCompatibleWith(other, inCSpace);

            var timeOnlyPropertyConfiguration = other as TimeOnlyPropertyConfiguration;

            if (timeOnlyPropertyConfiguration is null) return;
            if (timeOnlyPropertyConfiguration.Precision is not null) Precision = null;
        }

        internal override bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
        {
            var timeRhs = other as TimeOnlyPropertyConfiguration;

            var baseIsCompatible = base.IsCompatible(other, inCSpace, out errorMessage);
            var precisionIsCompatible = timeRhs is null || IsCompatible(c => c.Precision, timeRhs, ref errorMessage);

            return baseIsCompatible &&
                   precisionIsCompatible;
        }
    }
}