// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Utilities;
using ModelConfig = System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration;


namespace System.Data.Entity.Internal
{
    internal class EdmMetadataContext : DbContext
    {
        public const string TableName = "EdmMetadata";

        static EdmMetadataContext()
        {
            Database.SetInitializer<EdmMetadataContext>(null);
        }

        public EdmMetadataContext(DbConnection existingConnection)
            : base(existingConnection, contextOwnsConnection: false)
        {
        }

#pragma warning disable 612,618
        public virtual IDbSet<EdmMetadata> Metadata { get; set; }
#pragma warning restore 612,618

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ConfigureEdmMetadata(modelBuilder.ModelConfiguration);
        }

        public static void ConfigureEdmMetadata(ModelConfig modelConfiguration)
        {
            DebugCheck.NotNull(modelConfiguration);

#pragma warning disable 612,618
            modelConfiguration.Entity(typeof(EdmMetadata)).ToTable(TableName);
#pragma warning restore 612,618
        }
    }
}
