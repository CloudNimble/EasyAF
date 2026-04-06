// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Migrations.Model;

namespace System.Data.Entity.Migrations.Infrastructure
{
    /// <summary>
    /// Explicitly implemented by <see cref="DbMigration" /> to prevent certain members from showing up
    /// in the IntelliSense of scaffolded migrations.
    /// </summary>
    public interface IDbMigration
    {
        /// <summary>
        /// Adds a custom <see cref="MigrationOperation" /> to the migration.
        /// Custom operation implementors are encouraged to create extension methods on
        /// <see cref="IDbMigration" /> that provide a fluent-style API for adding new operations.
        /// </summary>
        /// <param name="migrationOperation"> The operation to add. </param>
        void AddOperation(MigrationOperation migrationOperation);
    }
}
