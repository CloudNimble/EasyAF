// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using ModelConfig = System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration;


namespace System.Data.Entity.ModelConfiguration.Conventions
{
    internal interface IConfigurationConvention<TMemberInfo> : IConvention
        where TMemberInfo : MemberInfo
    {
        void Apply(TMemberInfo memberInfo, ModelConfig modelConfiguration);
    }
}
