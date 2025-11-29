using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using CloudNimble.EasyAF.Core;

namespace CloudNimble.EasyAF.CodeGen.Legacy
{
    /// <summary>
    /// Responsible for making the Entity Framework Metadata more accessible for code generation.
    /// </summary>
    public static class MetadataTools
    {


        /// <summary>
        /// This method returns the underlying CLR type of the o-space type corresponding to the supplied <paramref name="typeUsage"/>
        /// Note that for an enum type this means that the type backing the enum will be returned, not the enum type itself.
        /// </summary>
        public static Type ClrType(TypeUsage typeUsage)
        {
            Ensure.ArgumentNotNull(typeUsage, nameof(typeUsage));

            return UnderlyingClrType(typeUsage.EdmType);
        }

        /// <summary>
        /// Gets the documentation comment for an EDM type.
        /// </summary>
        /// <param name="edmType">The EDM type.</param>
        /// <returns>The documentation comment, preferring LongDescription over Summary.</returns>
        public static string Comment(EdmType edmType)
        {
            Ensure.ArgumentNotNull(edmType, nameof(edmType));
            var doc = edmType.Documentation;
            if (doc is null) return string.Empty;

            // Prefer LongDescription over Summary, but check for empty strings
            // since Documentation properties default to "" not null
            if (!string.IsNullOrEmpty(doc.LongDescription))
                return doc.LongDescription;
            if (!string.IsNullOrEmpty(doc.Summary))
                return doc.Summary;
            return string.Empty;
        }

        /// <summary>
        /// Gets the documentation comment for an EDM property.
        /// </summary>
        /// <param name="edmProperty">The EDM property.</param>
        /// <returns>The documentation comment, preferring LongDescription over Summary.</returns>
        public static string Comment(EdmProperty edmProperty)
        {
            Ensure.ArgumentNotNull(edmProperty, nameof(edmProperty));
            var doc = edmProperty.Documentation;
            if (doc is null) return string.Empty;

            // Prefer LongDescription over Summary, but check for empty strings
            // since Documentation properties default to "" not null
            if (!string.IsNullOrEmpty(doc.LongDescription))
                return doc.LongDescription;
            if (!string.IsNullOrEmpty(doc.Summary))
                return doc.Summary;
            return string.Empty;
        }

        /// <summary>
        /// Gets the documentation comment for a navigation property.
        /// </summary>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <returns>The documentation comment, preferring LongDescription over Summary.</returns>
        public static string Comment(NavigationProperty navigationProperty)
        {
            Ensure.ArgumentNotNull(navigationProperty, nameof(navigationProperty));
            var doc = navigationProperty.Documentation;
            if (doc is null) return string.Empty;

            // Prefer LongDescription over Summary, but check for empty strings
            // since Documentation properties default to "" not null
            if (!string.IsNullOrEmpty(doc.LongDescription))
                return doc.LongDescription;
            if (!string.IsNullOrEmpty(doc.Summary))
                return doc.Summary;
            return string.Empty;
        }

        /// <summary>
        /// Gets the documentation comment for an entity container.
        /// </summary>
        /// <param name="container">The entity container.</param>
        /// <returns>The documentation comment, preferring LongDescription over Summary.</returns>
        public static string Comment(EntityContainer container)
        {
            Ensure.ArgumentNotNull(container, nameof(container));
            var doc = container.Documentation;
            if (doc is null) return string.Empty;

            // Prefer LongDescription over Summary, but check for empty strings
            // since Documentation properties default to "" not null
            if (!string.IsNullOrEmpty(doc.LongDescription))
                return doc.LongDescription;
            if (!string.IsNullOrEmpty(doc.Summary))
                return doc.Summary;
            return string.Empty;
        }

        /// <summary>
        /// Gets the documentation comment for an entity set.
        /// </summary>
        /// <param name="entitySet">The entity set.</param>
        /// <returns>The documentation comment, preferring LongDescription over Summary.</returns>
        public static string Comment(EntitySet entitySet)
        {
            Ensure.ArgumentNotNull(entitySet, nameof(entitySet));
            var doc = entitySet.Documentation;
            if (doc is null) return string.Empty;

            // Prefer LongDescription over Summary, but check for empty strings
            // since Documentation properties default to "" not null
            if (!string.IsNullOrEmpty(doc.LongDescription))
                return doc.LongDescription;
            if (!string.IsNullOrEmpty(doc.Summary))
                return doc.Summary;
            return string.Empty;
        }

        /// <summary>
        /// True if this entity type participates in any relationships where the other end has an OnDelete
        /// cascade delete defined, or if it is the dependent in any identifying relationships
        /// </summary>
        private static bool ContainsCascadeDeleteAssociation(ItemCollection itemCollection, EntityType entity)
        {
            return itemCollection.GetItems<AssociationType>().Where(a =>
                    ((RefType)a.AssociationEndMembers[0].TypeUsage.EdmType).ElementType == entity && IsCascadeDeletePrincipal(a.AssociationEndMembers[1]) ||
                    ((RefType)a.AssociationEndMembers[1].TypeUsage.EdmType).ElementType == entity && IsCascadeDeletePrincipal(a.AssociationEndMembers[0])).Any();
        }

        /// <summary>
        /// Given a property on the principal end of a referential constraint, returns the corresponding property on the dependent end.
        /// Requires: The association has a referential constraint, and the specified principalProperty is one of the properties on the principal end.
        /// </summary>
        public static EdmProperty GetCorrespondingDependentProperty(NavigationProperty navProperty, EdmProperty principalProperty)
        {
            Ensure.ArgumentNotNull(navProperty, nameof(navProperty));
            Ensure.ArgumentNotNull(principalProperty, nameof(principalProperty));

            var fromProperties = GetPrincipalProperties(navProperty);
            var toProperties = GetDependentProperties(navProperty);
            return toProperties[fromProperties.IndexOf(principalProperty)];
        }

        /// <summary>
        /// Given a property on the dependent end of a referential constraint, returns the corresponding property on the principal end.
        /// Requires: The association has a referential constraint, and the specified dependentProperty is one of the properties on the dependent end.
        /// </summary>
        public static EdmProperty GetCorrespondingPrincipalProperty(NavigationProperty navProperty, EdmProperty dependentProperty)
        {
            Ensure.ArgumentNotNull(navProperty, nameof(navProperty));
            Ensure.ArgumentNotNull(dependentProperty, nameof(dependentProperty));

            var fromProperties = GetPrincipalProperties(navProperty);
            var toProperties = GetDependentProperties(navProperty);
            return fromProperties[toProperties.IndexOf(dependentProperty)];
        }

        /// <summary>
        /// Gets the collection of properties that are on the dependent end of a referential constraint for the specified navigation property.
        /// Requires: The association has a referential constraint.
        /// </summary>
        public static ReadOnlyMetadataCollection<EdmProperty> GetDependentProperties(NavigationProperty navProperty)
        {
            Ensure.ArgumentNotNull(navProperty, nameof(navProperty));

            return ((AssociationType)navProperty.RelationshipType).ReferentialConstraints[0].ToProperties;
        }

        /// <summary>
        /// If the passed in TypeUsage represents a collection this method returns final element
        /// type of the collection, otherwise it returns the value passed in.
        /// </summary>
        public static TypeUsage GetElementType(TypeUsage typeUsage)
        {
            if (typeUsage is null) return null;

            return typeUsage.EdmType is CollectionType ? GetElementType(((CollectionType)typeUsage.EdmType).TypeUsage) : typeUsage;
        }

        /// <summary>
        /// Gets the collection of properties that are on the principal end of a referential constraint for the specified navigation property.
        /// Requires: The association has a referential constraint.
        /// </summary>
        public static ReadOnlyMetadataCollection<EdmProperty> GetPrincipalProperties(NavigationProperty navProperty)
        {
            Ensure.ArgumentNotNull(navProperty, nameof(navProperty));

            return ((AssociationType)navProperty.RelationshipType).ReferentialConstraints[0].FromProperties;
        }

        /// <summary>
        /// Returns the subtype of the EntityType in the current itemCollection
        /// </summary>
        public static IEnumerable<EntityType> GetSubtypesOf(EntityType type, ItemCollection itemCollection, bool includeAbstractTypes)
        {
            if (type is null || itemCollection is null) return Enumerable.Empty<EntityType>();

            return itemCollection.GetItems<EntityType>()
                .Where(c => !type.Equals(c) && IsSubtypeOf(c, type) && (includeAbstractTypes || !c.Abstract));
        }

        /// <summary>
        /// Returns the NavigationProperty that is the other end of the same association set if it is
        /// available, otherwise it returns null.
        /// </summary>
        public static NavigationProperty Inverse(NavigationProperty navProperty)
        {
            if (navProperty is null) return null;

            var toEntity = navProperty.ToEndMember.GetEntityType();
            return toEntity.NavigationProperties.SingleOrDefault(n => ReferenceEquals(n.RelationshipType, navProperty.RelationshipType) && !ReferenceEquals(n, navProperty));
        }

        /// <summary>
        /// True if the source end of the specified navigation property is the principal in an identifying relationship.
        /// or if the source end has cascade delete defined.
        /// </summary>
        public static bool IsCascadeDeletePrincipal(NavigationProperty navProperty)
        {
            Ensure.ArgumentNotNull(navProperty, nameof(navProperty));

            return IsCascadeDeletePrincipal((AssociationEndMember)navProperty.FromEndMember);
        }

        /// <summary>
        /// True if the specified association end is the principal in an identifying relationship.
        /// or if the association end has cascade delete defined.
        /// </summary>
        public static bool IsCascadeDeletePrincipal(AssociationEndMember associationEnd)
        {
            Ensure.ArgumentNotNull(associationEnd, nameof(associationEnd));

            return associationEnd.DeleteBehavior == OperationAction.Cascade || IsPrincipalEndOfIdentifyingRelationship(associationEnd);
        }

        /// <summary>
        /// True if the specified association type is an identifying relationship.
        /// In order to be an identifying relationship, the association must have a referential constraint where all of the dependent properties are part of the dependent type's primary key.
        /// </summary>
        public static bool IsIdentifyingRelationship(AssociationType association)
        {
            Ensure.ArgumentNotNull(association, nameof(association));

            return IsPrincipalEndOfIdentifyingRelationship(association.AssociationEndMembers[0]) || IsPrincipalEndOfIdentifyingRelationship(association.AssociationEndMembers[1]);
        }

        /// <summary>
        /// True if the EdmProperty is a key of its DeclaringType, False otherwise.
        /// </summary>
        public static bool IsKey(EdmProperty property)
        {
            if (property is not null && property.DeclaringType.BuiltInTypeKind == BuiltInTypeKind.EntityType)
            {
                return ((EntityType)property.DeclaringType).KeyMembers.Contains(property);
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static bool IsLazyLoadingEnabled(EntityContainer container)
        {
            var lazyLoadingAttributeName = "http://schemas.microsoft.com/ado/2009/02/edm/annotation:LazyLoadingEnabled";
            return !TryGetStringMetadataPropertySetting(container, lazyLoadingAttributeName, out var lazyLoadingAttributeValue)
                || !bool.TryParse(lazyLoadingAttributeValue, out var isLazyLoading)
                || isLazyLoading;
        }

        /// <summary>
        /// True if the EdmProperty TypeUsage is Nullable, False otherwise.
        /// </summary>
        public static bool IsNullable(EdmProperty property)
        {
            return property is not null && IsNullable(property.TypeUsage);
        }

        /// <summary>
        /// True if the TypeUsage is Nullable, False otherwise.
        /// </summary>
        public static bool IsNullable(TypeUsage typeUsage)
        {
            if (typeUsage is not null && typeUsage.Facets.TryGetValue("Nullable", true, out var nullableFacet))
            {
                return (bool)nullableFacet.Value;
            }

            return false;
        }

        /// <summary>
        /// True if the specified association end is the principal end in an identifying relationship.
        /// In order to be an identifying relationship, the association must have a referential constraint where all of the dependent properties are part of the dependent type's primary key.
        /// </summary>
        public static bool IsPrincipalEndOfIdentifyingRelationship(AssociationEndMember associationEnd)
        {
            Ensure.ArgumentNotNull(associationEnd, nameof(associationEnd));

            var refConstraint = ((AssociationType)associationEnd.DeclaringType).ReferentialConstraints.Where(rc => rc.FromRole == associationEnd).SingleOrDefault();
            if (refConstraint is not null)
            {
                var entity = refConstraint.ToRole.GetEntityType();
                return !refConstraint.ToProperties.Where(tp => !entity.KeyMembers.Contains(tp)).Any();
            }
            return false;
        }

        /// <summary>
        /// requires: firstType is not null
        /// effects: if secondType is among the base types of the firstType, return true,
        /// otherwise returns false.
        /// when firstType is same as the secondType, return false.
        /// </summary>
        public static bool IsSubtypeOf(EdmType firstType, EdmType secondType)
        {
            Ensure.ArgumentNotNull(firstType, nameof(firstType));

            if (secondType is null) return false;

            // walk up firstType hierarchy list
            for (var t = firstType.BaseType; t is not null; t = t.BaseType)
            {
                if (t == secondType) return true;
            }
            return false;
        }

        /// <summary>
        /// True if this entity type requires the HandleCascadeDelete method defined and the method has
        /// not been defined on any base type
        /// </summary>
        public static bool NeedsHandleCascadeDeleteMethod(ItemCollection itemCollection, EntityType entityType)
        {
            Ensure.ArgumentNotNull(itemCollection, nameof(itemCollection));
            Ensure.ArgumentNotNull(entityType, nameof(entityType));

            var needsMethod = ContainsCascadeDeleteAssociation(itemCollection, entityType);
            // Check to make sure no base types have already declared this method
            var baseType = entityType.BaseType as EntityType;
            while (needsMethod && baseType is not null)
            {
                needsMethod = !ContainsCascadeDeleteAssociation(itemCollection, baseType);
                baseType = baseType.BaseType as EntityType;
            }
            return needsMethod;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetStringMetadataPropertySetting(MetadataItem item, string propertyName, out string value)
        {
            Ensure.ArgumentNotNull(item, nameof(item));

            value = null;
            var property = item.MetadataProperties.FirstOrDefault(p => p.Name == propertyName);
            if (property is not null)
            {
                value = (string)property.Value;
            }
            return value is not null;
        }

        /// <summary>
        /// This method returns the underlying CLR type given the c-space type.
        /// Note that for an enum type this means that the type backing the enum will be returned, not the enum type itself.
        /// </summary>
        public static Type UnderlyingClrType(EdmType edmType)
        {
            return true switch
            {
                true when edmType is PrimitiveType primitiveType => primitiveType.ClrEquivalentType,
                true when edmType is EnumType enumType => enumType.UnderlyingType.ClrEquivalentType,
                _ => typeof(object),
            };
        }

    }

}
