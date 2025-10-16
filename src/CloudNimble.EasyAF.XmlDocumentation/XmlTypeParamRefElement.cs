using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a typeparamref XML documentation element for type parameter references.
    /// </summary>
    /// <remarks>
    /// The typeparamref element creates a reference to a generic type parameter within the documentation.
    /// It is used to refer to type parameters inline within text.
    /// </remarks>
    public class XmlTypeParamRefElement : XmlDocumentationElement
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name of the referenced type parameter.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlTypeParamRefElement class.
        /// </summary>
        public XmlTypeParamRefElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlTypeParamRefElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlTypeParamRefElement(XElement element) : base(element)
        {
            Name = element.Attribute("name")?.Value ?? string.Empty;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this typeparamref element to MDX format as inline code.
        /// </summary>
        /// <returns>The MDX representation of this type parameter reference.</returns>
        public override string ToMdx()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return string.Empty;
            }

            // Render type parameter reference as inline code
            return $"`{Name}`";
        }

        #endregion

    }

}