using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a code block XML documentation element.
    /// </summary>
    /// <remarks>
    /// The code element contains code examples or snippets.
    /// It is typically rendered as a formatted code block with syntax highlighting.
    /// </remarks>
    public class XmlCodeBlockElement : XmlDocumentationElement
    {

        #region Properties

        /// <summary>
        /// Gets or sets the programming language for syntax highlighting.
        /// </summary>
        public string Language { get; set; } = "csharp";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlCodeBlockElement class.
        /// </summary>
        public XmlCodeBlockElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlCodeBlockElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlCodeBlockElement(XElement element) : base(element)
        {
            // Try to determine language from attributes
            Language = element.Attribute("lang")?.Value ?? 
                      element.Attribute("language")?.Value ?? 
                      "csharp";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this code block element to MDX format with syntax highlighting.
        /// </summary>
        /// <returns>The MDX representation of this code block.</returns>
        public override string ToMdx()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return string.Empty;
            }

            var code = Text.Trim();
            
            // Create code block with language specification
            return $"```{Language.ToLowerInvariant()}\n{code}\n```";
        }

        #endregion

    }

}