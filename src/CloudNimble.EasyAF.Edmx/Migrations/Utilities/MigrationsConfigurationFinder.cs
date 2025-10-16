// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
#if !NET40
using System.Runtime.ExceptionServices;

namespace System.Data.Entity.Migrations.Utilities
{
#endif

    internal class MigrationsConfigurationFinder
    {
        private readonly TypeFinder _typeFinder;

        // <summary>
        // For testing.
        // </summary>
        public MigrationsConfigurationFinder()
        {
        }

        public MigrationsConfigurationFinder(TypeFinder typeFinder)
        {
            DebugCheck.NotNull(typeFinder);

            _typeFinder = typeFinder;
        }

        public virtual DbMigrationsConfiguration FindMigrationsConfiguration(
            Type contextType,
            string configurationTypeName,
            Func<string, Exception> noType = null,
            Func<string, IEnumerable<Type>, Exception> multipleTypes = null,
            Func<string, string, Exception> noTypeWithName = null,
            Func<string, string, Exception> multipleTypesWithName = null)
        {
            var configurationType = _typeFinder.FindType(
                contextType is null ? typeof(DbMigrationsConfiguration) : typeof(DbMigrationsConfiguration<>).MakeGenericType(contextType),
                configurationTypeName,
                types => types
                             .Where(
                                 t => t.GetPublicConstructor() is not null
                                      && !t.IsAbstract()
                                      && !t.IsGenericType())
                             .ToList(),
                noType,
                multipleTypes,
                noTypeWithName,
                multipleTypesWithName);

            try
            {
                return configurationType is null
                           ? null
                           : configurationType.CreateInstance<DbMigrationsConfiguration>(
                               Strings.CreateInstance_BadMigrationsConfigurationType,
                               s => new MigrationsException(s));
            }
            catch (TargetInvocationException ex)
            {
                Debug.Assert(ex.InnerException is not null);
#if !NET40
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
#endif
                throw ex.InnerException;
            }
        }
    }
}
