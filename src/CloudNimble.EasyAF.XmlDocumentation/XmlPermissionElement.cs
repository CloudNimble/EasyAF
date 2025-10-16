using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a permission XML documentation element.
    /// </summary>
    /// <remarks>
    /// The permission element documents the security permissions required
    /// to access or use a particular type or member.
    /// </remarks>
    public class XmlPermissionElement : XmlDocumentationElement
    {

        #region Properties

        /// <summary>
        /// Gets or sets the permission type reference.
        /// </summary>
        public string Cref { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlPermissionElement class.
        /// </summary>
        public XmlPermissionElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlPermissionElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlPermissionElement(XElement element) : base(element)
        {
            Cref = element.Attribute("cref")?.Value ?? string.Empty;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this permission element to MDX format.
        /// </summary>
        /// <returns>The MDX representation of this permission requirement.</returns>
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