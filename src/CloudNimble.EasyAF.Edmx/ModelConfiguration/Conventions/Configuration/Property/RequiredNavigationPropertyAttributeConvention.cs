// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.ModelConfiguration.Configuration.Properties;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;
using ModelConfig = System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration;


namespace System.Data.Entity.ModelConfiguration.Conventions
{
    /// <summary>
    /// Convention to process instances of <see cref="RequiredAttribute" /> found on navigation properties in the model.
    /// </summary>
    public class RequiredNavigationPropertyAttributeConvention : Convention
    {
        private readonly AttributeProvider _attributeProvider = DbConfiguration.DependencyResolver.GetService<AttributeProvider>();

        // Not using the public API to avoid configuring the property as a navigation property if it wasn't one before
        internal override void ApplyPropertyConfiguration(
            PropertyInfo propertyInfo, Func<PropertyConfiguration> propertyConfiguration, ModelConfig modelConfiguration)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(propertyConfiguration);
            DebugCheck.NotNull(modelConfiguration);

            if (propertyInfo.IsValidEdmNavigationProperty()
                && !propertyInfo.PropertyType.IsCollection()
                && _attributeProvider.GetAttributes(propertyInfo).OfType<RequiredAttribute>().Any())
            {
                var navigationPropertyConfiguration = (NavigationPropertyConfiguration)propertyConfiguration();

                navigationPropertyConfiguration.RelationshipMultiplicity ??= (RelationshipMultiplicity.One);
            }
        }
    }
}
