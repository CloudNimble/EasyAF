#if !EFOLD
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using NMemory;

namespace CloudNimble.EasyAF.Edmx.InMemory
{
    /// <summary>Manager for entity framework efforts.</summary>
    public class EntityFrameworkEffortManager
    {
        /// <summary>Full pathname of the custom manifest file.</summary>
        public static string CustomManifestPath = null;

        /// <summary>The context factory.</summary>
        public static Func<DbContext, DbContext> ContextFactory;

        /// <summary>
        /// Gets or sets a value indicating if a default value should be used for a not nullable column
        /// with a null value.
        /// </summary>
        /// <value>
        /// A value indicating if a default value should be used for a not nullable column with a null
        /// value.
        /// </value>
	    public static bool UseDefaultForNotNullable
        {
            get { return NMemoryManager.UseDefaultForNotNullable; }
            set { NMemoryManager.UseDefaultForNotNullable = value; }
        }


        internal static DbContext CreateFactoryContext(DbContext context)
        {
            if (ContextFactory is not null)
            {
                return ContextFactory(context);
            }

            if (context is not null)
            {
                var type = context.GetType();

                var emptyConstructor = type.GetConstructor([]);

                if (emptyConstructor is not null)
                {
                    return (DbContext)emptyConstructor.Invoke([]);
                }
            }

            throw new Exception("The specified code require a ContextFactory to work. Example: EntityFrmeworkEffortManager.ContextFactory = (currentContext) => new EntitiesContext()");
        }
    }
}
#endif
