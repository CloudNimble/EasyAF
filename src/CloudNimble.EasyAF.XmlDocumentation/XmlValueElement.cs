using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a value XML documentation element for properties.
    /// </summary>
    /// <remarks>
    /// The value element describes the value that a property represents.
    /// It is used primarily for properties to explain what the property value means.
    /// </remarks>
    public class XmlValueElement : XmlDocumentationElement
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlValueElement class.
        /// </summary>
        public XmlValueElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlValueElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlValueElement(XElement element) : base(element)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this value element to MDX format.
        /// </summary>
        /// <returns>The MDX representation of this value description.</returns>
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