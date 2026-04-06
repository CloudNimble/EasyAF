using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a type parameter XML documentation element.
    /// </summary>
    /// <remarks>
    /// The typeparam element describes a generic type parameter.
    /// It includes the parameter name and description of its constraints and usage.
    /// </remarks>
    public class XmlTypeParameterElement : XmlDocumentationElement
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name of the type parameter.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlTypeParameterElement class.
        /// </summary>
        public XmlTypeParameterElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlTypeParameterElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlTypeParameterElement(XElement element) : base(element)
        {
            Name = element.Attribute("name")?.Value ?? string.Empty;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this type parameter element to MDX format.
        /// </summary>
        /// <returns>The MDX representation of this type parameter.</returns>
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