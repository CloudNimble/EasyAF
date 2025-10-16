// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    // <summary>
    // Used to configure a property with length facets for an entity type or
    // complex type.
    // </summary>
    internal abstract class LengthPropertyConfiguration : PrimitivePropertyConfiguration
    {
        // <summary>
        // Gets or sets a value indicating whether the property is fixed length.
        // </summary>
        public bool? IsFixedLength { get; set; }

        // <summary>
        // Gets or sets the maximum length of the property.
        // </summary>
        public int? MaxLength { get; set; }

        // <summary>
        // Gets or sets a value indicating whether the property allows the maximum
        // length supported by the database provider.
        // </summary>
        public bool? IsMaxLength { get; set; }

        // <summary>
        // Initializes a new instance of the LengthPropertyConfiguration class.
        // </summary>
        protected LengthPropertyConfiguration()
        {
        }

        // <summary>
        // Initializes a new instance of the <see cref="T:System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.LengthPropertyConfiguration" /> 
        // class with the same settings as another configuration.
        // </summary>
        // <param name="source">The configuration to copy settings from.</param>
        protected LengthPropertyConfiguration(LengthPropertyConfiguration source)
            : base(source)
        {
            Check.NotNull(source, "source");

            IsFixedLength = source.IsFixedLength;
            MaxLength = source.MaxLength;
            IsMaxLength = source.IsMaxLength;
        }

        protected override void ConfigureProperty(EdmProperty property)
        {
            base.ConfigureProperty(property);

            if (IsFixedLength is not null)
            {
                property.IsFixedLength = IsFixedLength;
            }

            if (MaxLength is not null)
            {
                property.MaxLength = MaxLength;
            }

            if (IsMaxLength is not null)
            {
                property.IsMaxLength = IsMaxLength.Value;
            }
        }

        internal override void Configure(EdmProperty column, FacetDescription facetDescription)
        {
            base.Configure(column, facetDescription);

            switch (facetDescription.FacetName)
            {
                case XmlConstants.FixedLengthElement:
                    column.IsFixedLength = facetDescription.IsConstant ? null : IsFixedLength ?? column.IsFixedLength;
                    break;
                case XmlConstants.MaxLengthElement:
                    column.MaxLength = facetDescription.IsConstant ? null : MaxLength ?? column.MaxLength;
                    column.IsMaxLength = !facetDescription.IsConstant && (IsMaxLength ?? column.IsMaxLength);
                    break;
            }
        }

        internal override void CopyFrom(PrimitivePropertyConfiguration other)
        {
            base.CopyFrom(other);
            var lenConfigRhs = other as LengthPropertyConfiguration;
            if (lenConfigRhs is not null)
            {
                IsFixedLength = lenConfigRhs.IsFixedLength;
                MaxLength = lenConfigRhs.MaxLength;
                IsMaxLength = lenConfigRhs.IsMaxLength;
            }
        }

        internal override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            base.FillFrom(other, inCSpace);
            var lenConfigRhs = other as LengthPropertyConfiguration;
            if (lenConfigRhs is not null)
            {
                IsFixedLength ??= lenConfigRhs.IsFixedLength;
                MaxLength ??= lenConfigRhs.MaxLength;
                IsMaxLength ??= lenConfigRhs.IsMaxLength;
            }
        }

        internal override void MakeCompatibleWith(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            DebugCheck.NotNull(other);

            base.MakeCompatibleWith(other, inCSpace);

            var lengthPropertyConfiguration = other as LengthPropertyConfiguration;

            if (lengthPropertyConfiguration is null)
            {
                return;
            }
            if (lengthPropertyConfiguration.IsFixedLength is not null)
            {
                IsFixedLength = null;
            }
            if (lengthPropertyConfiguration.MaxLength is not null)
            {
                MaxLength = null;
            }
            if (lengthPropertyConfiguration.IsMaxLength is not null)
            {
                IsMaxLength = null;
            }
        }

        internal override bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
        {
            var lenRhs = other as LengthPropertyConfiguration;

            var baseIsCompatible = base.IsCompatible(other, inCSpace, out errorMessage);
            var isFixedLengthIsCompatible = lenRhs is null
                                            || IsCompatible(c => c.IsFixedLength, lenRhs, ref errorMessage);
            var isMaxLengthIsCompatible = lenRhs is null || IsCompatible(c => c.IsMaxLength, lenRhs, ref errorMessage);
            var maxLengthIsCompatible = lenRhs is null || IsCompatible(c => c.MaxLength, lenRhs, ref errorMessage);

            return baseIsCompatible &&
                   isFixedLengthIsCompatible &&
                   isMaxLengthIsCompatible &&
                   maxLengthIsCompatible;
        }
    }
}
