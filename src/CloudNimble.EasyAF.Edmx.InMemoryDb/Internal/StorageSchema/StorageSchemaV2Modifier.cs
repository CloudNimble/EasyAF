// --------------------------------------------------------------------------------------------
// <copyright file="StorageSchemaV2Modifier.cs" company="Effort Team">
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

using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.StorageSchema;

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.StorageSchema
{
    using System;
    using System.Xml.Linq;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.Common.XmlProcessing;

    internal class StorageSchemaV2Modifier : IElementModifier
    {
        public static readonly XNamespace Namespace = StorageSchemaNamespaces.V2;

        private StorageSchemaContentNameProvider nameProvider;
        private AggregatedElementModifier modificationLogic;
        private IElementVisitor<IProviderInformation> providerParser;

        public StorageSchemaV2Modifier()
        {
            nameProvider = new StorageSchemaContentNameProvider(Namespace);

            providerParser = new ProviderParser(nameProvider);

            modificationLogic = new AggregatedElementModifier();

            // Schema[Provider] : Provider
            modificationLogic.AddModifier(
                new ComposedElementModifier(
                    new SelfElementSelector(),
                    new ProviderAttributeSelector(nameProvider),
                    new ProviderAttributeModifier()));

            // Schema[ProviderManifestToken] : ProviderManifestToken
            modificationLogic.AddModifier(
                new ComposedElementModifier(
                    new SelfElementSelector(),
                    new ProviderManifestTokenAttributeSelector(nameProvider),
                    new ProviderManifestTokenAttributeModifier()));

            // Schema.EntityType
            modificationLogic.AddModifier(
                new ComposedElementModifier(
                    new EntityTypePropertyElementSelector(nameProvider),
                    new CommonPropertyElementModifier(nameProvider)));

            // Schema.Function.Parameter[Type] : FunctionType
            modificationLogic.AddModifier(
                new ComposedElementModifier(
                    new FunctionParameterElementSelector(nameProvider),
                    new TypeAttributeSelector(nameProvider),
                    new FunctionTypeAttributeModifier()));

            // Schema.Function[ReturnType] : FunctionType
            modificationLogic.AddModifier(
                new ComposedElementModifier(
                    new FunctionElementSelector(nameProvider),
                    new ReturnTypeAttributeSelector(nameProvider),
                    new FunctionTypeAttributeModifier()));
        }

        public void Modify(XElement ssdl, IModificationContext context)
        {
            if (ssdl == null)
            {
                throw new ArgumentNullException("ssdl");
            }

            if (ssdl.Name != nameProvider.SchemaElement)
            {
                throw new ArgumentException("", "ssdl");
            }

            // Parse and store original provider information
            var providerInfo = providerParser.VisitElement(ssdl);
            context.Set(ModificationContextHelper.OriginalProvider, providerInfo);

            // Modify the xml
            modificationLogic.Modify(ssdl, context);
        }
    }
}
