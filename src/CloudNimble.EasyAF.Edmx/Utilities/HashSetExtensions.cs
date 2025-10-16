// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Data.Entity.Utilities
{
    internal static class HashSetExtensions
    {
        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
        {
            DebugCheck.NotNull(set);
            DebugCheck.NotNull(items);

            foreach (var i in items)
            {
                set.Add(i);
            }
        }
    }
}
