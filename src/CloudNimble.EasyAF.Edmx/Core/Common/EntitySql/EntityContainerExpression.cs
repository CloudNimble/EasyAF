// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql
{
    // <summary>
    // Represents an eSQL expression classified as <see cref="ExpressionResolutionClass.EntityContainer" />.
    // </summary>
    internal sealed class EntityContainerExpression : ExpressionResolution
    {
        internal EntityContainerExpression(EntityContainer entityContainer)
            : base(ExpressionResolutionClass.EntityContainer)
        {
            EntityContainer = entityContainer;
        }

        internal override string ExpressionClassName
        {
            get { return EntityContainerClassName; }
        }

        internal static string EntityContainerClassName
        {
            get { return Strings.LocalizedEntityContainerExpression; }
        }

        internal readonly EntityContainer EntityContainer;
    }
}
