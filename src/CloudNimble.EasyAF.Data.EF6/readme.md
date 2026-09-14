# EasyAF.Data.EF6

Entity Framework 6 provider configuration for SQL Server / Azure SQL using Microsoft.Data.SqlClient.

## Install

```bash
dotnet add package EasyAF.Data.EF6
```

## Usage

```csharp
DbConfiguration.SetConfiguration(new EasyAFSqlAzureConfiguration());

using var context = new MyDbContext(connectionString);
```

`EasyAFSqlAzureConfiguration` registers `Microsoft.Data.SqlClient`, EF6 SQL services, and `MicrosoftSqlAzureExecutionStrategy`. Optional Azure AD token support is in `AzureActiveDirectorySqlAuthProvider`.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
