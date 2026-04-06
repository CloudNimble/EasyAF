// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Utilities;
using System.Reflection;
using ModelConfig = System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration;
using PrimitivePropertyConfiguration = System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration;

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    internal class PropertyConvention : PropertyConventionBase
    {
        private readonly Action<ConventionPrimitivePropertyConfiguration> _propertyConfigurationAction;

        public PropertyConvention(
            IEnumerable<Func<PropertyInfo, bool>> predicates,
            Action<ConventionPrimitivePropertyConfiguration> propertyConfigurationAction)
            : base(predicates)
        {
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(propertyConfigurationAction);

            _propertyConfigurationAction = propertyConfigurationAction;
        }

        internal Action<ConventionPrimitivePropertyConfiguration> PropertyConfigurationAction
        {
            get { return _propertyConfigurationAction; }
        }

        protected override void ApplyCore(
            PropertyInfo memberInfo, Func<PrimitivePropertyConfiguration> configuration, ModelConfig modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            _propertyConfigurationAction(new ConventionPrimitivePropertyConfiguration(memberInfo, configuration));
        }
    }
}
