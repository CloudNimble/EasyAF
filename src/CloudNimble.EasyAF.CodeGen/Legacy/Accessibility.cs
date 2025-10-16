using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace CloudNimble.EasyAF.CodeGen.Legacy
{
    /// <summary>
    /// Responsible for encapsulating the retrieval and translation of the CodeGeneration
    /// annotations in the EntityFramework Metadata to a form that is useful in code generation.
    /// </summary>
    public static class Accessibility
    {
        private const string GETTER_ACCESS = "http://schemas.microsoft.com/ado/2006/04/codegeneration:GetterAccess";
        private const string SETTER_ACCESS = "http://schemas.microsoft.com/ado/2006/04/codegeneration:SetterAccess";
        private const string TYPE_ACCESS = "http://schemas.microsoft.com/ado/2006/04/codegeneration:TypeAccess";
        private const string METHOD_ACCESS = "http://schemas.microsoft.com/ado/2006/04/codegeneration:MethodAccess";
        private const string ACCESS_PROTECTED = "Protected";
        private const string ACCESS_INTERNAL = "Internal";
        private const string ACCESS_PRIVATE = "Private";
        private static readonly Dictionary<string, int> AccessibilityRankIdLookup = new Dictionary<string, int>
        {
            { "private", 1 },
            { "internal", 2 },
            { "protected", 3 },
            { "public", 4 },
        };

        /// <summary>
        /// Gets the accessibility that should be applied to a type being generated from the provided GlobalItem.
        ///
        /// defaults to public if no annotation is found.
        /// </summary>
        public static string ForType(GlobalItem item)
        {
            if (item is null)
            {
                return null;
            }

            return GetAccessibility(item, TYPE_ACCESS);
        }

        /// <summary>
        /// Gets the accessibility that should be applied at the property level for a property being
        /// generated from the provided EdmMember.
        ///
        /// defaults to public if no annotation is found.
        /// </summary>
        public static string ForProperty(EdmMember member)
        {
            if (member is null)
            {
                return null;
            }

            CalculatePropertyAccessibility(member, out var propertyAccess, out var getterAccess, out var setterAccess);
            return propertyAccess;
        }

        /// <summary>
        /// Gets the accessibility that should be applied to a NavigationProperty being generated
        ///
        /// Looks up the accessibility for the property (as defined by its getterAccess and setterAccess)
        /// and compares to the accessibility for the target type (as defined by its typeAccess)
        /// and takes the minimum
        /// </summary>
        public static string ForNavigationProperty(NavigationProperty navProp)
        {
            if (navProp is null)
            {
                return null;
            }

            CalculatePropertyAccessibility(navProp, out var propertyAccess, out var getterAccess, out var setterAccess);

            var endType = navProp.ToEndMember.GetEntityType();
            var typeAccess = ForType(endType);

            var propertyRank = AccessibilityRankIdLookup[propertyAccess];
            var typeRank = AccessibilityRankIdLookup[typeAccess];
            var navPropRank = Math.Min(propertyRank, typeRank);
            return AccessibilityRankIdLookup.Single(r => r.Value == navPropRank).Key;
        }

        /// <summary>
        /// Gets the accessibility that should be applied at the property level for a Read-Only property being
        /// generated from the provided EdmMember.
        ///
        /// defaults to public if no annotation is found.
        /// </summary>
        public static string ForReadOnlyProperty(EdmMember member)
        {
            if (member is null)
            {
                return null;
            }

            return GetAccessibility(member, GETTER_ACCESS);
        }

        /// <summary>
        /// Gets the accessibility that should be applied at the property level for a property being
        /// generated from the provided EntitySet.
        ///
        /// defaults to public if no annotation is found.
        /// </summary>
        public static string ForReadOnlyProperty(EntitySet set)
        {
            if (set is null)
            {
                return null;
            }

            return GetAccessibility(set, GETTER_ACCESS);
        }

        /// <summary>
        /// Gets the accessibility that should be applied at the property level for a Write-Only property being
        /// generated from the provided EdmMember.
        ///
        /// defaults to public if no annotation is found.
        /// </summary>
        public static string ForWriteOnlyProperty(EdmMember member)
        {
            if (member is null)
            {
                return null;
            }

            return GetAccessibility(member, SETTER_ACCESS);
        }


        /// <summary>
        /// Gets the accessibility that should be applied at the get level for a property being
        /// generated from the provided EdmMember.
        ///
        /// defaults to empty if no annotation is found or the accessibility is the same as the property level.
        /// </summary>
        public static string ForGetter(EdmMember member)
        {
            if (member is null)
            {
                return null;
            }

            CalculatePropertyAccessibility(member, out _, out var getterAccess, out _);
            return getterAccess;
        }

        /// <summary>
        /// Gets the accessibility that should be applied at the set level for a property being
        /// generated from the provided EdmMember.
        ///
        /// defaults to empty if no annotation is found or the accessibility is the same as the property level.
        /// </summary>
        public static string ForSetter(EdmMember member)
        {
            if (member is null)
            {
                return null;
            }

            CalculatePropertyAccessibility(member, out _, out _, out var setterAccess);
            return setterAccess;
        }

        /// <summary>
        /// Gets the accessibility that should be applied to a method being generated from the provided EdmFunction.
        ///
        /// defaults to public if no annotation is found.
        /// </summary>
        public static string ForMethod(EdmFunction function)
        {
            if (function is null)
            {
                return null;
            }

            return GetAccessibility(function, METHOD_ACCESS);
        }

        private static void CalculatePropertyAccessibility(MetadataItem item,
            out string propertyAccessibility,
            out string getterAccessibility,
            out string setterAccessibility)
        {
            getterAccessibility = GetAccessibility(item, GETTER_ACCESS);
            var getterRank = AccessibilityRankIdLookup[getterAccessibility];

            setterAccessibility = GetAccessibility(item, SETTER_ACCESS);
            var setterRank = AccessibilityRankIdLookup[setterAccessibility];

            var propertyRank = Math.Max(getterRank, setterRank);
            if (setterRank == propertyRank)
            {
                setterAccessibility = string.Empty;
            }

            if (getterRank == propertyRank)
            {
                getterAccessibility = string.Empty;
            }

            propertyAccessibility = AccessibilityRankIdLookup.Where(v => v.Value == propertyRank).Select(v => v.Key).Single();
        }

        private static string GetAccessibility(MetadataItem item, string name)
        {
            if (MetadataTools.TryGetStringMetadataPropertySetting(item, name, out var accessibility))
            {
                return TranslateUserAccessibilityToCSharpAccessibility(accessibility);
            }

            return "public";
        }

        private static string TranslateUserAccessibilityToCSharpAccessibility(string userAccessibility)
        {
            return userAccessibility switch
            {
                ACCESS_PROTECTED => "protected",
                ACCESS_INTERNAL => "internal",
                ACCESS_PRIVATE => "private",
                _ => "public",
            };
        }
    }
}
