using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents an exception XML documentation element.
    /// </summary>
    /// <remarks>
    /// The exception element documents exceptions that can be thrown by a method or property.
    /// It includes the exception type and conditions under which it is thrown.
    /// </remarks>
    public class XmlExceptionElement : XmlDocumentationElement
    {

        #region Properties

        /// <summary>
        /// Gets or sets the fully qualified name of the exception type.
        /// </summary>
        public string Cref { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlExceptionElement class.
        /// </summary>
        public XmlExceptionElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlExceptionElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlExceptionElement(XElement element) : base(element)
        {
            Cref = element.Attribute("cref")?.Value ?? string.Empty;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this exception element to MDX format.
        /// </summary>
        /// <returns>The MDX representation of this exception.</returns>
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