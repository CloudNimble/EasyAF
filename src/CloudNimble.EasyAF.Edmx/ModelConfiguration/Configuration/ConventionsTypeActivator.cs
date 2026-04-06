// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    internal class ConventionsTypeActivator
    {
        public virtual IConvention Activate(Type conventionType)
        {
            DebugCheck.NotNull(conventionType);

            return (IConvention)Activator
                .CreateInstance(conventionType, nonPublic: true);
        }
    }
}
