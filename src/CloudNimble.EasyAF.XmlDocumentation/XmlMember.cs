using CloudNimble.EasyAF.Core;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.XmlDocumentation
{

    /// <summary>
    /// Represents a documented member from XML documentation.
    /// </summary>
    /// <remarks>
    /// This class contains all the documentation elements for a single member,
    /// including summary, remarks, parameters, return values, exceptions, and examples.
    /// It provides methods to convert the documentation to various formats.
    /// </remarks>
    public class XmlMember
    {

        #region Properties

        /// <summary>
        /// Gets or sets the full member name with prefix (e.g., T:System.String, M:System.String.Length).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the member type (Type, Method, Property, Field, Event).
        /// </summary>
        public MemberType MemberType { get; set; }

        /// <summary>
        /// Gets or sets the summary documentation element.
        /// </summary>
        public XmlSummaryElement Summary { get; set; }

        /// <summary>
        /// Gets or sets the remarks documentation element.
        /// </summary>
        public XmlRemarksElement Remarks { get; set; }

        /// <summary>
        /// Gets the collection of parameter documentation elements.
        /// </summary>
        public List<XmlParameterElement> Parameters { get; set; } = new List<XmlParameterElement>();

        /// <summary>
        /// Gets the collection of type parameter documentation elements.
        /// </summary>
        public List<XmlTypeParameterElement> TypeParameters { get; set; } = new List<XmlTypeParameterElement>();

        /// <summary>
        /// Gets or sets the returns documentation element.
        /// </summary>
        public XmlReturnsElement Returns { get; set; }

        /// <summary>
        /// Gets or sets the value documentation element (for properties).
        /// </summary>
        public XmlValueElement Value { get; set; }

        /// <summary>
        /// Gets the collection of exception documentation elements.
        /// </summary>
        public List<XmlExceptionElement> Exceptions { get; set; } = new List<XmlExceptionElement>();

        /// <summary>
        /// Gets the collection of example documentation elements.
        /// </summary>
        public List<XmlExampleElement> Examples { get; set; } = new List<XmlExampleElement>();

        /// <summary>
        /// Gets the collection of see also references.
        /// </summary>
        public List<XmlSeeAlsoElement> SeeAlso { get; set; } = new List<XmlSeeAlsoElement>();

        /// <summary>
        /// Gets the collection of permission documentation elements.
        /// </summary>
        public List<XmlPermissionElement> Permissions { get; set; } = new List<XmlPermissionElement>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the XmlMember class.
        /// </summary>
        public XmlMember()
        {
        }

        /// <summary>
        /// Initializes a new instance of the XmlMember class from an XML element.
        /// </summary>
        /// <param name="memberElement">The XML member element to parse.</param>
        public XmlMember(XElement memberElement)
        {
            Ensure.ArgumentNotNull(memberElement, nameof(memberElement));

            Name = memberElement.Attribute("name")?.Value ?? string.Empty;
            MemberType = DetermineMemberType(Name);

            ParseDocumentationElements(memberElement);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the simple name of the member without prefix and namespace.
        /// </summary>
        /// <returns>The simple member name.</returns>
        public string GetSimpleName()
        {
            var name = Name;
            
            // Remove prefix (T:, M:, P:, etc.)
            if (name.Length > 2 && name[1] == ':')
            {
                name = name.Substring(2);
            }

            // Handle method parameters and generics first
            var parenIndex = name.IndexOf('(');
            if (parenIndex >= 0)
            {
                name = name.Substring(0, parenIndex);
            }

            // Get the last part after the last dot
            var lastDot = name.LastIndexOf('.');
            if (lastDot >= 0)
            {
                name = name.Substring(lastDot + 1);
            }

            // Handle nested classes (OuterClass+InnerClass)
            var plusIndex = name.LastIndexOf('+');
            if (plusIndex >= 0)
            {
                name = name.Substring(plusIndex + 1);
            }

            return name;
        }

        /// <summary>
        /// Gets the namespace of the member.
        /// </summary>
        /// <returns>The namespace name.</returns>
        public string GetNamespace()
        {
            var name = Name;
            
            // Remove prefix (T:, M:, P:, etc.)
            if (name.Length > 2 && name[1] == ':')
            {
                name = name.Substring(2);
            }

            // Remove method parameters if present
            var parenIndex = name.IndexOf('(');
            if (parenIndex >= 0)
            {
                name = name.Substring(0, parenIndex);
            }

            // Handle nested classes (OuterClass+InnerClass)
            var plusIndex = name.IndexOf('+');
            if (plusIndex >= 0)
            {
                name = name.Substring(0, plusIndex);
            }

            // For types, get everything before the last dot
            if (MemberType == MemberType.Type)
            {
                var lastDot = name.LastIndexOf('.');
                return lastDot > 0 ? name.Substring(0, lastDot) : string.Empty;
            }

            // For members, get the namespace of the containing type
            var memberDot = name.LastIndexOf('.');
            if (memberDot > 0)
            {
                var typeName = name.Substring(0, memberDot);
                var namespaceDot = typeName.LastIndexOf('.');
                return namespaceDot > 0 ? typeName.Substring(0, namespaceDot) : string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the containing type name for members.
        /// </summary>
        /// <returns>The containing type name, or empty string for types.</returns>
        public string GetContainingType()
        {
            if (MemberType == MemberType.Type)
            {
                return string.Empty;
            }

            var name = Name;
            
            // Remove prefix (T:, M:, P:, etc.)
            if (name.Length > 2 && name[1] == ':')
            {
                name = name.Substring(2);
            }

            // Remove method parameters if present
            var parenIndex = name.IndexOf('(');
            if (parenIndex >= 0)
            {
                name = name.Substring(0, parenIndex);
            }

            // Get everything before the last dot (which should be the type name)
            var lastDot = name.LastIndexOf('.');
            if (lastDot > 0)
            {
                var typeName = name.Substring(0, lastDot);
                var typeNameDot = typeName.LastIndexOf('.');
                return typeNameDot > 0 ? typeName.Substring(typeNameDot + 1) : typeName;
            }

            return string.Empty;
        }

        #endregion

        #region Private Methods

        private MemberType DetermineMemberType(string memberName)
        {
            if (string.IsNullOrWhiteSpace(memberName) || memberName.Length < 2)
            {
                return MemberType.Unknown;
            }

            return memberName[0] switch
            {
                'T' => MemberType.Type,
                'M' => MemberType.Method,
                'P' => MemberType.Property,
                'F' => MemberType.Field,
                'E' => MemberType.Event,
                'N' => MemberType.Namespace,
                _ => MemberType.Unknown
            };
        }

        private void ParseDocumentationElements(XElement memberElement)
        {
            foreach (var element in memberElement.Elements())
            {
                switch (element.Name.LocalName.ToLowerInvariant())
                {
                    case "summary":
                        Summary = new XmlSummaryElement(element);
                        break;
                    case "remarks":
                        Remarks = new XmlRemarksElement(element);
                        break;
                    case "param":
                        Parameters.Add(new XmlParameterElement(element));
                        break;
                    case "typeparam":
                        TypeParameters.Add(new XmlTypeParameterElement(element));
                        break;
                    case "returns":
                        Returns = new XmlReturnsElement(element);
                        break;
                    case "value":
                        Value = new XmlValueElement(element);
                        break;
                    case "exception":
                        Exceptions.Add(new XmlExceptionElement(element));
                        break;
                    case "example":
                        Examples.Add(new XmlExampleElement(element));
                        break;
                    case "seealso":
                        SeeAlso.Add(new XmlSeeAlsoElement(element));
                        break;
                    case "permission":
                        Permissions.Add(new XmlPermissionElement(element));
                        break;
                }
            }
        }

        #endregion

    }

    /// <summary>
    /// Enumeration of member types in XML documentation.
    /// </summary>
    public enum MemberType
    {
        /// <summary>
        /// Unknown member type.
        /// </summary>
        Unknown,

        /// <summary>
        /// Type (class, interface, struct, enum, delegate).
        /// </summary>
        Type,

        /// <summary>
        /// Method or constructor.
        /// </summary>
        Method,

        /// <summary>
        /// Property or indexer.
        /// </summary>
        Property,

        /// <summary>
        /// Field or constant.
        /// </summary>
        Field,

        /// <summary>
        /// Event.
        /// </summary>
        Event,

        /// <summary>
        /// Namespace.
        /// </summary>
        Namespace
    }

}
