using CloudNimble.EasyAF.XmlDocumentation;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.XmlDocumentation
{

    /// <summary>
    /// Comprehensive validation tests for all baseline XML documentation files.
    /// </summary>
    /// <remarks>
    /// These tests dynamically discover and validate all XML files in the Baselines folder,
    /// ensuring that every aspect of the real build-generated documentation is correctly
    /// parsed by the AssemblyXmlDocumentation class.
    /// </remarks>
    [TestClass]
    public class BaselineValidationTests
    {

        #region Fields

        private static readonly string _basePath = Path.Combine(Directory.GetCurrentDirectory(), "Baselines");

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets all XML files from the Baselines directory for data-driven tests.
        /// </summary>
        /// <returns>Array of object arrays containing file paths for MSTest TestMethod.</returns>
        public static IEnumerable<object[]> GetBaselineXmlFiles()
        {
            if (!Directory.Exists(_basePath))
            {
                yield return new object[] { "NoBaselinesFolder" };
                yield break;
            }

            var xmlFiles = Directory.GetFiles(_basePath, "*.xml");
            if (!xmlFiles.Any())
            {
                yield return new object[] { "NoXmlFiles" };
                yield break;
            }

            foreach (var xmlFile in xmlFiles)
            {
                yield return new object[] { xmlFile };
            }
        }

        /// <summary>
        /// Gets a display name for the dynamic data test that shows only the filename.
        /// </summary>
        /// <param name="methodInfo">The test method info.</param>
        /// <param name="data">The test data.</param>
        /// <returns>A display name showing just the filename.</returns>
        public static string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            if (data?[0] is string filePath)
            {
                var fileName = Path.GetFileName(filePath);
                return fileName;
            }
            return "Unknown";
        }

        #endregion

        #region Test Methods

        /// <summary>
        /// Comprehensive validation of all baseline XML files.
        /// </summary>
        /// <param name="xmlFilePath">Path to the XML file to validate.</param>
        [TestMethod]
        [DynamicData(nameof(GetBaselineXmlFiles), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetDisplayName))]
        public void BaselineValidation_ComprehensiveValidation_ShouldSucceed(string xmlFilePath)
        {
            // Handle special cases for missing directories or files
            if (xmlFilePath is "NoBaselinesFolder" or "NoXmlFiles")
            {
                Assert.Inconclusive($"Baseline validation skipped: {xmlFilePath}");
                return;
            }

            xmlFilePath.Should().NotBeNullOrWhiteSpace();
            File.Exists(xmlFilePath).Should().BeTrue($"XML file should exist: {xmlFilePath}");

            // Load with XDocument (our implementation)
            var xDocument = XDocument.Load(xmlFilePath);
            var documentation = new AssemblyXmlDocumentation(xDocument);

            // Load with XmlDocument (validation)
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlFilePath);

            var fileName = Path.GetFileNameWithoutExtension(xmlFilePath);

            // Run all validation checks
            ValidateBasicStructure(documentation, fileName);
            ValidateAllMembers(documentation, xmlDocument, fileName);
            ValidateSummaryElements(documentation, xmlDocument, fileName);
            ValidateParameterElements(documentation, xmlDocument, fileName);
            ValidateReturnsElements(documentation, xmlDocument, fileName);
            ValidateExceptionElements(documentation, xmlDocument, fileName);
            ValidateTypeParameterElements(documentation, xmlDocument, fileName);
            ValidateMemberTypes(documentation, xmlDocument, fileName);
            ValidateNamespaces(documentation, fileName);
            ValidateMembersByType(documentation, xmlDocument, fileName);
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates basic structure and assembly name.
        /// </summary>
        private static void ValidateBasicStructure(AssemblyXmlDocumentation documentation, string fileName)
        {
            documentation.Should().NotBeNull();
            documentation.AssemblyName.Should().NotBeNullOrWhiteSpace();
            documentation.AssemblyName.Should().Be(fileName, "assembly name should match file name");
        }

        /// <summary>
        /// Validates that all members from the raw XML are correctly parsed.
        /// </summary>
        private static void ValidateAllMembers(AssemblyXmlDocumentation documentation, XmlDocument xmlDocument, string fileName)
        {
            var memberNodes = xmlDocument.SelectNodes("//members/member");
            if (memberNodes is null || memberNodes.Count == 0)
            {
                documentation.Members.Should().BeEmpty("no members in XML means no parsed members");
                return;
            }

            documentation.Members.Should().HaveCount(memberNodes.Count, 
                $"parsed member count should match XML member count in {fileName}");

            foreach (XmlNode memberNode in memberNodes)
            {
                var memberName = memberNode.Attributes?["name"]?.Value;
                memberName.Should().NotBeNullOrWhiteSpace("member should have name attribute");

                documentation.Members.Should().ContainKey(memberName, 
                    $"member {memberName} should be parsed from {fileName}");

                var parsedMember = documentation.Members[memberName];
                parsedMember.Name.Should().Be(memberName, "parsed member name should match XML");
            }
        }

        /// <summary>
        /// Validates that all summary elements are correctly parsed.
        /// </summary>
        private static void ValidateSummaryElements(AssemblyXmlDocumentation documentation, XmlDocument xmlDocument, string fileName)
        {
            var membersWithSummary = xmlDocument.SelectNodes("//members/member[summary]");
            if (membersWithSummary is null || membersWithSummary.Count == 0)
            {
                return;
            }

            foreach (XmlNode memberNode in membersWithSummary)
            {
                var memberName = memberNode.Attributes?["name"]?.Value;
                var summaryNode = memberNode.SelectSingleNode("summary");
                
                if (summaryNode is not null)
                {
                    var parsedMember = documentation.Members[memberName];
                    parsedMember.Summary.Should().NotBeNull($"member {memberName} should have parsed summary");
                    
                    var summaryText = summaryNode.InnerText?.Trim();
                    if (!string.IsNullOrWhiteSpace(summaryText))
                    {
                        parsedMember.Summary.Text.Should().NotBeNullOrWhiteSpace(
                            $"member {memberName} summary should have text content");
                    }
                }
            }
        }

        /// <summary>
        /// Validates that all parameter elements are correctly parsed.
        /// </summary>
        private static void ValidateParameterElements(AssemblyXmlDocumentation documentation, XmlDocument xmlDocument, string fileName)
        {
            var membersWithParams = xmlDocument.SelectNodes("//members/member[param]");
            if (membersWithParams is null || membersWithParams.Count == 0)
            {
                return;
            }

            foreach (XmlNode memberNode in membersWithParams)
            {
                var memberName = memberNode.Attributes?["name"]?.Value;
                var paramNodes = memberNode.SelectNodes("param");

                if (paramNodes is not null && paramNodes.Count > 0)
                {
                    var parsedMember = documentation.Members[memberName];
                    parsedMember.Parameters.Should().HaveCount(paramNodes.Count,
                        $"member {memberName} should have {paramNodes.Count} parsed parameters");

                    foreach (XmlNode paramNode in paramNodes)
                    {
                        var paramName = paramNode.Attributes?["name"]?.Value;
                        paramName.Should().NotBeNullOrWhiteSpace("param should have name attribute");

                        var parsedParam = parsedMember.Parameters.FirstOrDefault(p => p.Name == paramName);
                        parsedParam.Should().NotBeNull($"parameter {paramName} should be parsed for member {memberName}");
                    }
                }
            }
        }

        /// <summary>
        /// Validates that all returns elements are correctly parsed.
        /// </summary>
        private static void ValidateReturnsElements(AssemblyXmlDocumentation documentation, XmlDocument xmlDocument, string fileName)
        {
            var membersWithReturns = xmlDocument.SelectNodes("//members/member[returns]");
            if (membersWithReturns is null || membersWithReturns.Count == 0)
            {
                return;
            }

            foreach (XmlNode memberNode in membersWithReturns)
            {
                var memberName = memberNode.Attributes?["name"]?.Value;
                var returnsNode = memberNode.SelectSingleNode("returns");

                if (returnsNode is not null)
                {
                    var parsedMember = documentation.Members[memberName];
                    parsedMember.Returns.Should().NotBeNull($"member {memberName} should have parsed returns element");
                }
            }
        }

        /// <summary>
        /// Validates that all exception elements are correctly parsed.
        /// </summary>
        private static void ValidateExceptionElements(AssemblyXmlDocumentation documentation, XmlDocument xmlDocument, string fileName)
        {
            var membersWithExceptions = xmlDocument.SelectNodes("//members/member[exception]");
            if (membersWithExceptions is null || membersWithExceptions.Count == 0)
            {
                return;
            }

            foreach (XmlNode memberNode in membersWithExceptions)
            {
                var memberName = memberNode.Attributes?["name"]?.Value;
                var exceptionNodes = memberNode.SelectNodes("exception");

                if (exceptionNodes is not null && exceptionNodes.Count > 0)
                {
                    var parsedMember = documentation.Members[memberName];
                    parsedMember.Exceptions.Should().HaveCount(exceptionNodes.Count,
                        $"member {memberName} should have {exceptionNodes.Count} parsed exceptions");

                    foreach (XmlNode exceptionNode in exceptionNodes)
                    {
                        var cref = exceptionNode.Attributes?["cref"]?.Value;
                        cref.Should().NotBeNullOrWhiteSpace("exception should have cref attribute");

                        var parsedException = parsedMember.Exceptions.FirstOrDefault(e => e.Cref == cref);
                        parsedException.Should().NotBeNull($"exception {cref} should be parsed for member {memberName}");
                    }
                }
            }
        }

        /// <summary>
        /// Validates that all typeparam elements are correctly parsed.
        /// </summary>
        private static void ValidateTypeParameterElements(AssemblyXmlDocumentation documentation, XmlDocument xmlDocument, string fileName)
        {
            var membersWithTypeParams = xmlDocument.SelectNodes("//members/member[typeparam]");
            if (membersWithTypeParams is null || membersWithTypeParams.Count == 0)
            {
                return;
            }

            foreach (XmlNode memberNode in membersWithTypeParams)
            {
                var memberName = memberNode.Attributes?["name"]?.Value;
                var typeParamNodes = memberNode.SelectNodes("typeparam");

                if (typeParamNodes is not null && typeParamNodes.Count > 0)
                {
                    var parsedMember = documentation.Members[memberName];
                    parsedMember.TypeParameters.Should().HaveCount(typeParamNodes.Count,
                        $"member {memberName} should have {typeParamNodes.Count} parsed type parameters");

                    foreach (XmlNode typeParamNode in typeParamNodes)
                    {
                        var paramName = typeParamNode.Attributes?["name"]?.Value;
                        paramName.Should().NotBeNullOrWhiteSpace("typeparam should have name attribute");

                        var parsedTypeParam = parsedMember.TypeParameters.FirstOrDefault(tp => tp.Name == paramName);
                        parsedTypeParam.Should().NotBeNull($"type parameter {paramName} should be parsed for member {memberName}");
                    }
                }
            }
        }

        /// <summary>
        /// Validates that member types are correctly determined from XML member names.
        /// </summary>
        private static void ValidateMemberTypes(AssemblyXmlDocumentation documentation, XmlDocument xmlDocument, string fileName)
        {
            var memberNodes = xmlDocument.SelectNodes("//members/member");
            if (memberNodes is null || memberNodes.Count == 0)
            {
                return;
            }

            foreach (XmlNode memberNode in memberNodes)
            {
                var memberName = memberNode.Attributes?["name"]?.Value;
                if (string.IsNullOrWhiteSpace(memberName) || memberName.Length < 2)
                {
                    continue;
                }

                var parsedMember = documentation.Members[memberName];
                var expectedType = memberName[0] switch
                {
                    'T' => MemberType.Type,
                    'M' => MemberType.Method,
                    'P' => MemberType.Property,
                    'F' => MemberType.Field,
                    'E' => MemberType.Event,
                    'N' => MemberType.Namespace,
                    _ => MemberType.Unknown
                };

                parsedMember.MemberType.Should().Be(expectedType,
                    $"member {memberName} should have correct type based on prefix");
            }
        }

        /// <summary>
        /// Validates that GetNamespaces() returns all unique namespaces from the documentation.
        /// </summary>
        private static void ValidateNamespaces(AssemblyXmlDocumentation documentation, string fileName)
        {
            var namespaces = documentation.GetNamespaces();
            
            namespaces.Should().OnlyHaveUniqueItems("namespaces should not have duplicates");
            namespaces.Should().BeInAscendingOrder("namespaces should be sorted alphabetically");
            
            namespaces.Where(ns => !string.IsNullOrEmpty(ns))
                .Should().AllSatisfy(ns => ns.Should().NotBeNullOrWhiteSpace("namespace should be valid"));
        }

        /// <summary>
        /// Validates that GetMembersByType() correctly filters members.
        /// </summary>
        private static void ValidateMembersByType(AssemblyXmlDocumentation documentation, XmlDocument xmlDocument, string fileName)
        {
            var typeNodes = xmlDocument.SelectNodes("//members/member[starts-with(@name, 'T:')]");
            if (typeNodes is null || typeNodes.Count == 0)
            {
                return;
            }

            foreach (XmlNode typeNode in typeNodes)
            {
                var typeName = typeNode.Attributes?["name"]?.Value?.Substring(2); // Remove "T:" prefix
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    continue;
                }

                var membersByType = documentation.GetMembersByType(typeName);
                
                if (membersByType.Count > 0)
                {
                    membersByType.Values.Should().AllSatisfy(member => 
                        member.Name.Should().Contain(typeName, $"member of type {typeName} should contain type name"));
                }
            }
        }

        #endregion

    }

}