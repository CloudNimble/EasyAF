// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Pluralization;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    /// <summary>
    /// Convention to set the entity set name to be a pluralized version of the entity type name.
    /// </summary>
    public class PluralizingEntitySetNameConvention : IConceptualModelConvention<EntitySet>
    {
        private static readonly IPluralizationService _pluralizationService
            = DbConfiguration.DependencyResolver.GetService<IPluralizationService>();

        /// <inheritdoc />
        public virtual void Apply(EntitySet item, DbModel model)
        {
            Check.NotNull(item, "item");
            Check.NotNull(model, "model");

            if (item.GetConfiguration() is null)
            {
                item.Name
                    = model.ConceptualModel.GetEntitySets()
                           .Except([item])
                           .UniquifyName(_pluralizationService.Pluralize(item.Name));
            }
        }
    }
}
