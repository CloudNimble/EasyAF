using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a returns XML documentation element.
    /// </summary>
    /// <remarks>
    /// The returns element describes the return value of a method or property.
    /// It explains what the method returns and under what conditions.
    /// </remarks>
    public class XmlReturnsElement : XmlDocumentationElement
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlReturnsElement class.
        /// </summary>
        public XmlReturnsElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlReturnsElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlReturnsElement(XElement element) : base(element)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this returns element to MDX format.
        /// </summary>
        /// <returns>The MDX representation of this returns description.</returns>
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