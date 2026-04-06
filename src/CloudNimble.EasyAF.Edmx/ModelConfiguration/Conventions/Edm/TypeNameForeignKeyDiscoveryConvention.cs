// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    /// <summary>
    /// Convention to discover foreign key properties whose names are a combination
    /// of the principal type name and the principal type primary key property name(s).
    /// </summary>
    public class TypeNameForeignKeyDiscoveryConvention : ForeignKeyDiscoveryConvention
    {
        /// <inheritdoc/>
        protected override bool MatchDependentKeyProperty(
            AssociationType associationType,
            AssociationEndMember dependentAssociationEnd,
            EdmProperty dependentProperty,
            EntityType principalEntityType,
            EdmProperty principalKeyProperty)
        {
            Check.NotNull(associationType, "associationType");
            Check.NotNull(dependentAssociationEnd, "dependentAssociationEnd");
            Check.NotNull(dependentProperty, "dependentProperty");
            Check.NotNull(principalEntityType, "principalEntityType");
            Check.NotNull(principalKeyProperty, "principalKeyProperty");

            return string.Equals(
                dependentProperty.Name, principalEntityType.Name + principalKeyProperty.Name,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
