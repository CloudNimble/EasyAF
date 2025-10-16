using System.Text;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a list XML documentation element.
    /// </summary>
    /// <remarks>
    /// The list element creates bulleted or numbered lists within documentation.
    /// It supports different list types including bullet, number, and table formats.
    /// </remarks>
    public class XmlListElement : XmlDocumentationElement
    {

        #region Properties

        /// <summary>
        /// Gets or sets the type of list (bullet, number, table).
        /// </summary>
        public string Type { get; set; } = "bullet";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlListElement class.
        /// </summary>
        public XmlListElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlListElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlListElement(XElement element) : base(element)
        {
            Type = element.Attribute("type")?.Value ?? "bullet";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this list element to MDX format.
        /// </summary>
        /// <returns>The MDX representation of this list.</returns>
        public override string ToMdx()
        {
            if (InnerElements.Count == 0)
            {
                return string.Empty;
            }

            var result = new StringBuilder();
            
            if (Type.ToLowerInvariant() == "table")
            {
                // Handle table format
                result.AppendLine("| Item | Description |");
                result.AppendLine("| --- | --- |");
                
                foreach (var item in InnerElements)
                {
                    if (item.RawXml.Contains("<item>"))
                    {
                        var itemMdx = item.ToMdx();
                        result.AppendLine($"| {itemMdx} |");
                    }
                }
            }
            else
            {
                // Handle bullet or numbered lists
                var prefix = Type.ToLowerInvariant() == "number" ? "1. " : "- ";
                
                foreach (var item in InnerElements)
                {
                    if (item.RawXml.Contains("<item>"))
                    {
                        var itemMdx = item.ToMdx();
                        result.AppendLine($"{prefix}{itemMdx}");
                    }
                }
            }
            
            return result.ToString().Trim();
        }

        #endregion

    }

}
