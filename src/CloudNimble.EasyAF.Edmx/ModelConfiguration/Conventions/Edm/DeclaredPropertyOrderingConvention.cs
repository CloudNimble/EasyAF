// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.ModelConfiguration.Mappers;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    /// <summary>
    /// Convention to move primary key properties to appear first.
    /// </summary>
    public class DeclaredPropertyOrderingConvention : IConceptualModelConvention<EntityType>
    {
        /// <inheritdoc />
        public virtual void Apply(EntityType item, DbModel model)
        {
            Check.NotNull(item, "item");
            Check.NotNull(model, "model");

            if (item.BaseType is null)
            {
                // Performance: avoid converting to .Each<>() Linq expressions in order to avoid closure allocations   
                foreach (var p in item.KeyProperties)
                {
                    item.RemoveMember(p);
                    item.AddKeyMember(p);
                }

                foreach (var p in 
                    new PropertyFilter()
                    .GetProperties(item.GetClrType(), declaredOnly: false, includePrivate: true))
                {
                    var property
                        = item
                            .DeclaredProperties
                            .SingleOrDefault(ep => ep.Name == p.Name);

                    if ((property is not null)
                        && !item.KeyProperties.Contains(property))
                    {
                        item.RemoveMember(property);
                        item.AddMember(property);
                    }
                }
            }
        }
    }
}
