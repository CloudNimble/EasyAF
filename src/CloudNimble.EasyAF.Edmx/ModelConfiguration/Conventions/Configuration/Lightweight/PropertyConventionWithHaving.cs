// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Utilities;
using System.Reflection;
using ModelConfig = System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration;
using PrimitivePropertyConfiguration = System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration;

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    internal class PropertyConventionWithHaving<T> : PropertyConventionBase
        where T : class
    {
        private readonly Func<PropertyInfo, T> _capturingPredicate;
        private readonly Action<ConventionPrimitivePropertyConfiguration, T> _propertyConfigurationAction;

        public PropertyConventionWithHaving(
            IEnumerable<Func<PropertyInfo, bool>> predicates,
            Func<PropertyInfo, T> capturingPredicate,
            Action<ConventionPrimitivePropertyConfiguration, T> propertyConfigurationAction)
            : base(predicates)
        {
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(capturingPredicate);
            DebugCheck.NotNull(propertyConfigurationAction);

            _capturingPredicate = capturingPredicate;
            _propertyConfigurationAction = propertyConfigurationAction;
        }

        internal Func<PropertyInfo, T> CapturingPredicate
        {
            get { return _capturingPredicate; }
        }

        internal Action<ConventionPrimitivePropertyConfiguration, T> PropertyConfigurationAction
        {
            get { return _propertyConfigurationAction; }
        }

        protected override void ApplyCore(
            PropertyInfo memberInfo, Func<PrimitivePropertyConfiguration> configuration, ModelConfig modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            var value = _capturingPredicate(memberInfo);

            if (value is not null)
            {
                _propertyConfigurationAction(
                    new ConventionPrimitivePropertyConfiguration(memberInfo, configuration),
                    value);
            }
        }
    }
}
