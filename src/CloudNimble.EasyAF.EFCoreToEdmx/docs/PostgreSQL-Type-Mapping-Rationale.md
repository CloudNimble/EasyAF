# PostgreSQL Type Mapping in EdmxXmlGenerator

## Background

The `EdmxXmlGenerator.MapToSqlType()` method was updated to return PostgreSQL-native types when `_databaseProviderType` is `DatabaseProviderType.PostgreSQL`, even though the EDMX file's `ProviderManifestToken` is always set to `2012.Azure` (SQL Server).

## Important Context

**The EDMX file exists solely to power EasyAF.CodeGen's code generator.** It will never be used to actually connect to a PostgreSQL database (or any database). It's purely an intermediate representation that the code generator reads to produce C# classes.

## Why This Change Was Made

Tests were failing because they expected PostgreSQL-specific types in the EDMX storage model:
- `GenerateEdmxWithPostgreSQLProvider_ShouldMapDateTimeOffsetToTimestampWithTimeZone`
- `PostgreSQLTypeMappingLogic_ShouldHandleAllCommonTypes`

These tests verified that when converting a PostgreSQL database, the generated EDMX should contain PostgreSQL type names like:
- `timestamp with time zone` (not `datetimeoffset`)
- `character varying` (not `nvarchar`)
- `uuid` (not `uniqueidentifier`)

The assumption was that preserving the source database's type names would help the code generator make better decisions about C# type mappings.

## Why We Might Need to Roll This Back

Since the EDMX is only used for code generation and never connects to a real database:

1. **The code generator may not care about storage types**: If EasyAF.CodeGen only looks at the conceptual model types (which are already correct CLR types like `DateTimeOffset`, `Guid`, etc.), then the storage type names are irrelevant.

2. **Consistency is simpler**: Having the storage model always use SQL Server types means one less variable to consider. The conceptual model already has the correct CLR types.

3. **EDMX tooling expectations**: Any tooling that reads EDMX files may expect SQL Server types in the storage model since `ProviderManifestToken="2012.Azure"` indicates SQL Server.

4. **The tests may have been wrong**: The tests that required PostgreSQL types may have been testing implementation details rather than meaningful behavior. If the code generator works correctly with SQL Server storage types, the tests should be updated instead.

## What Changed

**File:** `EdmxXmlGenerator.cs`
**Method:** `MapToSqlType(EdmxProperty property)`

Added a PostgreSQL-specific type mapping branch that returns PostgreSQL type names when the provider type is PostgreSQL.

## To Rollback

1. Remove the PostgreSQL-specific switch block from `MapToSqlType()`, leaving only the SQL Server mappings
2. Update the PostgreSQL tests to expect SQL Server storage types (or remove them if they're testing irrelevant implementation details)

## Date

This change was made on 2025-11-27 as part of fixing EFCoreToEdmx test failures.
