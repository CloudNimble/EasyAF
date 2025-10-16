using CloudNimble.EasyAF.XmlDocumentation;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.XmlDocumentation
{

    /// <summary>
    /// Tests for edge cases, error scenarios, and boundary conditions in XML documentation parsing.
    /// </summary>
    [TestClass]
    public class XmlDocumentationEdgeCaseTests
    {

        #region Test Methods

        /// <summary>
        /// Tests parsing of malformed XML documentation.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_MalformedXml_ShouldThrowXmlException()
        {
            var malformedXml = "<?xml version='1.0'?><doc><assembly><name>Test</name></assembly><members><member name='T:Test' unclosed tag>";

            Action action = () =>
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(malformedXml));
                var doc = XDocument.Load(stream);
                new AssemblyXmlDocumentation(doc);
            };

            action.Should().Throw<XmlException>();
        }

        /// <summary>
        /// Tests parsing of XML with missing assembly name.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_MissingAssemblyName_ShouldHandleGracefully()
        {
            var xmlWithoutName = """
                <?xml version="1.0"?>     
                <doc>
                    <assembly>
                    </assembly>
                    <members>
                        <member name="T:TestClass">
                            <summary>Test class</summary>
                        </member>
                    </members>
                </doc>
                """;

            var doc = XDocument.Parse(xmlWithoutName);
            var documentation = new AssemblyXmlDocumentation(doc);

            documentation.AssemblyName.Should().BeEmpty();
            documentation.Members.Should().HaveCount(1);
        }

        /// <summary>
        /// Tests parsing of XML with missing members section.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_MissingMembersSection_ShouldHandleGracefully()
        {
            var xmlWithoutMembers = """
                <?xml version="1.0"?>
                <doc>
                    <assembly>
                        <name>TestAssembly</name>
                    </assembly>
                </doc>
                """;

            var doc = XDocument.Parse(xmlWithoutMembers);
            var documentation = new AssemblyXmlDocumentation(doc);

            documentation.AssemblyName.Should().Be("TestAssembly");
            documentation.Members.Should().BeEmpty();
        }

        /// <summary>
        /// Tests parsing of members with missing name attribute.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_MemberWithoutName_ShouldSkipMember()
        {
            var xmlWithMemberWithoutName = @"<?xml version='1.0'?>
                <doc>
                    <assembly>
                        <name>TestAssembly</name>
                    </assembly>
                    <members>
                        <member>
                            <summary>Member without name</summary>
                        </member>
                        <member name='T:ValidClass'>
                            <summary>Valid member</summary>
                        </member>
                    </members>
                </doc>";

            var doc = XDocument.Parse(xmlWithMemberWithoutName);
            var documentation = new AssemblyXmlDocumentation(doc);

            documentation.Members.Should().HaveCount(1);
            documentation.Members.Should().ContainKey("T:ValidClass");
        }

        /// <summary>
        /// Tests parsing of very large XML documentation files.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_LargeXmlFile_ShouldHandleEfficiently()
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.AppendLine("<?xml version='1.0'?>");
            xmlBuilder.AppendLine("<doc>");
            xmlBuilder.AppendLine("    <assembly><name>LargeAssembly</name></assembly>");
            xmlBuilder.AppendLine("    <members>");

            // Generate 1000 members
            for (int i = 0; i < 1000; i++)
            {
                xmlBuilder.AppendLine($"        <member name='T:TestNamespace.TestClass{i}'>");
                xmlBuilder.AppendLine($"            <summary>Test class number {i}</summary>");
                xmlBuilder.AppendLine("        </member>");
            }

            xmlBuilder.AppendLine("    </members>");
            xmlBuilder.AppendLine("</doc>");

            var doc = XDocument.Parse(xmlBuilder.ToString());
            var documentation = new AssemblyXmlDocumentation(doc);

            documentation.AssemblyName.Should().Be("LargeAssembly");
            documentation.Members.Should().HaveCount(1000);
            documentation.Types.Should().HaveCount(1000);
        }

        /// <summary>
        /// Tests member name parsing with unusual but valid .NET member names.
        /// </summary>
        [TestMethod]
        [DataRow("T:Namespace.Class`1", "Class`1", "Namespace")]
        [DataRow("T:Namespace.Class`2+NestedClass", "NestedClass", "Namespace")]
        [DataRow("M:Class.op_Addition(Class,Class)", "op_Addition", "")]
        [DataRow("P:Class.Item(System.String)", "Item", "")]
        [DataRow("M:Class.#ctor(System.String)", "#ctor", "")]
        [DataRow("M:Class.#cctor", "#cctor", "")]
        [DataRow("F:Class.field_name", "field_name", "")]
        [DataRow("E:Class.SomeEvent", "SomeEvent", "")]
        public void XmlMember_UnusualMemberNames_ShouldParseCorrectly(string memberName, string expectedSimpleName, string expectedNamespace)
        {
            var xmlElement = XElement.Parse($"<member name='{memberName}'><summary>Test</summary></member>");
            var member = new XmlMember(xmlElement);

            member.GetSimpleName().Should().Be(expectedSimpleName);
            member.GetNamespace().Should().Be(expectedNamespace);
        }

        /// <summary>
        /// Tests parsing of XML documentation with Unicode characters.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_UnicodeContent_ShouldParseCorrectly()
        {
            var xmlWithUnicode = """
                <?xml version="1.0"?>
                <doc>
                    <assembly>
                        <name>UnicodeAssembly</name>
                    </assembly>
                    <members>
                        <member name="T:TestClass">
                            <summary>This class handles 中文字符 and émojis 🚀</summary>
                            <remarks>Supports русский text and العربية</remarks>
                        </member>
                    </members>
                </doc>
                """;

            var doc = XDocument.Parse(xmlWithUnicode);
            var documentation = new AssemblyXmlDocumentation(doc);

            var member = documentation.Members.Values.First();
            member.Summary.Text.Should().Contain("中文字符");
            member.Summary.Text.Should().Contain("🚀");
            member.Remarks.Text.Should().Contain("русский");
            member.Remarks.Text.Should().Contain("العربية");
        }

        /// <summary>
        /// Tests parsing of XML documentation with CDATA sections.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_CDataContent_ShouldParseCorrectly()
        {
            var xmlWithCData = """
                <?xml version="1.0"?>
                <doc>
                    <assembly>
                        <name>CDataAssembly</name>
                    </assembly>
                    <members>
                        <member name="T:TestClass">
                            <summary><![CDATA[This contains <XML> tags and & symbols]]></summary>
                            <example>
                                <code><![CDATA[
                                    if (x < y && y > z) 
                                    {
                                        Console.WriteLine("Complex condition");
                                    }
                                ]]></code>
                            </example>
                        </member>
                    </members>
                </doc>
                """;

            var doc = XDocument.Parse(xmlWithCData);
            var documentation = new AssemblyXmlDocumentation(doc);

            var member = documentation.Members.Values.First();
            member.Summary.Text.Should().Contain("<XML>");
            member.Summary.Text.Should().Contain("&");
            member.Examples.Should().HaveCount(1);
            member.Examples[0].Text.Should().Contain("x < y && y > z");
        }

        /// <summary>
        /// Tests parsing of deeply nested XML documentation structures.
        /// </summary>
        [TestMethod]
        public void XmlDocumentationElements_DeeplyNestedStructure_ShouldParseCorrectly()
        {
            var nestedXml = """
                <summary>
                    <para>
                        This is a paragraph with <see cref="System.String"/> and 
                        <paramref name="value"/> references.
                        <list type="bullet">
                            <item>
                                <description>
                                    Item with <c>code</c> and more <see cref="System.Int32"/> references.
                                </description>
                            </item>
                        </list>
                    </para>
                </summary>
                """;

            var element = XElement.Parse(nestedXml);
            var summaryElement = new XmlSummaryElement(element);

            summaryElement.Should().NotBeNull();
            summaryElement.InnerElements.Should().NotBeEmpty();
            summaryElement.RawXml.Should().Contain("see cref=\"System.String\"");
            summaryElement.RawXml.Should().Contain("paramref name=\"value\"");
        }

        /// <summary>
        /// Tests handling of XML documentation with invalid member type prefixes.
        /// </summary>
        [TestMethod]
        public void XmlMember_InvalidMemberPrefix_ShouldDefaultToUnknown()
        {
            var xmlElement = XElement.Parse("<member name='X:InvalidPrefix.Member'><summary>Test</summary></member>");
            var member = new XmlMember(xmlElement);

            member.MemberType.Should().Be(MemberType.Unknown);
            member.Name.Should().Be("X:InvalidPrefix.Member");
        }

        /// <summary>
        /// Tests parsing of XML documentation with very long member names.
        /// </summary>
        [TestMethod]
        public void XmlMember_VeryLongMemberName_ShouldParseCorrectly()
        {
            var longNamespace = string.Join(".", Enumerable.Repeat("VeryLongNamespacePart", 10));
            var longClassName = "VeryLongClassNameThatExceedsNormalLengthLimits";
            var longMethodName = "VeryLongMethodNameWithManyParametersAndGenericTypes";
            var fullMemberName = $"M:{longNamespace}.{longClassName}.{longMethodName}(System.String,System.Int32,System.Boolean)";

            var xmlElement = XElement.Parse($"<member name='{fullMemberName}'><summary>Test</summary></member>");
            var member = new XmlMember(xmlElement);

            member.Name.Should().Be(fullMemberName);
            member.MemberType.Should().Be(MemberType.Method);
            member.GetSimpleName().Should().Be(longMethodName);
            member.GetNamespace().Should().Be(longNamespace);
        }

        /// <summary>
        /// Tests parsing of XML documentation with special characters in content.
        /// </summary>
        [TestMethod]
        public void XmlDocumentationElements_SpecialCharacters_ShouldParseCorrectly()
        {
            var xmlWithSpecialChars = @"
                <summary>
                    This method handles special characters: &lt; &gt; &amp; &quot; &apos;
                    And line breaks:
                    Line 1
                    Line 2
                    
                    With tabs:	indented content
                </summary>";

            var element = XElement.Parse(xmlWithSpecialChars);
            var summaryElement = new XmlSummaryElement(element);

            summaryElement.Text.Should().Contain("<");
            summaryElement.Text.Should().Contain(">");
            summaryElement.Text.Should().Contain("&");
            summaryElement.Text.Should().Contain("\"");
            summaryElement.Text.Should().Contain("'");
            summaryElement.Text.Should().Contain("Line 1");
            summaryElement.Text.Should().Contain("Line 2");
        }

        /// <summary>
        /// Tests that GetTypesByNamespace handles null and empty namespace correctly.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_GetTypesByNamespace_WithNullOrEmpty_ShouldReturnEmpty()
        {
            var xmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Baselines", "CloudNimble.EasyAF.Core.xml");
            var xmlDocument = XDocument.Load(xmlPath);
            var documentation = new AssemblyXmlDocumentation(xmlDocument);

            var typesWithNull = documentation.GetTypesByNamespace(null);
            var typesWithEmpty = documentation.GetTypesByNamespace(string.Empty);
            var typesWithWhitespace = documentation.GetTypesByNamespace("   ");

            typesWithNull.Should().BeEmpty();
            typesWithEmpty.Should().BeEmpty();
            typesWithWhitespace.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that GetMembersByType handles null and empty type name correctly.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_GetMembersByType_WithNullOrEmpty_ShouldReturnEmpty()
        {
            var xmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Baselines", "CloudNimble.EasyAF.Core.xml");
            var xmlDocument = XDocument.Load(xmlPath);
            var documentation = new AssemblyXmlDocumentation(xmlDocument);

            var membersWithNull = documentation.GetMembersByType(null);
            var membersWithEmpty = documentation.GetMembersByType(string.Empty);
            var membersWithWhitespace = documentation.GetMembersByType("   ");

            membersWithNull.Should().BeEmpty();
            membersWithEmpty.Should().BeEmpty();
            membersWithWhitespace.Should().BeEmpty();
        }

        /// <summary>
        /// Tests parsing performance with various XML documentation file sizes.
        /// </summary>
        [TestMethod]
        public void AssemblyXmlDocumentation_ParsingPerformance_ShouldBeReasonable()
        {
            var baselinePath = Path.Combine(Directory.GetCurrentDirectory(), "Baselines");
            var xmlFiles = Directory.GetFiles(baselinePath, "*.xml");

            foreach (var xmlFile in xmlFiles)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                var xmlDocument = XDocument.Load(xmlFile);
                var documentation = new AssemblyXmlDocumentation(xmlDocument);
                
                stopwatch.Stop();

                // Performance assertion - parsing should complete within reasonable time
                stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, 
                    $"parsing {Path.GetFileName(xmlFile)} should complete within 5 seconds");
                
                documentation.Members.Should().NotBeEmpty($"{Path.GetFileName(xmlFile)} should have parsed members");
            }
        }

        #endregion

    }

}
