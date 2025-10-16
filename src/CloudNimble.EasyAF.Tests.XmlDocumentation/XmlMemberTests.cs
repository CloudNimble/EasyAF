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
    /// Comprehensive tests for XmlMember functionality.
    /// </summary>
    [TestClass]
    public class XmlMemberTests
    {

        #region Fields

        private static readonly string _basePath = Path.Combine(Directory.GetCurrentDirectory(), "Baselines");

        #endregion

        #region Test Methods

        /// <summary>
        /// Tests XmlMember constructor with valid XML element.
        /// </summary>
        [TestMethod]
        public void XmlMember_Constructor_ShouldParseValidXmlElement()
        {
            var xmlElement = XElement.Parse(@"
                <member name='T:CloudNimble.EasyAF.Core.TestClass'>
                    <summary>This is a test class.</summary>
                    <remarks>This class is used for testing purposes.</remarks>
                </member>");

            var xmlMember = new XmlMember(xmlElement);

            xmlMember.Name.Should().Be("T:CloudNimble.EasyAF.Core.TestClass");
            xmlMember.MemberType.Should().Be(MemberType.Type);
            xmlMember.Summary.Should().NotBeNull();
            xmlMember.Remarks.Should().NotBeNull();
        }

        /// <summary>
        /// Tests XmlMember constructor with null input.
        /// </summary>
        [TestMethod]
        public void XmlMember_Constructor_WithNullElement_ShouldThrowArgumentNullException()
        {
            Action action = () => new XmlMember(null);
            action.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// Tests member type determination for all possible prefixes.
        /// </summary>
        [TestMethod]
        [DataRow("T:System.String", MemberType.Type)]
        [DataRow("M:System.String.Length", MemberType.Method)]
        [DataRow("P:System.String.Length", MemberType.Property)]
        [DataRow("F:System.String.Empty", MemberType.Field)]
        [DataRow("E:System.ComponentModel.PropertyChanged", MemberType.Event)]
        [DataRow("N:System", MemberType.Namespace)]
        [DataRow("X:Unknown", MemberType.Unknown)]
        [DataRow("", MemberType.Unknown)]
        [DataRow("T", MemberType.Unknown)]
        public void XmlMember_MemberType_ShouldBeCorrectlyDetermined(string memberName, MemberType expectedType)
        {
            var xmlElement = XElement.Parse($"<member name='{memberName}'></member>");
            var xmlMember = new XmlMember(xmlElement);

            xmlMember.MemberType.Should().Be(expectedType);
        }

        /// <summary>
        /// Tests GetSimpleName method for various member types.
        /// </summary>
        [TestMethod]
        [DataRow("T:System.String", "String")]
        [DataRow("M:System.String.Substring(System.Int32)", "Substring")]
        [DataRow("P:System.String.Length", "Length")]
        [DataRow("F:System.String.Empty", "Empty")]
        [DataRow("T:System.Collections.Generic.List`1", "List`1")]
        [DataRow("T:OuterClass+InnerClass", "InnerClass")]
        [DataRow("M:Class.Method(System.String,System.Int32)", "Method")]
        public void XmlMember_GetSimpleName_ShouldReturnCorrectName(string memberName, string expectedSimpleName)
        {
            var xmlElement = XElement.Parse($"<member name='{memberName}'></member>");
            var xmlMember = new XmlMember(xmlElement);

            xmlMember.GetSimpleName().Should().Be(expectedSimpleName);
        }

        /// <summary>
        /// Tests GetNamespace method for various member types.
        /// </summary>
        [TestMethod]
        [DataRow("T:System.String", "System")]
        [DataRow("M:System.String.Substring(System.Int32)", "System")]
        [DataRow("P:System.Collections.Generic.List`1.Count", "System.Collections.Generic")]
        [DataRow("F:MyNamespace.MyClass.MyField", "MyNamespace")]
        [DataRow("T:OuterNamespace.OuterClass+InnerClass", "OuterNamespace")]
        [DataRow("T:GlobalClass", "")]
        public void XmlMember_GetNamespace_ShouldReturnCorrectNamespace(string memberName, string expectedNamespace)
        {
            var xmlElement = XElement.Parse($"<member name='{memberName}'></member>");
            var xmlMember = new XmlMember(xmlElement);

            xmlMember.GetNamespace().Should().Be(expectedNamespace);
        }

        /// <summary>
        /// Tests GetContainingType method for non-type members.
        /// </summary>
        [TestMethod]
        [DataRow("M:System.String.Substring(System.Int32)", "String")]
        [DataRow("P:System.Collections.Generic.List`1.Count", "List`1")]
        [DataRow("F:MyNamespace.MyClass.MyField", "MyClass")]
        [DataRow("E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged", "INotifyPropertyChanged")]
        [DataRow("T:System.String", "")] // Types should return empty string
        public void XmlMember_GetContainingType_ShouldReturnCorrectType(string memberName, string expectedContainingType)
        {
            var xmlElement = XElement.Parse($"<member name='{memberName}'></member>");
            var xmlMember = new XmlMember(xmlElement);

            xmlMember.GetContainingType().Should().Be(expectedContainingType);
        }

        /// <summary>
        /// Tests parsing of complex XML documentation elements.
        /// </summary>
        [TestMethod]
        public void XmlMember_ComplexDocumentation_ShouldParseAllElements()
        {
            var xmlElement = XElement.Parse(@"
                <member name='M:TestClass.TestMethod(System.String,System.Int32)'>
                    <summary>This is a test method.</summary>
                    <remarks>This method demonstrates complex documentation.</remarks>
                    <param name='input'>The input string parameter.</param>
                    <param name='count'>The count parameter.</param>
                    <typeparam name='T'>The generic type parameter.</typeparam>
                    <returns>Returns a boolean value.</returns>
                    <exception cref='System.ArgumentNullException'>Thrown when input is null.</exception>
                    <exception cref='System.ArgumentOutOfRangeException'>Thrown when count is negative.</exception>
                    <example>
                        <code>
                        var result = TestMethod('hello', 5);
                        </code>
                    </example>
                    <seealso cref='OtherMethod'/>
                    <permission cref='System.Security.Permissions.FileIOPermission'>Requires file access.</permission>
                </member>");

            var xmlMember = new XmlMember(xmlElement);

            xmlMember.Summary.Should().NotBeNull();
            xmlMember.Remarks.Should().NotBeNull();
            xmlMember.Parameters.Should().HaveCount(2);
            xmlMember.TypeParameters.Should().HaveCount(1);
            xmlMember.Returns.Should().NotBeNull();
            xmlMember.Exceptions.Should().HaveCount(2);
            xmlMember.Examples.Should().HaveCount(1);
            xmlMember.SeeAlso.Should().HaveCount(1);
            xmlMember.Permissions.Should().HaveCount(1);
        }

        /// <summary>
        /// Tests parsing of property with value documentation.
        /// </summary>
        [TestMethod]
        public void XmlMember_PropertyWithValue_ShouldParseValueElement()
        {
            var xmlElement = XElement.Parse(@"
                <member name='P:TestClass.TestProperty'>
                    <summary>This is a test property.</summary>
                    <value>Gets or sets the test value.</value>
                </member>");

            var xmlMember = new XmlMember(xmlElement);

            xmlMember.MemberType.Should().Be(MemberType.Property);
            xmlMember.Summary.Should().NotBeNull();
            xmlMember.Value.Should().NotBeNull();
        }

        /// <summary>
        /// Tests parsing of members from real XML documentation files.
        /// </summary>
        [TestMethod]
        public void XmlMember_RealXmlDocumentation_ShouldParseCorrectly()
        {
            var xmlPath = Path.Combine(_basePath, "CloudNimble.EasyAF.Core.xml");
            var xmlDocument = XDocument.Load(xmlPath);
            var documentation = new AssemblyXmlDocumentation(xmlDocument);

            foreach (var member in documentation.Members.Values.Take(10)) // Test first 10 members
            {
                member.Name.Should().NotBeNullOrWhiteSpace();
                member.MemberType.Should().NotBe(MemberType.Unknown);
                
                if (member.Summary is not null && !string.IsNullOrWhiteSpace(member.Summary.Text))
                {
                    member.Summary.Text.Should().NotBeNullOrWhiteSpace();
                }
                
                if (member.Parameters.Count > 0)
                {
                    foreach (var param in member.Parameters)
                    {
                        param.Name.Should().NotBeNullOrWhiteSpace();
                    }
                }
                
                if (member.Exceptions.Count > 0)
                {
                    foreach (var exception in member.Exceptions)
                    {
                        exception.Cref.Should().NotBeNullOrWhiteSpace();
                    }
                }
            }
        }

        /// <summary>
        /// Tests that XmlMember handles empty or minimal documentation gracefully.
        /// </summary>
        [TestMethod]
        public void XmlMember_MinimalDocumentation_ShouldHandleGracefully()
        {
            var xmlElement = XElement.Parse("<member name='T:TestClass'></member>");
            var xmlMember = new XmlMember(xmlElement);

            xmlMember.Name.Should().Be("T:TestClass");
            xmlMember.MemberType.Should().Be(MemberType.Type);
            xmlMember.Summary.Should().BeNull();
            xmlMember.Remarks.Should().BeNull();
            xmlMember.Parameters.Should().BeEmpty();
            xmlMember.TypeParameters.Should().BeEmpty();
            xmlMember.Exceptions.Should().BeEmpty();
            xmlMember.Examples.Should().BeEmpty();
            xmlMember.SeeAlso.Should().BeEmpty();
            xmlMember.Permissions.Should().BeEmpty();
        }

        /// <summary>
        /// Tests parsing of constructor documentation.
        /// </summary>
        [TestMethod]
        public void XmlMember_Constructor_ShouldParseCorrectly()
        {
            var xmlElement = XElement.Parse(@"
                <member name='M:TestClass.#ctor(System.String)'>
                    <summary>Initializes a new instance of TestClass.</summary>
                    <param name='name'>The name parameter.</param>
                </member>");

            var xmlMember = new XmlMember(xmlElement);

            xmlMember.MemberType.Should().Be(MemberType.Method);
            xmlMember.GetSimpleName().Should().Be("#ctor");
            xmlMember.Summary.Should().NotBeNull();
            xmlMember.Parameters.Should().HaveCount(1);
        }

        /// <summary>
        /// Tests parsing of generic method documentation.
        /// </summary>
        [TestMethod]
        public void XmlMember_GenericMethod_ShouldParseCorrectly()
        {
            var xmlElement = XElement.Parse(@"
                <member name='M:TestClass.GenericMethod``1(``0)'>
                    <summary>A generic method.</summary>
                    <typeparam name='T'>The generic type parameter.</typeparam>
                    <param name='value'>The value parameter of type T.</param>
                    <returns>Returns the input value.</returns>
                </member>");

            var xmlMember = new XmlMember(xmlElement);

            xmlMember.MemberType.Should().Be(MemberType.Method);
            xmlMember.GetSimpleName().Should().Be("GenericMethod``1");
            xmlMember.TypeParameters.Should().HaveCount(1);
            xmlMember.Parameters.Should().HaveCount(1);
            xmlMember.Returns.Should().NotBeNull();
        }

        /// <summary>
        /// Tests default constructor creates empty XmlMember.
        /// </summary>
        [TestMethod]
        public void XmlMember_DefaultConstructor_ShouldCreateEmptyMember()
        {
            var xmlMember = new XmlMember();

            xmlMember.Name.Should().BeEmpty();
            xmlMember.MemberType.Should().Be(MemberType.Unknown);
            xmlMember.Summary.Should().BeNull();
            xmlMember.Remarks.Should().BeNull();
            xmlMember.Parameters.Should().BeEmpty();
            xmlMember.TypeParameters.Should().BeEmpty();
            xmlMember.Returns.Should().BeNull();
            xmlMember.Value.Should().BeNull();
            xmlMember.Exceptions.Should().BeEmpty();
            xmlMember.Examples.Should().BeEmpty();
            xmlMember.SeeAlso.Should().BeEmpty();
            xmlMember.Permissions.Should().BeEmpty();
        }

        #endregion

    }

}
