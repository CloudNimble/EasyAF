# EasyAF.Analyzers.EF6

Roslyn source generators for EasyAF EF6 models (entities, DbContext, managers, Restier APIs).

## Install

```bash
dotnet add package EasyAF.Analyzers.EF6
```

The package ships as an analyzer. Adding the reference is enough; you do not call the generators from application code.

## Usage

Point the generator at your EDMX (via the EasyAF MSBuild props the package imports) and rebuild. Generated C# is written as analyzer output.

Prefer `dotnet easyaf code generate` when you want files on disk instead of compiler-generated sources.

Requires an EF6 EDMX in the consuming project. For EF Core, generate EDMX with `EasyAF.EFCoreToEdmx` first.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
