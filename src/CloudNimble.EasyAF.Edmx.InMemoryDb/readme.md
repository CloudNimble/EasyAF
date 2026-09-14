# EasyAF.Edmx.InMemoryDb

In-memory EF6 provider (Effort-style) for EasyAF EDMX / ObjectContext tests.

## Install

```bash
dotnet add package EasyAF.Edmx.InMemoryDb
```

## Usage

```csharp
var connection = EntityConnectionFactory.CreateTransient("name=MyEntities");
using var context = new MyEntities(connection);
```

Use this when tests should not hit SQL Server. Pair with `EasyAF.Edmx`.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
