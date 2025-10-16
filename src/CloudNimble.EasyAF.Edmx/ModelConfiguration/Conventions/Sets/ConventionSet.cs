// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions.Sets
{
    internal class ConventionSet
    {
        public ConventionSet()
        {
            ConfigurationConventions = [];
            ConceptualModelConventions = [];
            ConceptualToStoreMappingConventions = [];
            StoreModelConventions = [];
        }

        public ConventionSet(
            IEnumerable<IConvention> configurationConventions,
            IEnumerable<IConvention> entityModelConventions,
            IEnumerable<IConvention> dbMappingConventions,
            IEnumerable<IConvention> dbModelConventions)
        {
            DebugCheck.NotNull(configurationConventions);
            DebugCheck.NotNull(entityModelConventions);
            DebugCheck.NotNull(dbMappingConventions);
            DebugCheck.NotNull(dbModelConventions);

            ConfigurationConventions = configurationConventions;
            ConceptualModelConventions = entityModelConventions;
            ConceptualToStoreMappingConventions = dbMappingConventions;
            StoreModelConventions = dbModelConventions;
        }

        public IEnumerable<IConvention> ConfigurationConventions { get; private set; }
        public IEnumerable<IConvention> ConceptualModelConventions { get; private set; }
        public IEnumerable<IConvention> ConceptualToStoreMappingConventions { get; private set; }
        public IEnumerable<IConvention> StoreModelConventions { get; private set; }
    }
}
