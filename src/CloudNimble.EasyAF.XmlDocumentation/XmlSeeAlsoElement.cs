using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a seealso XML documentation element for related references.
    /// </summary>
    /// <remarks>
    /// The seealso element creates a link to related types or members.
    /// These are typically displayed in a "See Also" section.
    /// </remarks>
    public class XmlSeeAlsoElement : XmlDocumentationElement
    {

        #region Properties

        /// <summary>
        /// Gets or sets the cross-reference target.
        /// </summary>
        public string Cref { get; set; }

        /// <summary>
        /// Gets or sets the link text to display.
        /// </summary>
        public string LinkText { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlSeeAlsoElement class.
        /// </summary>
        public XmlSeeAlsoElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlSeeAlsoElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlSeeAlsoElement(XElement element) : base(element)
        {
            Cref = element.Attribute("cref")?.Value ?? string.Empty;
            LinkText = element.Attribute("linkText")?.Value ?? element.Value?.Trim() ?? string.Empty;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this seealso element to MDX format as a link.
        /// </summary>
        /// <returns>The MDX representation of this related reference.</returns>
        public override string ToMdx()
        {
            if (string.IsNullOrWhiteSpace(Cref))
            {
                return LinkText ?? Text ?? string.Empty;
            }

            // Parse the cref to determine the link format
            var linkTarget = ParseCref(Cref);
            var displayText = !string.IsNullOrWhiteSpace(LinkText) ? LinkText : GetDisplayTextFromCref(Cref);

            // Create MDX link
            return $"[{displayText}]({linkTarget})";
        }

        #endregion

        #region Private Methods

        private string ParseCref(string cref)
        {
            // Remove the type prefix (T:, M:, P:, etc.)
            if (cref.Length > 2 && cref[1] == ':')
            {
                cref = cref.Substring(2);
            }

            // Convert namespace.type format to relative path
            return cref.Replace('.', '/').ToLowerInvariant();
        }

        private string GetDisplayTextFromCref(string cref)
        {
            // Remove the type prefix (T:, M:, P:, etc.)
            if (cref.Length > 2 && cref[1] == ':')
            {
                cref = cref.Substring(2);
            }

            // Return just the type/member name
            var lastDot = cref.LastIndexOf('.');
            return lastDot >= 0 ? cref.Substring(lastDot + 1) : cref;
        }

        #endregion

    }

}