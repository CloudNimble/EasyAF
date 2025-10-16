using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a remarks XML documentation element.
    /// </summary>
    /// <remarks>
    /// The remarks element provides additional detailed information about a type or member.
    /// It is typically displayed after the summary and can contain more extensive explanations,
    /// usage notes, or implementation details.
    /// </remarks>
    public class XmlRemarksElement : XmlDocumentationElement
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlRemarksElement class.
        /// </summary>
        public XmlRemarksElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlRemarksElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlRemarksElement(XElement element) : base(element)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this remarks element to MDX format.
        /// </summary>
        /// <returns>The MDX representation of these remarks.</returns>
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