using CloudNimble.EasyAF.Core;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents the root XML documentation structure for a .NET assembly.
    /// </summary>
    /// <remarks>
    /// This class parses and contains all the XML documentation for a single assembly,
    /// including all types, members, and their associated documentation elements.
    /// It provides methods to access and filter documentation by various criteria.
    /// </remarks>
    public class AssemblyXmlDocumentation
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name of the assembly this documentation belongs to.
        /// </summary>
        public string AssemblyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the collection of all documented members in the assembly.
        /// </summary>
        public Dictionary<string, XmlMember> Members { get; set; } = new Dictionary<string, XmlMember>();

        /// <summary>
        /// Gets the collection of all documented types in the assembly.
        /// </summary>
        public Dictionary<string, XmlMember> Types => Members
            .Where(kvp => kvp.Key.StartsWith("T:"))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        /// <summary>
        /// Gets the collection of all documented methods in the assembly.
        /// </summary>
        public Dictionary<string, XmlMember> Methods => Members
            .Where(kvp => kvp.Key.StartsWith("M:"))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        /// <summary>
        /// Gets the collection of all documented properties in the assembly.
        /// </summary>
        public Dictionary<string, XmlMember> Properties => Members
            .Where(kvp => kvp.Key.StartsWith("P:"))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        /// <summary>
        /// Gets the collection of all documented fields in the assembly.
        /// </summary>
        public Dictionary<string, XmlMember> Fields => Members
            .Where(kvp => kvp.Key.StartsWith("F:"))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        /// <summary>
        /// Gets the collection of all documented events in the assembly.
        /// </summary>
        public Dictionary<string, XmlMember> Events => Members
            .Where(kvp => kvp.Key.StartsWith("E:"))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlDocumentationDocument class.
        /// </summary>
        public AssemblyXmlDocumentation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlDocumentationDocument class from an XML document.
        /// </summary>
        /// <param name="xmlDocument">The XML documentation to parse.</param>
        public AssemblyXmlDocumentation(XDocument xmlDocument)
        {
            Ensure.ArgumentNotNull(xmlDocument, nameof(xmlDocument));

            ParseXmlDocument(xmlDocument);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all types within a specific namespace.
        /// </summary>
        /// <param name="namespace">The namespace to filter by.</param>
        /// <returns>A dictionary of types in the specified namespace.</returns>
        public Dictionary<string, XmlMember> GetTypesByNamespace(string @namespace)
        {
            if (string.IsNullOrWhiteSpace(@namespace))
            {
                return new Dictionary<string, XmlMember>();
            }

            return Types
                .Where(kvp => GetNamespace(kvp.Key) == @namespace)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Gets all members belonging to a specific type.
        /// </summary>
        /// <param name="typeName">The fully qualified type name (without T: prefix).</param>
        /// <returns>A dictionary of members belonging to the specified type.</returns>
        public Dictionary<string, XmlMember> GetMembersByType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return new Dictionary<string, XmlMember>();
            }

            return Members
                .Where(kvp => !kvp.Key.StartsWith("T:") && kvp.Key.Contains(typeName))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Gets all unique namespaces represented in the documentation.
        /// </summary>
        /// <returns>A list of unique namespace names.</returns>
        public List<string> GetNamespaces()
        {
            return Types.Keys
                .Select(GetNamespace)
                .Where(ns => !string.IsNullOrWhiteSpace(ns))
                .Distinct()
                .OrderBy(ns => ns)
                .ToList();
        }

        #endregion

        #region Private Methods

        private void ParseXmlDocument(XDocument xmlDocument)
        {
            // Get assembly name
            var assemblyElement = xmlDocument.Root?.Element("assembly");
            if (assemblyElement is not null)
            {
                AssemblyName = assemblyElement.Element("name")?.Value ?? string.Empty;
            }

            // Parse all members
            var membersElement = xmlDocument.Root?.Element("members");
            if (membersElement is not null)
            {
                foreach (var memberElement in membersElement.Elements("member"))
                {
                    var memberName = memberElement.Attribute("name")?.Value;
                    if (!string.IsNullOrWhiteSpace(memberName))
                    {
                        var xmlMember = new XmlMember(memberElement);
                        Members[memberName] = xmlMember;
                    }
                }
            }
        }

        private string GetNamespace(string memberName)
        {
            // Remove type prefix (T:, M:, P:, etc.)
            if (memberName.Length > 2 && memberName[1] == ':')
            {
                memberName = memberName.Substring(2);
            }

            // Extract namespace from full type name
            var lastDot = memberName.LastIndexOf('.');
            if (lastDot > 0)
            {
                var potentialNamespace = memberName.Substring(0, lastDot);
                
                // Handle nested types (containing '+')
                var plusIndex = potentialNamespace.IndexOf('+');
                if (plusIndex > 0)
                {
                    potentialNamespace = potentialNamespace.Substring(0, plusIndex);
                }
                
                return potentialNamespace;
            }

            return string.Empty;
        }

        #endregion

    }

}
