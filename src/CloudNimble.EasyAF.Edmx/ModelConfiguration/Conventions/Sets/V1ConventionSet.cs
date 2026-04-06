// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Entity.ModelConfiguration.Conventions.Sets
{
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal static class V1ConventionSet
    {
        private static readonly ConventionSet _conventions
            = new(
                configurationConventions:
                    Enumerable.Reverse(new IConvention[]
                        {
                            // Type Configuration
                            new NotMappedTypeAttributeConvention(),
                            new ComplexTypeAttributeConvention(),
                            new TableAttributeConvention(),
                            // Property Configuration
                            new NotMappedPropertyAttributeConvention(),
                            new KeyAttributeConvention(),
                            new RequiredPrimitivePropertyAttributeConvention(),
                            new RequiredNavigationPropertyAttributeConvention(),
                            new TimestampAttributeConvention(),
                            new ConcurrencyCheckAttributeConvention(),
                            new DatabaseGeneratedAttributeConvention(),
                            new MaxLengthAttributeConvention(),
                            new StringLengthAttributeConvention(),
                            new ColumnAttributeConvention(),
                            new IndexAttributeConvention(),
                            new InversePropertyAttributeConvention(),
                            new ForeignKeyPrimitivePropertyAttributeConvention(),
                        }),
                entityModelConventions:
                    [
                            new IdKeyDiscoveryConvention(),
                            new AssociationInverseDiscoveryConvention(),
                            new ForeignKeyNavigationPropertyAttributeConvention(),
                            new OneToOneConstraintIntroductionConvention(),
                            new NavigationPropertyNameForeignKeyDiscoveryConvention(),
                            new PrimaryKeyNameForeignKeyDiscoveryConvention(),
                            new TypeNameForeignKeyDiscoveryConvention(),
                            new ForeignKeyAssociationMultiplicityConvention(),
                            new OneToManyCascadeDeleteConvention(),
                            new ComplexTypeDiscoveryConvention(),
                            new StoreGeneratedIdentityKeyConvention(),
                            new PluralizingEntitySetNameConvention(),
                            new DeclaredPropertyOrderingConvention(),
                            new SqlCePropertyMaxLengthConvention(),
                            new PropertyMaxLengthConvention(),
                            new DecimalPropertyConvention()
                        ],
                dbMappingConventions:
                    [
                            new ManyToManyCascadeDeleteConvention(),
                            new MappingInheritedPropertiesSupportConvention()
                        ],
                dbModelConventions:
                    [
                            new PluralizingTableNameConvention(),
                            new ColumnOrderingConvention(),
                            new ForeignKeyIndexConvention()
                        ]);

        public static ConventionSet Conventions
        {
            get { return _conventions; }
        }
    }
}
