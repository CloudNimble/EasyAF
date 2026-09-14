# EasyAF.CodeGen

EDMX-driven C# generators (entities, DbContext, managers, interceptors, Restier APIs, SimpleMessageBus messages).

## Install

```bash
dotnet add package EasyAF.CodeGen
```

This package is a development dependency. Typical consumers are `EasyAF.Analyzers.EF6` and the `EasyAF` CLI (`dotnet easyaf code generate`).

## Usage

```csharp
using var generator = new EntityGenerator(extraUsings, modelNamespace, entity);
generator.Generate();
var csharp = generator.ToString();
```

Load the EDMX with `EdmxLoader` first. Prefer the CLI or Roslyn analyzer rather than calling generators from application code.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
