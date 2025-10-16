// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    internal interface IDbMappingConvention : IConvention
    {
        void Apply(DbDatabaseMapping databaseMapping);
    }
}
