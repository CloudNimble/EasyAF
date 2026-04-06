// --------------------------------------------------------------------------------------------
// <copyright file="ComposedElementModifier.cs" company="Effort Team">
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

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.Common.XmlProcessing
{
    using System;
    using System.Xml.Linq;

    internal class ComposedElementModifier : IElementModifier
    {
        private IElementSelector elementSelector;
        private IElementAttributeSelector attributeSelector;
        private IAttributeModifier attributeModifier;
        private IElementModifier elementModifier;

        public ComposedElementModifier(
            IElementSelector elementSelector,
            IElementAttributeSelector attributeSelector,
            IAttributeModifier attributeModifier)
        {
            if (elementSelector is null)
            {
                throw new ArgumentNullException("elementSelector");
            }

            if (attributeSelector is null)
            {
                throw new ArgumentNullException("attributeSelector");
            }

            if (attributeModifier is null)
            {
                throw new ArgumentNullException("attributeModifier");
            }

            this.elementSelector = elementSelector;
            this.attributeSelector = attributeSelector;
            this.attributeModifier = attributeModifier;
        }

        public ComposedElementModifier(
            IElementSelector elementSelector,
            IElementModifier elementModifier)
        {
            if (elementSelector is null)
            {
                throw new ArgumentNullException("elementSelector");
            }

            if (elementModifier is null)
            {
                throw new ArgumentNullException("elementModifier");
            }

            this.elementSelector = elementSelector;
            this.elementModifier = elementModifier;
        }

        public void Modify(XElement element, IModificationContext context)
        {
            if (element is null)
            {
                throw new ArgumentNullException("element");
            }

            foreach (var selected in elementSelector.SelectElements(element))
            {
                if (attributeSelector is not null && attributeModifier is not null)
                {
                    var attribute = attributeSelector.SelectAttribute(selected);

                    if (attribute is not null)
                    {
                        attributeModifier.Modify(attribute, context);
                    }
                }

                if (elementModifier is not null)
                {
                    elementModifier.Modify(selected, context);
                }
            }
        }
    }
}
