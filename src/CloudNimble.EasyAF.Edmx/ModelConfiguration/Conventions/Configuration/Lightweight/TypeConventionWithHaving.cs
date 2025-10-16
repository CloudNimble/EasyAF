// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.Utilities;
using ModelConfig = System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration;

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    internal class TypeConventionWithHaving<T> : TypeConventionWithHavingBase<T>
        where T : class
    {
        private readonly Action<ConventionTypeConfiguration, T> _entityConfigurationAction;

        public TypeConventionWithHaving(
            IEnumerable<Func<Type, bool>> predicates,
            Func<Type, T> capturingPredicate,
            Action<ConventionTypeConfiguration, T> entityConfigurationAction)
            : base(predicates, capturingPredicate)
        {
            DebugCheck.NotNull(predicates);
            DebugCheck.NotNull(capturingPredicate);
            DebugCheck.NotNull(entityConfigurationAction);

            _entityConfigurationAction = entityConfigurationAction;
        }

        internal Action<ConventionTypeConfiguration, T> EntityConfigurationAction
        {
            get { return _entityConfigurationAction; }
        }

        protected override void InvokeAction(
            Type memberInfo, ModelConfig modelConfiguration, T value)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(modelConfiguration);
            DebugCheck.NotNull(value);

            _entityConfigurationAction(new ConventionTypeConfiguration(memberInfo, modelConfiguration), value);
        }

        protected override void InvokeAction(
            Type memberInfo, Func<EntityTypeConfiguration> configuration, ModelConfig modelConfiguration, T value)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);
            DebugCheck.NotNull(value);

            _entityConfigurationAction(new ConventionTypeConfiguration(memberInfo, configuration, modelConfiguration), value);
        }

        protected override void InvokeAction(
            Type memberInfo, Func<ComplexTypeConfiguration> configuration, ModelConfig modelConfiguration, T value)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(value);

            _entityConfigurationAction(new ConventionTypeConfiguration(memberInfo, configuration, modelConfiguration), value);
        }
    }
}
