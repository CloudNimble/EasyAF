// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    [DebuggerDisplay("{Column.Name}")]
    internal class ColumnMapping
    {
        private readonly EdmProperty _column;
        private readonly List<PropertyMappingSpecification> _propertyMappings;

        public ColumnMapping(EdmProperty column)
        {
            DebugCheck.NotNull(column);
            _column = column;
            _propertyMappings = [];
        }

        public EdmProperty Column
        {
            get { return _column; }
        }

        public IList<PropertyMappingSpecification> PropertyMappings
        {
            get { return _propertyMappings; }
        }

        public void AddMapping(
            EntityType entityType,
            IList<EdmProperty> propertyPath,
            IEnumerable<ConditionPropertyMapping> conditions,
            bool isDefaultDiscriminatorCondition)
        {
            _propertyMappings.Add(
                new PropertyMappingSpecification(
                    entityType, propertyPath, conditions.ToList(), isDefaultDiscriminatorCondition));
        }
    }
}
