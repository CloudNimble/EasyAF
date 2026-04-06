using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a paramref XML documentation element for parameter references.
    /// </summary>
    /// <remarks>
    /// The paramref element creates a reference to a parameter within the documentation.
    /// It is used to refer to parameters inline within text.
    /// </remarks>
    public class XmlParamRefElement : XmlDocumentationElement
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name of the referenced parameter.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlParamRefElement class.
        /// </summary>
        public XmlParamRefElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlParamRefElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlParamRefElement(XElement element) : base(element)
        {
            Name = element.Attribute("name")?.Value ?? string.Empty;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this paramref element to MDX format as inline code.
        /// </summary>
        /// <returns>The MDX representation of this parameter reference.</returns>
        public override string ToMdx()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return string.Empty;
            }

            // Render parameter reference as inline code
            return $"`{Name}`";
        }

        #endregion

    }

}