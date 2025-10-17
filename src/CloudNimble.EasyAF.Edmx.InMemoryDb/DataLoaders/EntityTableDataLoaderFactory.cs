// --------------------------------------------------------------------------------------------
// <copyright file="EntityTableDataLoaderFactory.cs" company="Effort Team">
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


namespace CloudNimble.EasyAF.Edmx.InMemoryDb.DataLoaders
{
    using CloudNimble.EasyAF.Edmx.InMemory;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.Extensions;
    using System;
    using System.Data.Entity.Core.EntityClient;
    using System.Reflection;

    /// <summary>
    ///     Represents a table data loader factory that creates 
    ///     <see cref="EntityTableDataLoader" /> instances for tables.
    /// </summary>
    public class EntityTableDataLoaderFactory : ITableDataLoaderFactory
    {
        private Func<EntityConnection> connectionFactory;
        private EntityConnection connection;



        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityTableDataLoaderFactory" /> 
        ///     class.
        /// </summary>
        /// <param name="connectionFactory">
        ///     A delegate that creates a connection towards the appropriate database.
        /// </param>
        public EntityTableDataLoaderFactory(Func<EntityConnection> connectionFactory)
        {
            var entityConnectionString_Fields = connectionFactory.Target.GetType().GetField("entityConnectionString", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            var entityConnectionString = entityConnectionString_Fields.GetValue(connectionFactory.Target);

            if (entityConnectionString is null || entityConnectionString.Equals(""))
            {
                this.connectionFactory = () => EntityFrameworkEffortManager.CreateFactoryContext(null).Database.GetEntityConnection();
            }
            else
            {
                this.connectionFactory = connectionFactory;
            }
        }

        /// <summary>
        ///     Ensures that a connection is established towards to appropriate database and 
        ///     creates a <see cref="EntityTableDataLoader" /> instance for the specified
        ///     table.
        /// </summary>
        /// <param name="table"> 
        ///     The metadata of the table.
        /// </param>
        /// <returns>
        ///     The <see cref="EntityTableDataLoader" /> instance for the table.
        /// </returns>
        public ITableDataLoader CreateTableDataLoader(TableDescription table)
        {
            if (connection is null)
            {
                connection = connectionFactory.Invoke();
                connection.Open();
            }

            return new EntityTableDataLoader(connection, table);
        }

        /// <summary>
        ///     Disposes the connection established towards the database.
        /// </summary>
        public void Dispose()
        {
            if (connection is not null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
}
