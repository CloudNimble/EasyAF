using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbManagement;
using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.DbManagement.Engine;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Provider
{

    /// <summary>
    /// 
    /// </summary>
    public class EffortRestorePoint
    {
#if !EFOLD

        /// <summary>
        /// 
        /// </summary>
        public EffortConnection EffortConnection { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<EffortRestorePointEntry> Entities { get; set; } = new List<EffortRestorePointEntry>();

        /// <summary>
        /// 
        /// </summary>
        private List<EffortRestorePointEntry> OrderedEntities { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effortConnection"></param>
        public EffortRestorePoint(EffortConnection effortConnection)
        {
            EffortConnection = effortConnection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="entities"></param>
        public void AddToIndex(object table, List<object> entities)
        {
            foreach (var entity in entities)
            {
                var itemDeserialized = ShallowCopy(entity);
                Entities.Add(new EffortRestorePointEntry(table, itemDeserialized));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dbContainer"></param>
        public void Restore(DbContext context, object dbContainer)
        {
            var oldIdentityFieldDictionary = new Dictionary<IExtendedTable, bool>();
            try
            {
                if (dbContainer is not null)
                {
                    foreach (IExtendedTable table in ((DbContainer)dbContainer).Internal.Tables.GetAllTables())
                    {
                        oldIdentityFieldDictionary.Add(table, table.IsIdentityFieldEnabled);
                        table.IsIdentityFieldEnabled = false;
                    }
                }

                if (OrderedEntities is null)
                {
                    CreateOrderedEntities();
                    EffortConnection.ClearTables(context);
                }

                foreach (var entity in OrderedEntities)
                {
                    var table = entity.Table;
                    var methods = table.GetType().GetMethods().Where(x => x.Name == "Insert").ToList()[0];
                    var obj = ShallowCopy(entity.Entity);
                    methods.Invoke(table, new[] { obj });
                }
            }
            finally
            {
                foreach (var dicTable in oldIdentityFieldDictionary)
                {
                    dicTable.Key.IsIdentityFieldEnabled = dicTable.Value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void CreateOrderedEntities()
        {
            var orderedEntities = new List<EffortRestorePointEntry>();
            var listToTryInsert = new List<EffortRestorePointEntry>();

            // Initialize list to insert
            foreach (var entity in Entities)
            {
                listToTryInsert.Add(new EffortRestorePointEntry(entity.Table, entity.Entity));
            }

            Exception lastError = null;

            while (listToTryInsert.Count > 0)
            {
                var remainingList = new List<EffortRestorePointEntry>();

                foreach (var itemToTry in listToTryInsert)
                    try
                    {
                        var method = itemToTry.Table.GetType().GetMethods().Where(x => x.Name == "Insert").ToList()[0];
                        var obj = ShallowCopy(itemToTry.Entity);

                        method.Invoke(itemToTry.Table, new[] { obj });
                        orderedEntities.Add(itemToTry);
                    }
                    catch (Exception ex)
                    {
                        lastError = ex;
                        remainingList.Add(itemToTry);
                    }

                if (listToTryInsert.Count == remainingList.Count && lastError is not null)
                {
                    throw new Exception("Oops! There is an error when trying to generate the insert order.", lastError);
                }

                listToTryInsert = remainingList;
            }

            OrderedEntities = orderedEntities;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T ShallowCopy<T>(T @this)
        {
            var method = @this.GetType().GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)method.Invoke(@this, null);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public class EffortRestorePointEntry
        {

            /// <summary>
            /// 
            /// </summary>
            /// <param name="table"></param>
            /// <param name="entity"></param>
            public EffortRestorePointEntry(object table, object entity)
            {
                Table = table;
                Entity = entity;
            }

            /// <summary>
            /// 
            /// </summary>
            public object Table { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public object Entity { get; set; }

        }
#endif
    }
}
