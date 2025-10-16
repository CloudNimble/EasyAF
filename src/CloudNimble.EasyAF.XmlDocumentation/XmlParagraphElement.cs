using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a paragraph XML documentation element.
    /// </summary>
    /// <remarks>
    /// The para element represents a paragraph break within documentation text.
    /// It is used to separate sections of content for better readability.
    /// </remarks>
    public class XmlParagraphElement : XmlDocumentationElement
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlParagraphElement class.
        /// </summary>
        public XmlParagraphElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlParagraphElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlParagraphElement(XElement element) : base(element)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this paragraph element to MDX format.
        /// </summary>
        /// <returns>The MDX representation of this paragraph.</returns>
        public override string ToMdx()
        {
            if (string.IsNullOrWhiteSpace(Text) && InnerElements.Count == 0)
            {
                return string.Empty;
            }

            var result = Text ?? string.Empty;

            // Process inner elements
            foreach (var innerElement in InnerElements)
            {
                result = result.Replace(innerElement.RawXml, innerElement.ToMdx());
            }

            // Clean up and format - add paragraph breaks
            result = result.Trim();
            
            return result + "\n\n";
        }

        #endregion

    }

}