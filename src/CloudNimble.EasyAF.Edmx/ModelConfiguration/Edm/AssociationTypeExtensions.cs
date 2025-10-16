// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Diagnostics;

namespace System.Data.Entity.ModelConfiguration.Edm
{
    internal static class AssociationTypeExtensions
    {
        private const string IsIndependentAnnotation = "IsIndependent";
        private const string IsPrincipalConfiguredAnnotation = "IsPrincipalConfigured";

        public static void MarkIndependent(this AssociationType associationType)
        {
            DebugCheck.NotNull(associationType);

            associationType.GetMetadataProperties().SetAnnotation(IsIndependentAnnotation, true);
        }

        public static bool IsIndependent(this AssociationType associationType)
        {
            DebugCheck.NotNull(associationType);

            var isIndependent
                = associationType.Annotations.GetAnnotation(IsIndependentAnnotation);

            return isIndependent is not null && (bool)isIndependent;
        }

        public static void MarkPrincipalConfigured(this AssociationType associationType)
        {
            DebugCheck.NotNull(associationType);

            associationType.GetMetadataProperties().SetAnnotation(IsPrincipalConfiguredAnnotation, true);
        }

        public static bool IsPrincipalConfigured(this AssociationType associationType)
        {
            DebugCheck.NotNull(associationType);

            var isPrincipalConfigured
                = associationType.Annotations.GetAnnotation(IsPrincipalConfiguredAnnotation);

            return isPrincipalConfigured is not null && (bool)isPrincipalConfigured;
        }

        public static AssociationEndMember GetOtherEnd(
            this AssociationType associationType, AssociationEndMember associationEnd)
        {
            DebugCheck.NotNull(associationType);
            DebugCheck.NotNull(associationEnd);

            return associationEnd == associationType.SourceEnd
                       ? associationType.TargetEnd
                       : associationType.SourceEnd;
        }

        public static object GetConfiguration(this AssociationType associationType)
        {
            DebugCheck.NotNull(associationType);

            return associationType.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this AssociationType associationType, object configuration)
        {
            DebugCheck.NotNull(associationType);

            associationType.GetMetadataProperties().SetConfiguration(configuration);
        }

        public static bool IsRequiredToMany(this AssociationType associationType)
        {
            DebugCheck.NotNull(associationType);
            Debug.Assert(associationType.SourceEnd is not null);
            Debug.Assert(associationType.TargetEnd is not null);

            return associationType.SourceEnd.IsRequired()
                   && associationType.TargetEnd.IsMany();
        }

        public static bool IsRequiredToRequired(this AssociationType associationType)
        {
            DebugCheck.NotNull(associationType);
            Debug.Assert(associationType.SourceEnd is not null);
            Debug.Assert(associationType.TargetEnd is not null);

            return associationType.SourceEnd.IsRequired()
                   && associationType.TargetEnd.IsRequired();
        }

        public static bool IsManyToRequired(this AssociationType associationType)
        {
            DebugCheck.NotNull(associationType);
            Debug.Assert(associationType.SourceEnd is not null);
            Debug.Assert(associationType.TargetEnd is not null);

            return associationType.SourceEnd.IsMany()
                   && associationType.TargetEnd.IsRequired();
        }

        public static bool IsManyToMany(this AssociationType associationType)
        {
            DebugCheck.NotNull(associationType);
            Debug.Assert(associationType.SourceEnd is not null);
            Debug.Assert(associationType.TargetEnd is not null);

            return associationType.SourceEnd.IsMany()
                   && associationType.TargetEnd.IsMany();
        }

        public static bool IsOneToOne(this AssociationType associationType)
        {
            DebugCheck.NotNull(associationType);
            Debug.Assert(associationType.SourceEnd is not null);
            Debug.Assert(associationType.TargetEnd is not null);

            return !associationType.SourceEnd.IsMany()
                   && !associationType.TargetEnd.IsMany();
        }

        public static bool IsSelfReferencing(this AssociationType associationType)
        {
            DebugCheck.NotNull(associationType);

            var sourceEnd = associationType.SourceEnd;
            var targetEnd = associationType.TargetEnd;

            Debug.Assert(sourceEnd is not null);
            Debug.Assert(targetEnd is not null);
            Debug.Assert(sourceEnd.GetEntityType() is not null);
            Debug.Assert(targetEnd.GetEntityType() is not null);

            return sourceEnd.GetEntityType().GetRootType() == targetEnd.GetEntityType().GetRootType();
        }

        public static bool IsRequiredToNonRequired(this AssociationType associationType)
        {
            DebugCheck.NotNull(associationType);
            Debug.Assert(associationType.SourceEnd is not null);
            Debug.Assert(associationType.TargetEnd is not null);

            return (associationType.SourceEnd.IsRequired() && !associationType.TargetEnd.IsRequired())
                   || (associationType.TargetEnd.IsRequired() && !associationType.SourceEnd.IsRequired());
        }

        // <summary>
        // Attempt to determine the principal and dependent ends of this association.
        // The following table illustrates the solution space.
        // Source | Target || Prin  | Dep   |
        // -------|--------||-------|-------|
        // 1      | 1      || -     | -     |
        // 1      | 0..1   || Sr    | Ta    |
        // 1      | *      || Sr    | Ta    |
        // 0..1   | 1      || Ta    | Sr    |
        // 0..1   | 0..1   || -     | -     |
        // 0..1   | *      || Sr    | Ta    |
        // *      | 1      || Ta    | Sr    |
        // *      | 0..1   || Ta    | Sr    |
        // *      | *      || -     | -     |
        // </summary>
        public static bool TryGuessPrincipalAndDependentEnds(
            this AssociationType associationType,
            out AssociationEndMember principalEnd,
            out AssociationEndMember dependentEnd)
        {
            DebugCheck.NotNull(associationType);
            Debug.Assert(associationType.SourceEnd is not null);
            Debug.Assert(associationType.TargetEnd is not null);

            principalEnd = dependentEnd = null;

            var sourceEnd = associationType.SourceEnd;
            var targetEnd = associationType.TargetEnd;

            if (sourceEnd.RelationshipMultiplicity
                != targetEnd.RelationshipMultiplicity)
            {
                principalEnd
                    = (sourceEnd.IsRequired()
                       || (sourceEnd.IsOptional() && targetEnd.IsMany()))
                          ? sourceEnd
                          : targetEnd;

                dependentEnd
                    = (principalEnd == sourceEnd)
                          ? targetEnd
                          : sourceEnd;
            }

            return (principalEnd is not null);
        }
    }
}
