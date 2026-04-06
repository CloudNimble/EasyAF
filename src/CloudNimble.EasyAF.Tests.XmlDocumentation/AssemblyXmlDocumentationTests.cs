using CloudNimble.EasyAF.XmlDocumentation;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.XmlDocumentation
{

    /// <summary>
    /// Comprehensive tests for AssemblyXmlDocumentation parsing functionality.
    /// </summary>
    [TestClass]
    public class AssemblyXmlDocumentationTests
    {

        #region Fields

        private static readonly string _basePath = Path.Combine(Directory.GetCurrentDirectory(), "Baselines");

        #endregion

        #region Test Methods

        /// <summary>
        /// Tests that all baseline XML documentation files can be successfully parsed.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_ParseAllBaselineFiles_ShouldSucceed()
        {
            var xmlFiles = Directory.GetFiles(_basePath, "*.xml");
            xmlFiles.Should().NotBeEmpty("baseline XML files should exist");

            foreach (var xmlFile in xmlFiles)
            {
                var xmlDocument = XDocument.Load(xmlFile);
                var documentation = new AssemblyXmlDocumentation(xmlDocument);

                documentation.Should().NotBeNull($"documentation for {Path.GetFileName(xmlFile)} should be parsed");
                documentation.AssemblyName.Should().NotBeNullOrWhiteSpace($"assembly name for {Path.GetFileName(xmlFile)} should be parsed");
                documentation.Members.Should().NotBeEmpty($"members for {Path.GetFileName(xmlFile)} should be parsed");
            }
        }

        /// <summary>
        /// Tests parsing of CloudNimble.EasyAF.Core.xml specifically for comprehensive coverage.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_ParseCoreXml_ShouldHaveCompleteStructure()
        {
            var xmlPath = Path.Combine(_basePath, "CloudNimble.EasyAF.Core.xml");
            var xmlDocument = XDocument.Load(xmlPath);
            var documentation = new AssemblyXmlDocumentation(xmlDocument);

            documentation.AssemblyName.Should().Be("CloudNimble.EasyAF.Core");
            documentation.Members.Should().NotBeEmpty();
            documentation.Types.Should().NotBeEmpty();
            documentation.Methods.Should().NotBeEmpty();
            documentation.Properties.Should().NotBeEmpty();

            var namespaces = documentation.GetNamespaces();
            namespaces.Should().NotBeEmpty();
            namespaces.Should().Contain(ns => ns.StartsWith("CloudNimble.EasyAF.Core"));
        }

        /// <summary>
        /// Tests that member types are correctly identified from member names.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_MemberTypes_ShouldBeCorrectlyIdentified()
        {
            var xmlPath = Path.Combine(_basePath, "CloudNimble.EasyAF.Core.xml");
            var xmlDocument = XDocument.Load(xmlPath);
            var documentation = new AssemblyXmlDocumentation(xmlDocument);

            var typeMembers = documentation.Types.Values;
            typeMembers.Should().NotBeEmpty();
            typeMembers.Should().OnlyContain(m => m.MemberType == MemberType.Type);

            var methodMembers = documentation.Methods.Values;
            methodMembers.Should().NotBeEmpty();
            methodMembers.Should().OnlyContain(m => m.MemberType == MemberType.Method);

            var propertyMembers = documentation.Properties.Values;
            propertyMembers.Should().NotBeEmpty();
            propertyMembers.Should().OnlyContain(m => m.MemberType == MemberType.Property);
        }

        /// <summary>
        /// Tests namespace extraction functionality.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_GetNamespaces_ShouldReturnUniqueOrderedNamespaces()
        {
            var xmlPath = Path.Combine(_basePath, "CloudNimble.EasyAF.Core.xml");
            var xmlDocument = XDocument.Load(xmlPath);
            var documentation = new AssemblyXmlDocumentation(xmlDocument);

            var namespaces = documentation.GetNamespaces();

            namespaces.Should().NotBeEmpty();
            namespaces.Should().OnlyHaveUniqueItems();
            namespaces.Should().BeInAscendingOrder();
            namespaces.Should().Contain(ns => !string.IsNullOrWhiteSpace(ns), "all namespaces should be non-empty strings");
        }

        /// <summary>
        /// Tests filtering types by namespace.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_GetTypesByNamespace_ShouldFilterCorrectly()
        {
            var xmlPath = Path.Combine(_basePath, "CloudNimble.EasyAF.Core.xml");
            var xmlDocument = XDocument.Load(xmlPath);
            var documentation = new AssemblyXmlDocumentation(xmlDocument);

            var namespaces = documentation.GetNamespaces();
            namespaces.Should().NotBeEmpty();

            var testNamespace = namespaces.First();
            var typesInNamespace = documentation.GetTypesByNamespace(testNamespace);

            typesInNamespace.Should().NotBeEmpty();
            typesInNamespace.Values.Should().OnlyContain(type => type.GetNamespace() == testNamespace);
        }

        /// <summary>
        /// Tests filtering members by type.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_GetMembersByType_ShouldFilterCorrectly()
        {
            var xmlPath = Path.Combine(_basePath, "CloudNimble.EasyAF.Core.xml");
            var xmlDocument = XDocument.Load(xmlPath);
            var documentation = new AssemblyXmlDocumentation(xmlDocument);

            var firstType = documentation.Types.Values.First();
            var typeName = firstType.Name.Substring(2); // Remove "T:" prefix
            
            var membersOfType = documentation.GetMembersByType(typeName);

            foreach (var member in membersOfType.Values)
            {
                member.Name.Should().Contain(typeName);
                member.MemberType.Should().NotBe(MemberType.Type);
            }
        }

        /// <summary>
        /// Tests parsing with null or invalid input.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_InvalidInput_ShouldHandleGracefully()
        {
            Action nullAction = () => new AssemblyXmlDocumentation(null);
            nullAction.Should().Throw<ArgumentNullException>();

            var emptyDoc = new XDocument();
            var emptyDocumentation = new AssemblyXmlDocumentation(emptyDoc);
            emptyDocumentation.AssemblyName.Should().BeEmpty();
            emptyDocumentation.Members.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that all expected documentation elements are correctly parsed.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_DocumentationElements_ShouldBeParsedCorrectly()
        {
            var xmlPath = Path.Combine(_basePath, "CloudNimble.EasyAF.Core.xml");
            var xmlDocument = XDocument.Load(xmlPath);
            var documentation = new AssemblyXmlDocumentation(xmlDocument);

            var membersWithSummary = documentation.Members.Values.Where(m => m.Summary is not null).ToList();
            membersWithSummary.Should().NotBeEmpty("some members should have summary documentation");

            var membersWithRemarks = documentation.Members.Values.Where(m => m.Remarks is not null).ToList();
            membersWithRemarks.Should().NotBeEmpty("some members should have remarks documentation");

            var membersWithParameters = documentation.Members.Values.Where(m => m.Parameters.Count > 0).ToList();
            membersWithParameters.Should().NotBeEmpty("some members should have parameter documentation");

            var membersWithExceptions = documentation.Members.Values.Where(m => m.Exceptions.Count > 0).ToList();
            membersWithExceptions.Should().NotBeEmpty("some members should have exception documentation");
        }

        /// <summary>
        /// Tests that complex type names and generics are handled correctly.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_ComplexTypeNames_ShouldBeHandledCorrectly()
        {
            var xmlPath = Path.Combine(_basePath, "CloudNimble.EasyAF.Core.xml");
            var xmlDocument = XDocument.Load(xmlPath);
            var documentation = new AssemblyXmlDocumentation(xmlDocument);

            var genericTypes = documentation.Types.Values.Where(t => t.Name.Contains("`")).ToList();
            if (genericTypes.Count > 0)
            {
                foreach (var genericType in genericTypes)
                {
                    genericType.GetSimpleName().Should().NotBeNullOrWhiteSpace();
                    genericType.GetNamespace().Should().NotBeNullOrWhiteSpace();
                }
            }

            var nestedTypes = documentation.Types.Values.Where(t => t.Name.Contains("+")).ToList();
            if (nestedTypes.Count > 0)
            {
                foreach (var nestedType in nestedTypes)
                {
                    nestedType.GetSimpleName().Should().NotBeNullOrWhiteSpace();
                    nestedType.GetNamespace().Should().NotBeNullOrWhiteSpace();
                }
            }
        }

        /// <summary>
        /// Tests parsing of all XML documentation files for complete code coverage.
        /// </summary>
        [TestMethod]
        [DataRow("CloudNimble.EasyAF.Analyzers.EF6.xml")]
        [DataRow("CloudNimble.EasyAF.CodeGen.xml")]
        [DataRow("CloudNimble.EasyAF.Core.xml")]
        [DataRow("CloudNimble.EasyAF.Edmx.InMemoryDb.xml")]
        [DataRow("CloudNimble.EasyAF.Edmx.xml")]
        [DataRow("CloudNimble.EasyAF.XmlDocumentation.xml")]
        public void AssemblyXmlDocumentation_ParseIndividualFiles_ShouldSucceed(string fileName)
        {
            var xmlPath = Path.Combine(_basePath, fileName);
            var xmlDocument = XDocument.Load(xmlPath);
            var documentation = new AssemblyXmlDocumentation(xmlDocument);

            var expectedAssemblyName = Path.GetFileNameWithoutExtension(fileName);
            
            documentation.AssemblyName.Should().Be(expectedAssemblyName);
            documentation.Members.Should().NotBeEmpty();
            
            var namespaces = documentation.GetNamespaces();
            namespaces.Should().NotBeEmpty();
            
            documentation.Types.Should().NotBeEmpty();
            
            foreach (var type in documentation.Types.Values)
            {
                type.MemberType.Should().Be(MemberType.Type);
                type.Name.Should().StartWith("T:");
                type.GetSimpleName().Should().NotBeNullOrWhiteSpace();
            }
        }

        /// <summary>
        /// Tests that member name parsing handles edge cases correctly.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_MemberNameParsing_ShouldHandleEdgeCases()
        {
            var xmlPath = Path.Combine(_basePath, "CloudNimble.EasyAF.Core.xml");
            var xmlDocument = XDocument.Load(xmlPath);
            var documentation = new AssemblyXmlDocumentation(xmlDocument);

            foreach (var member in documentation.Members.Values)
            {
                member.Name.Should().NotBeNullOrWhiteSpace();
                member.MemberType.Should().NotBe(MemberType.Unknown);
                
                if (member.MemberType != MemberType.Type)
                {
                    var containingType = member.GetContainingType();
                    containingType.Should().NotBeNullOrWhiteSpace("non-type members should have a containing type");
                }
            }
        }

        #endregion

    }

}
