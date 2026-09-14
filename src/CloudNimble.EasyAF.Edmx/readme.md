# EasyAF.Edmx

Portable Entity Framework 6 EDMX / ObjectContext runtime used by EasyAF code generation and EF6 apps.

## Install

```bash
dotnet add package EasyAF.Edmx
```

## Usage

This is the netstandard2.0 EF6 stack EasyAF uses to load EDMX metadata and run EF6 on modern TFMs. Application projects usually take `EasyAF.Data.EF6` and `EasyAF.Business.EF6` instead of referencing this package directly.

For in-memory tests, add `EasyAF.Edmx.InMemoryDb`.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
