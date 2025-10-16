// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure
{
    internal class ProviderInvariantName : IProviderInvariantName
    {
        public ProviderInvariantName(string name)
        {
            DebugCheck.NotEmpty(name);

            Name = name;
        }

        public string Name { get; private set; }
    }
}
