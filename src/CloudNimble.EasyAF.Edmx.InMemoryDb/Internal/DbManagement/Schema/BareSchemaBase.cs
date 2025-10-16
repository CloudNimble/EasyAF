// --------------------------------------------------------------------------------------------
// <copyright file="BareSchemaBase.cs" company="Effort Team">
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
// --------------------------------------------------------------------------------------------

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbManagement.Schema
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class BareSchemaBase : IBareSchema
    {
        private readonly Dictionary<TableName, Type> entityTypes;

        public BareSchemaBase()
        {
            entityTypes = new Dictionary<TableName, Type>();
        }

        public Type GetEntityType(TableName tableName)
        {
            Type result;
            if (!entityTypes.TryGetValue(tableName, out result))
            {
                return null;
            }

            return result;
        }

        public TableName GetTableName(Type entityType)
        {
            foreach (var keypair in entityTypes)
            {
                if (keypair.Value == entityType)
                {
                    return keypair.Key;
                }
            }

            return default;
        }

        public Type[] EntityTypes
        {
            get { return entityTypes.Values.ToArray(); }
        }

        public TableName[] Tables
        {
            get { return entityTypes.Keys.ToArray(); }
        }

        protected void Register(TableName tableName, Type entityType)
        {
            entityTypes.Add(tableName, entityType);
        }
    }
}
