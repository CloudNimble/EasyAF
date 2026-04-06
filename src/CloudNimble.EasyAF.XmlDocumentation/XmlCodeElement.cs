using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents an inline code XML documentation element.
    /// </summary>
    /// <remarks>
    /// The c element marks text as inline code within documentation.
    /// It is typically rendered with monospace font and different styling.
    /// </remarks>
    public class XmlCodeElement : XmlDocumentationElement
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlCodeElement class.
        /// </summary>
        public XmlCodeElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlCodeElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlCodeElement(XElement element) : base(element)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this inline code element to MDX format.
        /// </summary>
        /// <returns>The MDX representation of this inline code.</returns>
        public override string ToMdx()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return string.Empty;
            }

            // Wrap in backticks for inline code
            return $"`{Text.Trim()}`";
        }

        #endregion

    }

}
