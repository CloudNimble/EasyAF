// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping
{
    internal class ColumnMappingBuilder
    {
        private EdmProperty _columnProperty;
        private readonly IList<EdmProperty> _propertyPath;
        private ScalarPropertyMapping _scalarPropertyMapping;

        public ColumnMappingBuilder(EdmProperty columnProperty, IList<EdmProperty> propertyPath)
        {
            Check.NotNull(columnProperty, "columnProperty");
            Check.NotNull(propertyPath, "propertyPath");

            _columnProperty = columnProperty;
            _propertyPath = propertyPath;
        }

        public IList<EdmProperty> PropertyPath
        {
            get { return _propertyPath; }
        }

        public EdmProperty ColumnProperty
        {
            get { return _columnProperty; }
            internal set
            {
                DebugCheck.NotNull(value);

                _columnProperty = value;

                if (_scalarPropertyMapping is not null)
                {
                    _scalarPropertyMapping.Column = _columnProperty;
                }
            }
        }

        internal void SetTarget(ScalarPropertyMapping scalarPropertyMapping)
        {
            _scalarPropertyMapping = scalarPropertyMapping;
        }
    }
}
