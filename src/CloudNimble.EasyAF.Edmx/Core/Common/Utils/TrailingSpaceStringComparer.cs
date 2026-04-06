// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils
{
    // <summary>
    // Typed version of TrailingSpaceComparer.
    // </summary>
    internal class TrailingSpaceStringComparer : IEqualityComparer<string>
    {
        internal static readonly TrailingSpaceStringComparer Instance = new();

        private TrailingSpaceStringComparer()
        {
        }

        public bool Equals(string x, string y)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(NormalizeString(x), NormalizeString(y));
        }

        public int GetHashCode(string obj)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(NormalizeString(obj));
        }

        internal static string NormalizeString(string value)
        {
            if (null == value
                || !value.EndsWith(" ", StringComparison.Ordinal))
            {
                return value;
            }
            else
            {
                return value.TrimEnd(' ');
            }
        }
    }
}
