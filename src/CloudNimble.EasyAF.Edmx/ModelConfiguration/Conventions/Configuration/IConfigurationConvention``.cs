// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.ModelConfiguration.Configuration;
using System.Reflection;
using ModelConfig = System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration;

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    internal interface IConfigurationConvention<TMemberInfo, TConfiguration> : IConvention
        where TMemberInfo : MemberInfo
        where TConfiguration : ConfigurationBase
    {
        void Apply(TMemberInfo memberInfo, Func<TConfiguration> configuration, ModelConfig modelConfiguration);
    }
}
