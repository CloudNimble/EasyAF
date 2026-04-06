// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Text;
using System.Xml;

namespace System.Data.Entity.Core.Metadata.Edm
{
    internal abstract class XmlSchemaWriter
    {
        protected XmlWriter _xmlWriter;
        protected double _version;

        internal void WriteComment(string comment)
        {
            if (!String.IsNullOrEmpty(comment))
            {
                _xmlWriter.WriteComment(comment);
            }
        }

        internal virtual void WriteEndElement()
        {
            _xmlWriter.WriteEndElement();
        }

        protected static string GetQualifiedTypeName(string prefix, string typeName)
        {
            var sb = new StringBuilder();
            return sb.Append(prefix).Append(".").Append(typeName).ToString();
        }

        internal static string GetLowerCaseStringFromBoolValue(bool value)
        {
            return value ? XmlConstants.True : XmlConstants.False;
        }
    }
}
