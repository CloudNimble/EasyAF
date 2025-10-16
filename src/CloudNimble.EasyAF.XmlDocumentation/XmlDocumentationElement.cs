using CloudNimble.EasyAF.Core;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a base XML documentation element with common properties.
    /// </summary>
    /// <remarks>
    /// This abstract class provides the foundation for all XML documentation elements,
    /// including summary, remarks, parameters, returns, and other documentation tags.
    /// It handles parsing of XML content and preserves the original structure for
    /// conversion to MDX format.
    /// </remarks>
    public abstract class XmlDocumentationElement
    {

        #region Properties

        /// <summary>
        /// Gets or sets the raw XML content of the element.
        /// </summary>
        public string RawXml { get; set; }

        /// <summary>
        /// Gets or sets the parsed text content of the element.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the inner XML elements for nested content.
        /// </summary>
        public List<XmlDocumentationElement> InnerElements { get; set; } = new List<XmlDocumentationElement>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlDocumentationElement class.
        /// </summary>
        protected XmlDocumentationElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlDocumentationElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        protected XmlDocumentationElement(XElement element)
        {
            Ensure.ArgumentNotNull(element, nameof(element));

            RawXml = element.ToString();
            Text = element.Value?.Trim();
            ParseInnerElements(element);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Parses inner XML elements recursively.
        /// </summary>
        /// <param name="element">The parent XML element to parse.</param>
        protected virtual void ParseInnerElements(XElement element)
        {
            foreach (var innerElement in element.Elements())
            {
                var docElement = CreateDocumentationElement(innerElement);
                if (docElement is not null)
                {
                    InnerElements.Add(docElement);
                }
            }
        }

        /// <summary>
        /// Creates the appropriate documentation element based on the XML element name.
        /// </summary>
        /// <param name="element">The XML element to convert.</param>
        /// <returns>The appropriate documentation element, or null if not supported.</returns>
        protected virtual XmlDocumentationElement CreateDocumentationElement(XElement element)
        {
            return element.Name.LocalName.ToLowerInvariant() switch
            {
                "see" => new XmlSeeElement(element),
                "seealso" => new XmlSeeAlsoElement(element),
                "paramref" => new XmlParamRefElement(element),
                "typeparamref" => new XmlTypeParamRefElement(element),
                "c" => new XmlCodeElement(element),
                "code" => new XmlCodeBlockElement(element),
                "para" => new XmlParagraphElement(element),
                "list" => new XmlListElement(element),
                _ => new XmlGenericElement(element)
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this element to MDX format.
        /// </summary>
        /// <returns>The MDX representation of this element.</returns>
        public abstract string ToMdx();

        #endregion

    }

}
