// --------------------------------------------------------------------------------------------
// <copyright file="DbTableInfoBuilder.cs" company="Effort Team">
//     Copyright (C) Effort Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// ------------------------------------------------------------------------------------------

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbManagement.Schema
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NMemory.Indexes;

#if EFOLD
    using System.Data.Metadata.Edm;
#else
    using System.Data.Entity.Core.Metadata.Edm;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbManagement.Schema.Configuration;
#endif

    internal class DbTableInfoBuilder
    {
        private IList<IKeyInfo> uniqueKeys;
        private IList<IKeyInfo> otherKeys;
        private IList<object> constraintFactories;

        private Type entityType;
        private ILookup<string, PropertyInfo> members;

        public DbTableInfoBuilder()
        {
            uniqueKeys = new List<IKeyInfo>();
            otherKeys = new List<IKeyInfo>();
            constraintFactories = new List<object>();
        }

        public IKeyInfo PrimaryKey { get; set; }

        public MemberInfo IdentityField { get; set; }

        public TableName Name { get; set; }

        public EntitySet EntitySet { get; set; }

        public Type EntityType
        {
            get
            {
                return entityType;
            }

            set
            {
                entityType = value;

                if (entityType is not null)
                {
                    members = entityType
                        .GetProperties()
                        .ToLookup(p => p.Name, p => p);
                }
                else
                {
                    members = null;
                }
            }
        }

        protected IEnumerable<IKeyInfo> AllKeys
        {
            get
            {
                return AllUniqueKeys.Concat(otherKeys);
            }
        }

        protected IEnumerable<IKeyInfo> AllUniqueKeys
        {
            get
            {
                var result = Enumerable.Empty<IKeyInfo>();

                if (PrimaryKey is not null)
                {
                    result = result.Concat(Enumerable.Repeat(PrimaryKey, 1));
                }

                return result.Concat(uniqueKeys);
            }
        }

        public void AddKey(IKeyInfo key, bool isUnique)
        {
            if (isUnique)
            {
                uniqueKeys.Add(key);
            }
            else
            {
                otherKeys.Add(key);
            }
        }

        public void AddContraintFactory(object constraintFactory)
        {
            constraintFactories.Add(constraintFactory);
        }

        // strictOrder never in true, just for keep info.
        public IKeyInfo FindKey(MemberInfo[] members, bool strictOrder, bool unique)
        {
            if (!strictOrder)
            {
                members = members.OrderBy(m => m.Name).ToArray();
            }

            var keys = unique ? AllUniqueKeys : AllKeys;

            foreach (IKeyInfo key in AllKeys)
            {
                MemberInfo[] keyMembers = key.EntityKeyMembers;

                if (!strictOrder)
                {
                    keyMembers = keyMembers.OrderBy(m => m.Name).ToArray();
                }

                if (members.SequenceEqual(keyMembers))
                {
                    return key;
                }
            }

            return null;
        }

        public PropertyInfo FindMember(EntityPropertyInfo property)
        {
            return FindMember(property.Name);
        }

        public PropertyInfo FindMember(string name)
        {
            if (members is null)
            {
                return null;
            }

            return members[name].FirstOrDefault();
        }

        public DbTableInfo Create()
        {
            return new DbTableInfo(
                entitySet: EntitySet,
                tableName: Name,
                entityType: entityType,
                identityField: IdentityField,
                properties: entityType.GetProperties(),
                constraintFactories: constraintFactories.ToArray(),
                primaryKeyInfo: PrimaryKey,
                uniqueKeys: uniqueKeys.ToArray(),
                foreignKeys: otherKeys.ToArray());
        }
    }
}
