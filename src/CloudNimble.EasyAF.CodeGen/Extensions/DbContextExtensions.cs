using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace CloudNimble.EasyAF.CodeGen.Extensions
{

    /// <summary>
    /// A set of Reflection-based DbContext extensions.
    /// </summary>
    public static class EasyAF_CodeGen_DbContextExtensions
    {

        /// <summary>
        /// Returns a list of all the <see cref="DbSet{TEntity}"/> properties on the <see cref="DbContext"/>.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetDbSets(this DbContext dbContext)
        {
            return dbContext.GetType().GetProperties()
                        .Where(c => c.PropertyType.IsGenericType && c.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));
        }

        /// <summary>
        /// Returns a list of the entity types for all the <see cref="DbSet{TEntity}"/> properties on the <see cref="DbContext"/>.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetDbSetTypes(this DbContext dbContext)
        {
            return dbContext.GetDbSets().Select(c => c.PropertyType.GenericTypeArguments[0]);
        }

    }

}
