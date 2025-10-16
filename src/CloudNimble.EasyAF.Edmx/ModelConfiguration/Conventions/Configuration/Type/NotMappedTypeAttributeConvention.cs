// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    /// <summary>
    /// Convention to process instances of <see cref="NotMappedAttribute" /> found on types in the model.
    /// </summary>
    public class NotMappedTypeAttributeConvention :
        TypeAttributeConfigurationConvention<NotMappedAttribute>
    {
        /// <inheritdoc />
        public override void Apply(ConventionTypeConfiguration configuration, NotMappedAttribute attribute)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");

            configuration.Ignore();
        }
    }
}
