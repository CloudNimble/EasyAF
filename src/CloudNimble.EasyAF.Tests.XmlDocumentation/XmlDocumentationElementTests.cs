using CloudNimble.EasyAF.XmlDocumentation;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.XmlDocumentation
{

    /// <summary>
    /// Comprehensive tests for XML documentation element classes.
    /// </summary>
    [TestClass]
    public class XmlDocumentationElementTests
    {

        #region Test Methods

        /// <summary>
        /// Tests XmlSummaryElement parsing and properties.
        /// </summary>
        [TestMethod]
        public void XmlSummaryElement_Constructor_ShouldParseCorrectly()
        {
            var element = XElement.Parse("<summary>This is a summary description.</summary>");
            var summaryElement = new XmlSummaryElement(element);

            summaryElement.Should().NotBeNull();
            summaryElement.RawXml.Should().Contain("This is a summary description");
            summaryElement.Text.Should().Contain("This is a summary description");
        }

        /// <summary>
        /// Tests XmlRemarksElement parsing and properties.
        /// </summary>
        [TestMethod]
        public void XmlRemarksElement_Constructor_ShouldParseCorrectly()
        {
            var element = XElement.Parse("<remarks>These are additional remarks about the member.</remarks>");
            var remarksElement = new XmlRemarksElement(element);

            remarksElement.Should().NotBeNull();
            remarksElement.RawXml.Should().Contain("These are additional remarks");
            remarksElement.Text.Should().Contain("These are additional remarks");
        }

        /// <summary>
        /// Tests XmlParameterElement parsing with name attribute.
        /// </summary>
        [TestMethod]
        public void XmlParameterElement_Constructor_ShouldParseNameAndContent()
        {
            var element = XElement.Parse("<param name='value'>The input value parameter.</param>");
            var paramElement = new XmlParameterElement(element);

            paramElement.Should().NotBeNull();
            paramElement.Name.Should().Be("value");
            paramElement.RawXml.Should().Contain("The input value parameter");
            paramElement.Text.Should().Contain("The input value parameter");
        }

        /// <summary>
        /// Tests XmlTypeParameterElement parsing with name attribute.
        /// </summary>
        [TestMethod]
        public void XmlTypeParameterElement_Constructor_ShouldParseNameAndContent()
        {
            var element = XElement.Parse("<typeparam name='T'>The generic type parameter.</typeparam>");
            var typeParamElement = new XmlTypeParameterElement(element);

            typeParamElement.Should().NotBeNull();
            typeParamElement.Name.Should().Be("T");
            typeParamElement.RawXml.Should().Contain("The generic type parameter");
            typeParamElement.Text.Should().Contain("The generic type parameter");
        }

        /// <summary>
        /// Tests XmlReturnsElement parsing.
        /// </summary>
        [TestMethod]
        public void XmlReturnsElement_Constructor_ShouldParseCorrectly()
        {
            var element = XElement.Parse("<returns>Returns a boolean value indicating success.</returns>");
            var returnsElement = new XmlReturnsElement(element);

            returnsElement.Should().NotBeNull();
            returnsElement.RawXml.Should().Contain("Returns a boolean value");
            returnsElement.Text.Should().Contain("Returns a boolean value");
        }

        /// <summary>
        /// Tests XmlValueElement parsing for property documentation.
        /// </summary>
        [TestMethod]
        public void XmlValueElement_Constructor_ShouldParseCorrectly()
        {
            var element = XElement.Parse("<value>Gets or sets the current value.</value>");
            var valueElement = new XmlValueElement(element);

            valueElement.Should().NotBeNull();
            valueElement.RawXml.Should().Contain("Gets or sets the current value");
            valueElement.Text.Should().Contain("Gets or sets the current value");
        }

        /// <summary>
        /// Tests XmlExceptionElement parsing with cref attribute.
        /// </summary>
        [TestMethod]
        public void XmlExceptionElement_Constructor_ShouldParseCrefAndContent()
        {
            var element = XElement.Parse("<exception cref='System.ArgumentNullException'>Thrown when value is null.</exception>");
            var exceptionElement = new XmlExceptionElement(element);

            exceptionElement.Should().NotBeNull();
            exceptionElement.Cref.Should().Be("System.ArgumentNullException");
            exceptionElement.RawXml.Should().Contain("Thrown when value is null");
            exceptionElement.Text.Should().Contain("Thrown when value is null");
        }

        /// <summary>
        /// Tests XmlExampleElement parsing with nested code elements.
        /// </summary>
        [TestMethod]
        public void XmlExampleElement_Constructor_ShouldParseWithCodeElements()
        {
            var element = XElement.Parse(@"
                <example>
                    This example shows how to use the method:
                    <code>
                    var result = SomeMethod('test', 123);
                    Console.WriteLine(result);
                    </code>
                </example>");
            
            var exampleElement = new XmlExampleElement(element);

            exampleElement.Should().NotBeNull();
            exampleElement.RawXml.Should().Contain("This example shows");
            exampleElement.RawXml.Should().Contain("var result = SomeMethod");
            exampleElement.Text.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Tests XmlSeeElement parsing with cref attribute.
        /// </summary>
        [TestMethod]
        public void XmlSeeElement_Constructor_ShouldParseCrefAttribute()
        {
            var element = XElement.Parse("<see cref='System.String.Length'/>");
            var seeElement = new XmlSeeElement(element);

            seeElement.Should().NotBeNull();
            seeElement.Cref.Should().Be("System.String.Length");
        }

        /// <summary>
        /// Tests XmlSeeAlsoElement parsing with cref attribute.
        /// </summary>
        [TestMethod]
        public void XmlSeeAlsoElement_Constructor_ShouldParseCrefAttribute()
        {
            var element = XElement.Parse("<seealso cref='RelatedMethod'/>");
            var seeAlsoElement = new XmlSeeAlsoElement(element);

            seeAlsoElement.Should().NotBeNull();
            seeAlsoElement.Cref.Should().Be("RelatedMethod");
        }

        /// <summary>
        /// Tests XmlCodeElement parsing.
        /// </summary>
        [TestMethod]
        public void XmlCodeElement_Constructor_ShouldParseCodeContent()
        {
            var element = XElement.Parse("<code>var x = 10; Console.WriteLine(x);</code>");
            var codeElement = new XmlCodeElement(element);

            codeElement.Should().NotBeNull();
            codeElement.RawXml.Should().Contain("var x = 10");
            codeElement.Text.Should().Contain("var x = 10");
        }

        /// <summary>
        /// Tests XmlCodeBlockElement parsing with language attribute.
        /// </summary>
        [TestMethod]
        public void XmlCodeBlockElement_Constructor_ShouldParseLanguageAndContent()
        {
            var element = XElement.Parse("<c>SomeMethod()</c>");
            var codeBlockElement = new XmlCodeBlockElement(element);

            codeBlockElement.Should().NotBeNull();
            codeBlockElement.RawXml.Should().Contain("SomeMethod()");
            codeBlockElement.Text.Should().Contain("SomeMethod()");
        }

        /// <summary>
        /// Tests XmlParamRefElement parsing with name attribute.
        /// </summary>
        [TestMethod]
        public void XmlParamRefElement_Constructor_ShouldParseNameAttribute()
        {
            var element = XElement.Parse("<paramref name='value'/>");
            var paramRefElement = new XmlParamRefElement(element);

            paramRefElement.Should().NotBeNull();
            paramRefElement.Name.Should().Be("value");
        }

        /// <summary>
        /// Tests XmlTypeParamRefElement parsing with name attribute.
        /// </summary>
        [TestMethod]
        public void XmlTypeParamRefElement_Constructor_ShouldParseNameAttribute()
        {
            var element = XElement.Parse("<typeparamref name='T'/>");
            var typeParamRefElement = new XmlTypeParamRefElement(element);

            typeParamRefElement.Should().NotBeNull();
            typeParamRefElement.Name.Should().Be("T");
        }

        /// <summary>
        /// Tests XmlPermissionElement parsing with cref attribute.
        /// </summary>
        [TestMethod]
        public void XmlPermissionElement_Constructor_ShouldParseCrefAndContent()
        {
            var element = XElement.Parse("<permission cref='System.Security.Permissions.FileIOPermission'>Requires file access permission.</permission>");
            var permissionElement = new XmlPermissionElement(element);

            permissionElement.Should().NotBeNull();
            permissionElement.Cref.Should().Be("System.Security.Permissions.FileIOPermission");
            permissionElement.RawXml.Should().Contain("Requires file access permission");
            permissionElement.Text.Should().Contain("Requires file access permission");
        }

        /// <summary>
        /// Tests XmlListElement parsing with complex list structure.
        /// </summary>
        [TestMethod]
        public void XmlListElement_Constructor_ShouldParseListStructure()
        {
            var element = XElement.Parse(@"
                <list type='bullet'>
                    <item>
                        <description>First item description</description>
                    </item>
                    <item>
                        <description>Second item description</description>
                    </item>
                </list>");
            
            var listElement = new XmlListElement(element);

            listElement.Should().NotBeNull();
            listElement.Type.Should().Be("bullet");
            listElement.RawXml.Should().Contain("First item description");
            listElement.RawXml.Should().Contain("Second item description");
        }

        /// <summary>
        /// Tests XmlParagraphElement parsing.
        /// </summary>
        [TestMethod]
        public void XmlParagraphElement_Constructor_ShouldParseContent()
        {
            var element = XElement.Parse("<para>This is a paragraph of text with some <see cref='System.String'/> references.</para>");
            var paragraphElement = new XmlParagraphElement(element);

            paragraphElement.Should().NotBeNull();
            paragraphElement.RawXml.Should().Contain("This is a paragraph");
            paragraphElement.Text.Should().Contain("This is a paragraph");
        }

        /// <summary>
        /// Tests XmlGenericElement parsing for unknown elements.
        /// </summary>
        [TestMethod]
        public void XmlGenericElement_Constructor_ShouldParseUnknownElements()
        {
            var element = XElement.Parse("<custom attribute='value'>Custom content here</custom>");
            var genericElement = new XmlGenericElement(element);

            genericElement.Should().NotBeNull();
            genericElement.ElementName.Should().Be("custom");
            genericElement.RawXml.Should().Contain("Custom content here");
            genericElement.Text.Should().Contain("Custom content here");
        }

        /// <summary>
        /// Tests that all XML documentation elements handle null input gracefully.
        /// </summary>
        [TestMethod]
        public void XmlDocumentationElements_NullInput_ShouldThrowArgumentNullException()
        {
            Action summaryAction = () => new XmlSummaryElement(null);
            Action remarksAction = () => new XmlRemarksElement(null);
            Action parameterAction = () => new XmlParameterElement(null);
            Action typeParameterAction = () => new XmlTypeParameterElement(null);
            Action returnsAction = () => new XmlReturnsElement(null);
            Action valueAction = () => new XmlValueElement(null);
            Action exceptionAction = () => new XmlExceptionElement(null);
            Action exampleAction = () => new XmlExampleElement(null);

            summaryAction.Should().Throw<ArgumentNullException>();
            remarksAction.Should().Throw<ArgumentNullException>();
            parameterAction.Should().Throw<ArgumentNullException>();
            typeParameterAction.Should().Throw<ArgumentNullException>();
            returnsAction.Should().Throw<ArgumentNullException>();
            valueAction.Should().Throw<ArgumentNullException>();
            exceptionAction.Should().Throw<ArgumentNullException>();
            exampleAction.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// Tests default constructors for all XML documentation elements.
        /// </summary>
        [TestMethod]
        public void XmlDocumentationElements_DefaultConstructors_ShouldCreateValidInstances()
        {
            var summaryElement = new XmlSummaryElement();
            var remarksElement = new XmlRemarksElement();
            var valueElement = new XmlValueElement();
            var returnsElement = new XmlReturnsElement();

            summaryElement.Should().NotBeNull();
            remarksElement.Should().NotBeNull();
            valueElement.Should().NotBeNull();
            returnsElement.Should().NotBeNull();
        }

        /// <summary>
        /// Tests XML elements with nested content and mixed formatting.
        /// </summary>
        [TestMethod]
        public void XmlDocumentationElements_NestedContent_ShouldParseCorrectly()
        {
            var complexElement = XElement.Parse(@"
                <summary>
                    This method processes <paramref name='input'/> and returns a <see cref='System.Boolean'/>.
                    <para>Additional paragraph with more details.</para>
                    Use <c>SomeMethod(value)</c> to call this method.
                </summary>");
            
            var summaryElement = new XmlSummaryElement(complexElement);

            summaryElement.Should().NotBeNull();
            summaryElement.RawXml.Should().Contain("paramref name=\"input\"");
            summaryElement.RawXml.Should().Contain("see cref=\"System.Boolean\"");
            summaryElement.RawXml.Should().Contain("Additional paragraph");
            summaryElement.InnerElements.Should().NotBeEmpty();
        }

        /// <summary>
        /// Tests XML elements with empty content.
        /// </summary>
        [TestMethod]
        public void XmlDocumentationElements_EmptyContent_ShouldHandleGracefully()
        {
            var emptySummary = XElement.Parse("<summary></summary>");
            var emptyParam = XElement.Parse("<param name='test'></param>");
            var emptyException = XElement.Parse("<exception cref='System.Exception'></exception>");

            var summaryElement = new XmlSummaryElement(emptySummary);
            var paramElement = new XmlParameterElement(emptyParam);
            var exceptionElement = new XmlExceptionElement(emptyException);

            summaryElement.Should().NotBeNull();
            paramElement.Should().NotBeNull();
            paramElement.Name.Should().Be("test");
            exceptionElement.Should().NotBeNull();
            exceptionElement.Cref.Should().Be("System.Exception");
        }

        #endregion

    }

}