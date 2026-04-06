// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    // <summary>
    // Searches types (usually obtained from an assembly) for different kinds of <see cref="DbConfiguration" />.
    // </summary>
    internal class DbConfigurationFinder
    {
        public virtual Type TryFindConfigurationType(Type contextType, IEnumerable<Type> typesToSearch = null)
        {
            DebugCheck.NotNull(contextType);

            return TryFindConfigurationType(contextType.Assembly(), contextType, typesToSearch);
        }

        public virtual Type TryFindConfigurationType(
            Assembly assemblyHint, 
            Type contextTypeHint, 
            IEnumerable<Type> typesToSearch = null)
        {
            DebugCheck.NotNull(assemblyHint);

            if (contextTypeHint is not null)
            {
                var typeFromAttribute = contextTypeHint.GetCustomAttributes<DbConfigurationTypeAttribute>(inherit: true)
                    .Select(a => a.ConfigurationType)
                    .FirstOrDefault();

                if (typeFromAttribute is not null)
                {
                    if (!typeof(DbConfiguration).IsAssignableFrom(typeFromAttribute))
                    {
                        throw new InvalidOperationException(
                            Strings.CreateInstance_BadDbConfigurationType(typeFromAttribute.ToString(), typeof(DbConfiguration).ToString()));
                    }
                    return typeFromAttribute;
                }
            }

            var configurations = (typesToSearch ?? assemblyHint.GetAccessibleTypes())
                .Where(
                    t => t.IsSubclassOf(typeof(DbConfiguration))
                         && !t.IsAbstract()
                         && !t.IsGenericType())
                .ToList();

            if (configurations.Count > 1)
            {
                throw new InvalidOperationException(
                    Strings.MultipleConfigsInAssembly(configurations.First().Assembly(), typeof(DbConfiguration).Name));
            }

            return configurations.FirstOrDefault();
        }

        public virtual Type TryFindContextType(
            Assembly assemblyHint,
            Type contextTypeHint,
            IEnumerable<Type> typesToSearch = null)
        {
            if (contextTypeHint is not null)
            {
                return contextTypeHint;
            }

            // If no context type is known then try to find a single DbContext in the given assembly that
            // is attributed with the DbConfigurationTypeAttribute. This is a heuristic for tooling such
            // that if tooling only knows the assembly, but the assembly has a DbContext type in it, and
            // that DbContext type is attributed, then tooling will use the configuration specified in that
            // attribute.
            var contextTypes = (typesToSearch ?? assemblyHint.GetAccessibleTypes())
                .Where(
                    t => t.IsSubclassOf(typeof(DbContext))
                         && !t.IsAbstract()
                         && !t.IsGenericType()
                         && t.GetCustomAttributes<DbConfigurationTypeAttribute>(inherit: true).Any())
                .ToList();

            return contextTypes.Count == 1 ? contextTypes[0] : null;
        }
    }
}
