using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a summary XML documentation element.
    /// </summary>
    /// <remarks>
    /// The summary element provides a brief description of a type or member.
    /// It is typically displayed prominently in documentation and should be
    /// concise but informative.
    /// </remarks>
    public class XmlSummaryElement : XmlDocumentationElement
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlSummaryElement class.
        /// </summary>
        public XmlSummaryElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlSummaryElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlSummaryElement(XElement element) : base(element)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this summary element to MDX format.
        /// </summary>
        /// <returns>The MDX representation of this summary.</returns>
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

            // Clean up and format
            result = result.Trim();
            
            return result;
        }

        #endregion

    }

}