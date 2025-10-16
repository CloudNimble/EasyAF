using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a parameter XML documentation element.
    /// </summary>
    /// <remarks>
    /// The param element describes a parameter of a method, constructor, or indexer.
    /// It includes the parameter name and description of its purpose and usage.
    /// </remarks>
    public class XmlParameterElement : XmlDocumentationElement
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlParameterElement class.
        /// </summary>
        public XmlParameterElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlParameterElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlParameterElement(XElement element) : base(element)
        {
            Name = element.Attribute("name")?.Value ?? string.Empty;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this parameter element to MDX format.
        /// </summary>
        /// <returns>The MDX representation of this parameter.</returns>
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