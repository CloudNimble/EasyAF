// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration.Types;
using System.Data.Entity.Utilities;
using System.Linq;
using ModelConfig = System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration;

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    internal abstract class TypeConventionBase : IConfigurationConvention<Type, EntityTypeConfiguration>,
                                                   IConfigurationConvention<Type, ComplexTypeConfiguration>,
                                                   IConfigurationConvention<Type>
    {
        private readonly IEnumerable<Func<Type, bool>> _predicates;

        protected TypeConventionBase(IEnumerable<Func<Type, bool>> predicates)
        {
            DebugCheck.NotNull(predicates);

            _predicates = predicates;
        }

        internal IEnumerable<Func<Type, bool>> Predicates
        {
            get { return _predicates; }
        }

        public void Apply(Type memberInfo, ModelConfig modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(modelConfiguration);

            if (_predicates.All(p => p(memberInfo)))
            {
                ApplyCore(memberInfo, modelConfiguration);
            }
        }

        protected abstract void ApplyCore(Type memberInfo, ModelConfig modelConfiguration);

        public void Apply(Type memberInfo, Func<EntityTypeConfiguration> configuration, ModelConfig modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            if (_predicates.All(p => p(memberInfo)))
            {
                ApplyCore(memberInfo, configuration, modelConfiguration);
            }
        }

        protected abstract void ApplyCore(
            Type memberInfo, Func<EntityTypeConfiguration> configuration, ModelConfig modelConfiguration);

        public void Apply(Type memberInfo, Func<ComplexTypeConfiguration> configuration, ModelConfig modelConfiguration)
        {
            DebugCheck.NotNull(memberInfo);
            DebugCheck.NotNull(configuration);
            DebugCheck.NotNull(modelConfiguration);

            if (_predicates.All(p => p(memberInfo)))
            {
                ApplyCore(memberInfo, configuration, modelConfiguration);
            }
        }

        protected abstract void ApplyCore(
            Type memberInfo, Func<ComplexTypeConfiguration> configuration, ModelConfig modelConfiguration);
    }
}
