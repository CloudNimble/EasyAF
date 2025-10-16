// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions.Internal
{
    // Because we are using the source file for ExpressionVistor from System.Core
    // we need to add code to facilitate some external calls that ExpressionVisitor makes.
    // The classes in this file do that.

    internal static class ReadOnlyCollectionExtensions
    {
        internal static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> sequence)
        {
            if (sequence is null)
            {
                return DefaultReadOnlyCollection<T>.Empty;
            }
            var col = sequence as ReadOnlyCollection<T>;
            if (col is not null)
            {
                return col;
            }
            return new ReadOnlyCollection<T>(sequence.ToArray());
        }

        private static class DefaultReadOnlyCollection<T>
        {
            private static ReadOnlyCollection<T> _defaultCollection;

            internal static ReadOnlyCollection<T> Empty
            {
                get
                {
                    _defaultCollection ??= new ReadOnlyCollection<T>([]);
                    return _defaultCollection;
                }
            }
        }
    }
}
