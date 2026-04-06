// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    /// <summary>
    /// Convention to process instances of <see cref="ComplexTypeAttribute" /> found on types in the model.
    /// </summary>
    public class ComplexTypeAttributeConvention :
        TypeAttributeConfigurationConvention<ComplexTypeAttribute>
    {
        /// <inheritdoc />
        public override void Apply(ConventionTypeConfiguration configuration, ComplexTypeAttribute attribute)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");

            configuration.IsComplexType();
        }
    }
}
