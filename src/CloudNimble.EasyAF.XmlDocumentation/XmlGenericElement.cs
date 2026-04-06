using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a generic XML documentation element for unrecognized tags.
    /// </summary>
    /// <remarks>
    /// This class handles XML documentation elements that don't have specific implementations.
    /// It provides basic text extraction and formatting capabilities for any XML element.
    /// </remarks>
    public class XmlGenericElement : XmlDocumentationElement
    {

        #region Properties

        /// <summary>
        /// Gets or sets the XML element name.
        /// </summary>
        public string ElementName { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlGenericElement class.
        /// </summary>
        public XmlGenericElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlGenericElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlGenericElement(XElement element) : base(element)
        {
            ElementName = element.Name.LocalName;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this generic element to MDX format.
        /// </summary>
        /// <returns>The MDX representation of this element.</returns>
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