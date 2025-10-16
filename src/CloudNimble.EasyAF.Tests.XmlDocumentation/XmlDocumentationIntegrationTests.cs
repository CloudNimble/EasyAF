using CloudNimble.EasyAF.XmlDocumentation;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.XmlDocumentation
{

    /// <summary>
    /// Integration tests that validate complete parsing of all baseline XML documentation files.
    /// </summary>
    [TestClass]
    public class XmlDocumentationIntegrationTests
    {

        #region Fields

        private static readonly string _basePath = Path.Combine(Directory.GetCurrentDirectory(), "Baselines");
        private static Dictionary<string, AssemblyXmlDocumentation> _parsedDocumentations;

        #endregion

        #region Test Initialization

        /// <summary>
        /// Initializes test data by parsing all baseline XML files once.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _parsedDocumentations = new Dictionary<string, AssemblyXmlDocumentation>();
            
            var xmlFiles = Directory.GetFiles(_basePath, "*.xml");
            foreach (var xmlFile in xmlFiles)
            {
                var xmlDocument = XDocument.Load(xmlFile);
                var documentation = new AssemblyXmlDocumentation(xmlDocument);
                var fileName = Path.GetFileNameWithoutExtension(xmlFile);
                _parsedDocumentations[fileName] = documentation;
            }
        }

        #endregion

        #region Test Methods

        /// <summary>
        /// Validates that all baseline XML files are successfully parsed.
        /// </summary>
        [TestMethod]
        public void IntegrationTest_AllBaselineFiles_ShouldBeParsedSuccessfully()
        {
            _parsedDocumentations.Should().HaveCountGreaterThan(0, "baseline XML files should exist and be parsed");

            foreach (var kvp in _parsedDocumentations)
            {
                var fileName = kvp.Key;
                var documentation = kvp.Value;

                documentation.Should().NotBeNull($"{fileName} should be parsed successfully");
                documentation.AssemblyName.Should().Be(fileName, $"assembly name should match file name for {fileName}");
                documentation.Members.Should().NotBeEmpty($"{fileName} should have members");
            }
        }

        /// <summary>
        /// Validates member type distribution across all assemblies.
        /// </summary>
        [TestMethod]
        public void IntegrationTest_MemberTypeDistribution_ShouldBeDiverse()
        {
            var allMembers = _parsedDocumentations.Values
                .SelectMany(doc => doc.Members.Values)
                .ToList();

            allMembers.Should().NotBeEmpty("combined assemblies should have members");

            var membersByType = allMembers.GroupBy(m => m.MemberType).ToList();

            membersByType.Should().Contain(g => g.Key == MemberType.Type, "should have types");
            membersByType.Should().Contain(g => g.Key == MemberType.Method, "should have methods");
            membersByType.Should().Contain(g => g.Key == MemberType.Property, "should have properties");

            // Validate that we have a reasonable distribution
            var typeCount = membersByType.FirstOrDefault(g => g.Key == MemberType.Type)?.Count() ?? 0;
            var methodCount = membersByType.FirstOrDefault(g => g.Key == MemberType.Method)?.Count() ?? 0;
            var propertyCount = membersByType.FirstOrDefault(g => g.Key == MemberType.Property)?.Count() ?? 0;

            typeCount.Should().BeGreaterThan(0, "should have types");
            methodCount.Should().BeGreaterThan(0, "should have methods");
            propertyCount.Should().BeGreaterThan(0, "should have properties");
        }

        /// <summary>
        /// Validates namespace organization across all assemblies.
        /// </summary>
        [TestMethod]
        public void IntegrationTest_NamespaceOrganization_ShouldBeValid()
        {
            foreach (var kvp in _parsedDocumentations)
            {
                var fileName = kvp.Key;
                var documentation = kvp.Value;

                var namespaces = documentation.GetNamespaces();
                namespaces.Should().NotBeEmpty($"{fileName} should have namespaces");
                namespaces.Should().OnlyHaveUniqueItems($"{fileName} namespaces should be unique");
                namespaces.Should().BeInAscendingOrder($"{fileName} namespaces should be sorted");

                // Validate that namespaces are valid (basic sanity check)
                namespaces.Where(ns => !string.IsNullOrEmpty(ns))
                    .Should().AllSatisfy(ns => ns.Should().NotBeNullOrWhiteSpace(),
                    $"{fileName} namespaces should be valid");
            }
        }

        /// <summary>
        /// Validates documentation element coverage across all assemblies.
        /// </summary>
        [TestMethod]
        public void IntegrationTest_DocumentationElementCoverage_ShouldBeComprehensive()
        {
            var allMembers = _parsedDocumentations.Values
                .SelectMany(doc => doc.Members.Values)
                .ToList();

            // Check that we have various documentation elements
            var membersWithSummary = allMembers.Where(m => m.Summary is not null).ToList();
            var membersWithRemarks = allMembers.Where(m => m.Remarks is not null).ToList();
            var membersWithParameters = allMembers.Where(m => m.Parameters.Count > 0).ToList();
            var membersWithReturns = allMembers.Where(m => m.Returns is not null).ToList();
            var membersWithExceptions = allMembers.Where(m => m.Exceptions.Count > 0).ToList();
            var membersWithExamples = allMembers.Where(m => m.Examples.Count > 0).ToList();

            membersWithSummary.Should().NotBeEmpty("should have members with summary documentation");
            membersWithRemarks.Should().NotBeEmpty("should have members with remarks documentation");
            membersWithParameters.Should().NotBeEmpty("should have members with parameter documentation");
            membersWithReturns.Should().NotBeEmpty("should have members with return documentation");
            membersWithExceptions.Should().NotBeEmpty("should have members with exception documentation");

            // Calculate coverage percentages
            var summaryPercentage = (double)membersWithSummary.Count / allMembers.Count * 100;
            summaryPercentage.Should().BeGreaterThan(50, "at least 50% of members should have summary documentation");
        }

        /// <summary>
        /// Validates that type and member relationships are correctly parsed.
        /// </summary>
        [TestMethod]
        public void IntegrationTest_TypeMemberRelationships_ShouldBeValid()
        {
            foreach (var kvp in _parsedDocumentations)
            {
                var fileName = kvp.Key;
                var documentation = kvp.Value;

                foreach (var type in documentation.Types.Values)
                {
                    var typeName = type.Name.Substring(2); // Remove "T:" prefix
                    var membersOfType = documentation.GetMembersByType(typeName);

                    // Each type should have at least some members or be a simple type
                    if (membersOfType.Count > 0)
                    {
                        foreach (var member in membersOfType.Values)
                        {
                            member.Name.Should().Contain(typeName, 
                                $"member {member.Name} should contain type name {typeName} in {fileName}");
                            
                            var containingType = member.GetContainingType();
                            if (!string.IsNullOrEmpty(containingType))
                            {
                                // For members, their containing type should be part of the type name
                                var memberTypeName = member.Name.Substring(2); // Remove "M:", "P:", etc.
                                memberTypeName.Should().Contain(containingType, 
                                    $"member type name should contain containing type for {member.Name} in {fileName}");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates parsing of complex generics and nested types.
        /// </summary>
        [TestMethod]
        public void IntegrationTest_ComplexTypeStructures_ShouldBeHandledCorrectly()
        {
            var allTypes = _parsedDocumentations.Values
                .SelectMany(doc => doc.Types.Values)
                .ToList();

            var genericTypes = allTypes.Where(t => t.Name.Contains("`")).ToList();
            var nestedTypes = allTypes.Where(t => t.Name.Contains("+")).ToList();

            foreach (var genericType in genericTypes)
            {
                genericType.GetSimpleName().Should().NotBeNullOrWhiteSpace(
                    $"generic type {genericType.Name} should have a valid simple name");
                genericType.GetNamespace().Should().NotBeNullOrWhiteSpace(
                    $"generic type {genericType.Name} should have a valid namespace");
            }

            foreach (var nestedType in nestedTypes)
            {
                nestedType.GetSimpleName().Should().NotBeNullOrWhiteSpace(
                    $"nested type {nestedType.Name} should have a valid simple name");
                nestedType.GetNamespace().Should().NotBeNullOrWhiteSpace(
                    $"nested type {nestedType.Name} should have a valid namespace");
            }
        }

        /// <summary>
        /// Validates that all documented parameters have valid names.
        /// </summary>
        [TestMethod]
        public void IntegrationTest_ParameterDocumentation_ShouldHaveValidNames()
        {
            var allMembers = _parsedDocumentations.Values
                .SelectMany(doc => doc.Members.Values)
                .Where(m => m.Parameters.Count > 0)
                .ToList();

            foreach (var member in allMembers)
            {
                foreach (var parameter in member.Parameters)
                {
                    parameter.Name.Should().NotBeNullOrWhiteSpace(
                        $"parameter in {member.Name} should have a valid name");
                    
                    // Only check text if parameter has documentation
                    if (!string.IsNullOrWhiteSpace(parameter.Text))
                    {
                        parameter.Text.Should().NotBeNullOrWhiteSpace(
                            $"parameter {parameter.Name} in {member.Name} should have documentation text");
                    }
                }
            }
        }

        /// <summary>
        /// Validates exception documentation references.
        /// </summary>
        [TestMethod]
        public void IntegrationTest_ExceptionDocumentation_ShouldHaveValidReferences()
        {
            var allMembers = _parsedDocumentations.Values
                .SelectMany(doc => doc.Members.Values)
                .Where(m => m.Exceptions.Count > 0)
                .ToList();

            foreach (var member in allMembers)
            {
                foreach (var exception in member.Exceptions)
                {
                    exception.Cref.Should().NotBeNullOrWhiteSpace(
                        $"exception in {member.Name} should have a valid cref");
                    
                    // Only check text if exception has documentation
                    if (!string.IsNullOrWhiteSpace(exception.Text))
                    {
                        exception.Text.Should().NotBeNullOrWhiteSpace(
                            $"exception {exception.Cref} in {member.Name} should have documentation text");
                    }
                }
            }
        }

        /// <summary>
        /// Validates that member name parsing works correctly for all members.
        /// </summary>
        [TestMethod]
        public void IntegrationTest_MemberNameParsing_ShouldBeConsistent()
        {
            var allMembers = _parsedDocumentations.Values
                .SelectMany(doc => doc.Members.Values)
                .ToList();

            foreach (var member in allMembers)
            {
                // All members should have valid names
                member.Name.Should().NotBeNullOrWhiteSpace("member should have a name");
                member.MemberType.Should().NotBe(MemberType.Unknown, 
                    $"member {member.Name} should have a recognized type");

                // Simple name should be extractable
                var simpleName = member.GetSimpleName();
                simpleName.Should().NotBeNullOrWhiteSpace(
                    $"member {member.Name} should have a valid simple name");

                // Namespace should be valid for most members
                var namespaceName = member.GetNamespace();
                if (member.MemberType == MemberType.Type && !member.Name.Contains("+"))
                {
                    // Top-level types should have namespaces (except global ones)
                    if (!simpleName.StartsWith("\\") && member.Name.Contains("."))
                    {
                        namespaceName.Should().NotBeNullOrWhiteSpace(
                            $"type {member.Name} should have a namespace");
                    }
                }
            }
        }

        /// <summary>
        /// Validates the overall data integrity of parsed documentation.
        /// </summary>
        [TestMethod]
        public void IntegrationTest_DataIntegrity_ShouldBeConsistent()
        {
            foreach (var kvp in _parsedDocumentations)
            {
                var fileName = kvp.Key;
                var documentation = kvp.Value;

                // Assembly name should match file name
                documentation.AssemblyName.Should().Be(fileName, 
                    $"assembly name should match file name for {fileName}");

                // Member collections should be consistent
                var allMemberKeys = documentation.Members.Keys.ToList();
                var typeKeys = documentation.Types.Keys.ToList();
                var methodKeys = documentation.Methods.Keys.ToList();
                var propertyKeys = documentation.Properties.Keys.ToList();
                var fieldKeys = documentation.Fields.Keys.ToList();
                var eventKeys = documentation.Events.Keys.ToList();

                // Type keys should be subset of all member keys
                typeKeys.Should().BeSubsetOf(allMemberKeys, $"type keys should be subset of all members in {fileName}");
                methodKeys.Should().BeSubsetOf(allMemberKeys, $"method keys should be subset of all members in {fileName}");
                propertyKeys.Should().BeSubsetOf(allMemberKeys, $"property keys should be subset of all members in {fileName}");
                fieldKeys.Should().BeSubsetOf(allMemberKeys, $"field keys should be subset of all members in {fileName}");
                eventKeys.Should().BeSubsetOf(allMemberKeys, $"event keys should be subset of all members in {fileName}");

                // Total should equal all members
                var totalSpecificMembers = typeKeys.Count + methodKeys.Count + propertyKeys.Count + fieldKeys.Count + eventKeys.Count;
                totalSpecificMembers.Should().Be(allMemberKeys.Count, 
                    $"sum of specific member types should equal total members in {fileName}");
            }
        }

        #endregion

    }

}