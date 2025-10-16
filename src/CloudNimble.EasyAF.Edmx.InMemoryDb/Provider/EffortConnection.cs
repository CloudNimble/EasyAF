// --------------------------------------------------------------------------------------------
// <copyright file="EffortConnection.cs" company="Effort Team">
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


using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Provider
{
    using CloudNimble.EasyAF.Edmx.InMemoryDb.DataLoaders;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.Caching;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.CommandActions;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbManagement;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbManagement.Schema;
    using NMemory.Tables;
    using System;
    using System.Data;
    using System.Data.Common;

    /// <summary>
    ///     Represents a virtual connection towards an in-memory fake database.
    /// </summary>
    public class EffortConnection : DbConnection
    {
        private string connectionString;

        private string lastContainerId;
        private DbContainerManagerWrapper containerConfiguration;
        private DbContainer container;

        private Guid identifier;
        private ConnectionState state;
        private bool isPrimaryTransient;
        private EffortRestorePoint RestorePoint;

        private int? _connectionTimeout;

        /// <summary>
        /// 
        /// </summary>
        public override int ConnectionTimeout => _connectionTimeout ?? base.ConnectionTimeout;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void SetConnectionTimeout(int value)
        {
            _connectionTimeout = value;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EffortConnection" /> class.
        /// </summary>
        public EffortConnection()
        {
            identifier = Guid.NewGuid();
            state = ConnectionState.Closed;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsCaseSensitive
        {
            get
            {
                if (DbContainer is not null)
                {
                    return DbContainer.IsCaseSensitive;
                }
                else
                {
                    throw new Exception("The connection must be open to gets or sets 'IsCaseSensitive' value. Please open the connection first with 'effortConnection.Open()'");
                }
            }
            set
            {
                if (DbContainer is not null)
                {
                    DbContainer.IsCaseSensitive = value;
                }
                else
                {
                    throw new Exception("The connection must be open to gets or sets 'IsCaseSensitive' value. Please open the connection first with 'effortConnection.Open()'");
                }
            }
        }

#if !EFOLD

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableInfo"></param>
        /// <returns></returns>
        public ITable GetTable(DbTableInfo tableInfo)
        {
            return (ITable)DbContainer.Internal.Tables.FindTable(tableInfo.EntityType);
        }

        /// <summary>
        ///   Get the Effort TableInfo
        /// </summary> 
        public DbTableInfo GetTableInfo(string schema, string name)
        {
            DbTableInfo TableInfo = null;

            if (DbContainer != null)
            {
                var table = DbContainer.GetTable(new TableName(schema, name));

                var _TableInfo = table.GetType().GetProperty("TableInfo",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                if (_TableInfo != null)
                {
                    TableInfo = (DbTableInfo)_TableInfo.GetValue(table, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, null, null);
                }
            }

            return TableInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<ITable> GetAllTables()
        {
            return (List<ITable>)DbContainer.GetAllTables();
        }


        /// <summary>
        ///    Create a restore point of the database
        /// </summary>
        public void CreateRestorePoint()
        {
            RestorePoint = new EffortRestorePoint(this);

            if (DbContainer != null)
            {
                var actionContext = new ActionContext(DbContainer);

                var tables = DbCommandActionHelper.GetAllTables(actionContext.DbContainer).ToList()
                    .Where(x => !x.EntityType.Name.Contains("_____MigrationHistory")).ToList();

                foreach (var table in tables)
                {
                    var index = table.PrimaryKeyIndex;

                    var uniqueDataStructureField = index.GetType().GetField("uniqueDataStructure", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var uniqueDataStructure = uniqueDataStructureField.GetValue(index);

                    var innerField = uniqueDataStructure.GetType().GetField("inner", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var inner = (IDictionary)innerField.GetValue(uniqueDataStructure);

                    var entities = inner.Values;
                    var list = new List<object>();

                    foreach (var entity in entities)
                    {
                        list.Add(entity);
                    }

                    RestorePoint.AddToIndex(table, list);
                }
            }
            else
            {
                throw new Exception("The connection must be open to create a restore point. Please open the connection first with 'effortConnection.Open()'");
            }
        }

        /// <summary>
        ///    Rollback changes to the latest restore point
        /// </summary>
        public void RollbackToRestorePoint()
        {
            RollbackToRestorePoint(null);
        }

        /// <summary>
        ///     Rollback changes to the latest restore point
        /// </summary>
        public void RollbackToRestorePoint(DbContext context)
        {
            if (RestorePoint == null)
            {
                throw new Exception("You must create a restore point first");
            }

            ClearTables(context);

            RestorePoint.Restore(context, DbContainer);
        }

        /// <summary>
        /// Clear all tables from the effort connection. You must use a new context instance to clear all
        /// tracked entities, otherwise, use the ClearTables(DbContext) overload.
        /// </summary>
        public void ClearTables()
        {
            ClearTables(null);
        }

        /// <summary>
        ///     Clear all tables from the effort connection and ChangeTracker entries.
        /// </summary>
        public void ClearTables(DbContext context)
        {
            if (DbContainer != null)
            {
                var actionContext = new ActionContext(DbContainer);

                var tables = DbCommandActionHelper.GetAllTables(actionContext.DbContainer).ToList().Where(x => !x.EntityType.Name.Contains("_____MigrationHistory")).ToList();

                foreach (var table in tables)
                {
                    foreach (var index in table.Indexes)
                    {
                        index.Clear();
                    }

                    var _restoreIdentityFieldMethod = table.GetType().GetMethod("RestoreIdentityField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                    _restoreIdentityFieldMethod?.Invoke(table, new object[0]);
                }

                if (context != null)
                {
                    var changedEntriesCopy = context.ChangeTracker.Entries().ToList();
                    changedEntriesCopy.ForEach(x => x.State = EntityState.Detached);
                }
            }
        }

#endif

        /// <summary>
        ///     Gets or sets the string used to open the connection.
        /// </summary>
        /// <returns>
        ///     The connection string used to establish the initial connection. The exact
        ///     contents of the connection string depend on the specific data source for this
        ///     connection. The default value is an empty string.
        /// </returns>
        public override string ConnectionString
        {
            get
            {
                return connectionString;
            }

            set
            {
                var builder = new EffortConnectionStringBuilder(value);

                // Read the transient information now, because it is removed in the setter
                // This is required because EF clones the connection string and these should
                // not receive the IsTransient flag
                if (builder.IsTransient)
                {
                    isPrimaryTransient = builder.IsTransient;
                }

                // Remove informations that should not inherit
                builder.Normalize();

                connectionString = builder.ConnectionString;
            }
        }

        /// <summary>
        ///     Gets the name of the database server to which to connect.
        /// </summary>
        /// <returns>
        ///     The name of the database server to which to connect. The default value is an
        ///     empty string.
        /// </returns>
        public override string DataSource
        {
            get
            {
                return "in-process";
            }
        }

        /// <summary>
        ///     Gets a string that represents the version of the server to which the object is 
        ///     connected.
        /// </summary>
        /// <returns>
        ///     The version of the database. The format of the string returned depends on the 
        ///     specific type of connection you are using.
        /// </returns>
        public override string ServerVersion
        {
            get
            {
                return typeof(NMemory.Database).Assembly.GetName().Version.ToString();
            }
        }

        /// <summary>
        ///     Gets a string that describes the state of the connection.
        /// </summary>
        /// <returns>
        ///     The state of the connection. The format of the string returned depends on the 
        ///     specific type of connection you are using.
        /// </returns>
        public override ConnectionState State
        {
            get
            {
                return state;
            }
        }

        /// <summary>
        ///     Gets the internal <see cref="DbContainer" /> instance.
        /// </summary>
        /// <value>
        ///     The internal <see cref="DbContainer" /> instance.
        /// </value>
        internal DbContainer DbContainer
        {
            get
            {
                return container;
            }
        }

        /// <summary>
        ///     Gets the <see cref="T:System.Data.Common.DbProviderFactory" /> for this 
        ///     <see cref="T:System.Data.Common.DbConnection" />.
        /// </summary>
        /// <returns> 
        ///     A <see cref="T:System.Data.Common.DbProviderFactory" />.
        /// </returns>
        protected override DbProviderFactory DbProviderFactory
        {
            get
            {
                return EffortProviderFactory.Instance;
            }
        }

        /// <summary>
        ///     Changes the current database for an open connection.
        /// </summary>
        /// <param name="databaseName">
        ///     Specifies the name of the database for the connection to use.
        /// </param>
        public override void ChangeDatabase(string databaseName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Gets the name of the current database after a connection is opened, or the 
        ///     database name specified in the connection string before the connection is 
        ///     opened.
        /// </summary>
        /// <returns>
        ///     The name of the current database or the name of the database to be used after a 
        ///     connection is opened. The default value is an empty string.
        /// </returns>
        public override string Database
        {
            get
            {
                var connectionString =
                    new EffortConnectionStringBuilder(ConnectionString);

                return connectionString.InstanceId;
            }
        }

        /// <summary>
        ///     Gets the configuration object that allows to alter the current configuration
        ///     of the database.
        /// </summary>
        /// <returns>
        ///     The configuration object.
        /// </returns>
        public IDbManager DbManager
        {
            get
            {
                if (State != ConnectionState.Open)
                {
                    throw new InvalidOperationException();
                }

                return containerConfiguration;
            }
        }

        /// <summary>
        ///     Opens a database connection with the settings specified by the 
        ///     <see cref="P:System.Data.Common.DbConnection.ConnectionString" />.
        /// </summary>
        public override void Open()
        {
            var connectionString =
                new EffortConnectionStringBuilder(ConnectionString);

            var instanceId = connectionString.InstanceId;

            if (lastContainerId == instanceId)
            {
                // The id was not changed, so the appropriate container is associated
                ChangeConnectionState(ConnectionState.Open);
                return;
            }

            container =
                DbContainerStore.GetDbContainer(instanceId, CreateDbContainer);

            containerConfiguration = new DbContainerManagerWrapper(container);

            lastContainerId = instanceId;
            ChangeConnectionState(ConnectionState.Open);
        }

        /// <summary>
        ///     Closes the connection to the database. This is the preferred method of closing
        ///     any open connection.
        /// </summary>
        public override void Close()
        {
            ChangeConnectionState(ConnectionState.Closed);
        }

        /// <summary>
        ///     Marks the connection object as transient, so the underlying database instance
        ///     will be disposed when this connection object is disposed or garbage collected.
        /// </summary>
        internal void MarkAsPrimaryTransient()
        {
            isPrimaryTransient = true;
        }

        /// <summary>
        ///     Creates and returns a <see cref="T:System.Data.Common.DbCommand" /> object 
        ///     associated with the current connection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Common.DbCommand" /> object.
        /// </returns>
        protected override DbCommand CreateDbCommand()
        {
            return new EffortCommand() { Connection = this };
        }

        /// <summary>
        ///     Starts a database transaction.
        /// </summary>
        /// <param name="isolationLevel">
        ///     Specifies the isolation level for the transaction.
        /// </param>
        /// <returns>
        ///     An object representing the new transaction.
        /// </returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new EffortTransaction(this, isolationLevel);
        }

        /// <summary>
        ///     Enlists in the specified transaction.
        /// </summary>
        /// <param name="transaction">
        ///     A reference to an existing <see cref="T:System.Transactions.Transaction" /> in
        ///     which to enlist.
        /// </param>
        public override void EnlistTransaction(System.Transactions.Transaction transaction)
        {
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the 
        ///     <see cref="T:System.ComponentModel.Component" /> and optionally releases the 
        ///     managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only 
        ///     unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (isPrimaryTransient)
            {
                UnregisterContainer();
            }

            base.Dispose(disposing);
        }

        internal virtual void UnregisterContainer()
        {
            var builder = new EffortConnectionStringBuilder(ConnectionString);

            DbContainerStore.RemoveDbContainer(builder.InstanceId);
        }

        private void ChangeConnectionState(ConnectionState state)
        {
            var oldState = this.state;

            if (oldState != state)
            {
                this.state = state;

                OnStateChange(
                    new StateChangeEventArgs(oldState, this.state));
            }
        }

        private DbContainer CreateDbContainer()
        {
            var connectionString =
                new EffortConnectionStringBuilder(ConnectionString);

            IDataLoader dataLoader = null;
            var parameters = new DbContainerParameters();
            var dataLoaderType = connectionString.DataLoaderType;

            if (dataLoaderType != null)
            {
                //// TODO: check parameterless constructor

                dataLoader = Activator.CreateInstance(dataLoaderType) as IDataLoader;
                dataLoader.Argument = connectionString.DataLoaderArgument;

                parameters.DataLoader = dataLoader;
            }

            parameters.IsTransient = isPrimaryTransient;

            return new DbContainer(parameters);
        }
    }
}
