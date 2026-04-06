// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    /// <summary>
    /// Convention to configure the primary key(s) of the dependent entity type as foreign key(s) in a one:one relationship.
    /// </summary>
    public class OneToOneConstraintIntroductionConvention : IConceptualModelConvention<AssociationType>
    {
        /// <inheritdoc />
        public virtual void Apply(AssociationType item, DbModel model)
        {
            Check.NotNull(item, "item");
            Check.NotNull(model, "model");

            if (item.IsOneToOne()
                && !item.IsSelfReferencing()
                && !item.IsIndependent()
                && (item.Constraint is null))
            {
                var sourceKeys = item.SourceEnd.GetEntityType().KeyProperties();
                var targetKeys = item.TargetEnd.GetEntityType().KeyProperties();

                if ((sourceKeys.Count() == targetKeys.Count())
                    && sourceKeys.Select(p => p.UnderlyingPrimitiveType)
                                 .SequenceEqual(targetKeys.Select(p => p.UnderlyingPrimitiveType)))
                {
                    AssociationEndMember _;
                    if (item.TryGuessPrincipalAndDependentEnds(out _, out var dependentEnd)
                        || item.IsPrincipalConfigured())
                    {
                        dependentEnd = dependentEnd ?? item.TargetEnd;

                        var principalEnd = item.GetOtherEnd(dependentEnd);

                        var constraint
                            = new ReferentialConstraint(
                                principalEnd,
                                dependentEnd,
                                principalEnd.GetEntityType().KeyProperties().ToList(),
                                dependentEnd.GetEntityType().KeyProperties().ToList());

                        item.Constraint = constraint;
                    }
                }
            }
        }
    }
}
