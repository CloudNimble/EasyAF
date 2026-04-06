// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    // <summary>
    // Used to configure a <see cref="DateOnly" /> property of an entity type or
    // complex type.
    // </summary>
    internal class DateOnlyPropertyConfiguration : PrimitivePropertyConfiguration
    {
        // <summary>
        // Initializes a new instance of the DateOnlyPropertyConfiguration class.
        // </summary>
        public DateOnlyPropertyConfiguration()
        {
        }

        private DateOnlyPropertyConfiguration(DateOnlyPropertyConfiguration source)
            : base(source)
        {
            DebugCheck.NotNull(source);
        }

        internal override PrimitivePropertyConfiguration Clone()
        {
            return new DateOnlyPropertyConfiguration(this);
        }

        protected override void ConfigureProperty(EdmProperty property)
        {
            base.ConfigureProperty(property);
            // DateOnly has no additional facets to configure
        }

        internal override void Configure(EdmProperty column, FacetDescription facetDescription)
        {
            base.Configure(column, facetDescription);
            // DateOnly has no additional facets to configure
        }

        internal override void CopyFrom(PrimitivePropertyConfiguration other)
        {
            base.CopyFrom(other);
            // DateOnly has no additional properties to copy
        }

        internal override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            base.FillFrom(other, inCSpace);
            // DateOnly has no additional properties to fill
        }

        internal override void MakeCompatibleWith(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            DebugCheck.NotNull(other);
            base.MakeCompatibleWith(other, inCSpace);
            // DateOnly has no additional compatibility requirements
        }

        internal override bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
        {
            return base.IsCompatible(other, inCSpace, out errorMessage);
        }
    }
}