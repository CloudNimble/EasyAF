// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Infrastructure;
using System.Xml.Linq;

namespace System.Data.Entity.Utilities
{
    internal static class DbModelExtensions
    {
        public static XDocument GetModel(this DbModel model)
        {
            DebugCheck.NotNull(model);

            return DbContextExtensions.GetModel(w => EdmxWriter.WriteEdmx(model, w));
        }
    }
}
