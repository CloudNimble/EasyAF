using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents an example XML documentation element.
    /// </summary>
    /// <remarks>
    /// The example element contains code examples that demonstrate how to use a type or member.
    /// It can contain both description text and code blocks.
    /// </remarks>
    public class XmlExampleElement : XmlDocumentationElement
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlExampleElement class.
        /// </summary>
        public XmlExampleElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlExampleElement class with XML content.
        /// </summary>
        /// <param name="element">The XML element to parse.</param>
        public XmlExampleElement(XElement element) : base(element)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts this example element to MDX format with proper code formatting.
        /// </summary>
        /// <returns>The MDX representation of this example.</returns>
        public override string ToMdx()
        {
            if (string.IsNullOrWhiteSpace(Text) && InnerElements.Count == 0)
            {
                return string.Empty;
            }

            var result = Text ?? string.Empty;

            // Process inner elements, paying special attention to code blocks
            foreach (var innerElement in InnerElements)
            {
                var mdx = innerElement.ToMdx();
                result = result.Replace(innerElement.RawXml, mdx);
            }

            // Clean up and format
            result = result.Trim();
            
            // If the entire example is just code, wrap it in a code block
            if (!result.Contains("```") && !string.IsNullOrWhiteSpace(result))
            {
                result = $"```csharp\n{result}\n```";
            }
            
            return result;
        }

        #endregion

    }

}