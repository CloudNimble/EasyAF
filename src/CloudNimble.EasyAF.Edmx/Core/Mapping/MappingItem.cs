// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Diagnostics;

namespace System.Data.Entity.Core.Mapping
{
    /// <summary>
    /// Base class for items in the mapping space (DataSpace.CSSpace)
    /// </summary>
    public abstract class MappingItem
    {
        private bool _readOnly;
        private readonly List<MetadataProperty> _annotations = [];

        internal bool IsReadOnly
        {
            get { return _readOnly; }
        }

        internal IList<MetadataProperty> Annotations
        {
            get { return _annotations; }
        }

        internal virtual void SetReadOnly()
        {
            _annotations.TrimExcess();

            _readOnly = true;
        }

        internal void ThrowIfReadOnly()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(Strings.OperationOnReadOnlyItem);
            }
        }

        internal static void SetReadOnly(MappingItem item)
        {
            if (item is not null)
            {
                item.SetReadOnly();
            }
        }

        internal static void SetReadOnly(IEnumerable<MappingItem> items)
        {
            if (items is null)
            {
                return;
            }

            foreach (var item in items)
            {
                SetReadOnly(item);
            }
        }
    }
}
