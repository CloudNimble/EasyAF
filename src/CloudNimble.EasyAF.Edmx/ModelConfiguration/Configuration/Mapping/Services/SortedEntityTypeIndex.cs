// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    internal class SortedEntityTypeIndex
    {
        private static readonly EntityType[] _emptyTypes = [];

        private readonly Dictionary<EntitySet, List<EntityType>> _entityTypes;
        // these are sorted where base types come before derived types

        public SortedEntityTypeIndex()
        {
            _entityTypes = [];
        }

        public void Add(EntitySet entitySet, EntityType entityType)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(entityType);

            var i = 0;

            if (!_entityTypes.TryGetValue(entitySet, out var entityTypes))
            {
                entityTypes = [];
                _entityTypes.Add(entitySet, entityTypes);
            }

            for (; i < entityTypes.Count; i++)
            {
                if (entityTypes[i] == entityType)
                {
                    return;
                }
                else if (entityType.IsAncestorOf(entityTypes[i]))
                {
                    break;
                }
            }
            entityTypes.Insert(i, entityType);
        }

        public bool Contains(EntitySet entitySet, EntityType entityType)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(entityType);

            return _entityTypes.TryGetValue(entitySet, out var setTypes) && setTypes.Contains(entityType);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool IsRoot(EntitySet entitySet, EntityType entityType)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(entityType);

            var isRoot = true;
            var entityTypes = _entityTypes[entitySet];

            foreach (var et in entityTypes)
            {
                if (et != entityType
                    &&
                    et.IsAncestorOf(entityType))
                {
                    isRoot = false;
                }
            }

            return isRoot;
        }

        public IEnumerable<EntitySet> GetEntitySets()
        {
            return _entityTypes.Keys;
        }

        public IEnumerable<EntityType> GetEntityTypes(EntitySet entitySet)
        {
            if (_entityTypes.TryGetValue(entitySet, out var entityTypes))
            {
                return entityTypes;
            }
            else
            {
                return _emptyTypes;
            }
        }
    }
}
