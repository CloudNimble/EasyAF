// --------------------------------------------------------------------------------------------
// <copyright file="UniversalStorageSchemaModifier.cs" company="Effort Team">
//     Copyright (C) Effort Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.StorageSchema
{
    using System;
    using System.Xml.Linq;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.Common.XmlProcessing;

    internal class UniversalStorageSchemaModifier
    {
        public static readonly UniversalStorageSchemaModifier Instance =
            new UniversalStorageSchemaModifier();

        private IElementModifier schemaV1Modifier;
        private IElementModifier schemaV2Modifier;
        private IElementModifier schemaV3Modifier;

        public void Modify(XElement ssdl, IProviderInformation newProvider)
        {
            if (ssdl == null)
            {
                throw new ArgumentNullException("root");
            }

            if (newProvider == null)
            {
                throw new ArgumentNullException("newProvider");
            }

            IElementModifier appropriateModifier = null;
            var ns = ssdl.Name.Namespace;

            // Find the appropriate modifier
            if (ns == StorageSchemaV1Modifier.Namespace)
            {
                appropriateModifier = SchemaV1Modifier;
            }
            else if (ns == StorageSchemaV2Modifier.Namespace)
            {
                appropriateModifier = SchemaV2Modifier;
            }
            else if (ns == StorageSchemaV3Modifier.Namespace)
            {
                appropriateModifier = SchemaV3Modifier;
            }

            if (appropriateModifier == null)
            {
                throw new ArgumentException("", "root");
            }

            IModificationContext context = new ModificationContext();
            context.Set(ModificationContextHelper.NewProvider, newProvider);

            appropriateModifier.Modify(ssdl, context);
        }

        protected IElementModifier SchemaV1Modifier
        {
            get
            {
                if (schemaV1Modifier == null)
                {
                    schemaV1Modifier = new StorageSchemaV1Modifier();
                }

                return schemaV1Modifier;
            }
        }

        protected IElementModifier SchemaV2Modifier
        {
            get
            {
                if (schemaV2Modifier == null)
                {
                    schemaV2Modifier = new StorageSchemaV2Modifier();
                }

                return schemaV2Modifier;
            }
        }

        protected IElementModifier SchemaV3Modifier
        {
            get
            {
                if (schemaV3Modifier == null)
                {
                    schemaV3Modifier = new StorageSchemaV3Modifier();
                }

                return schemaV3Modifier;
            }
        }
    }
}
