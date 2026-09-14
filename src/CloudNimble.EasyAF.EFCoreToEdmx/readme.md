# EasyAF.EFCoreToEdmx

Converts an EF Core `DbContext` (or a live database) into EDMX for Restier and EasyAF code generation.

## Install

```bash
dotnet add package EasyAF.EFCoreToEdmx
```

## Usage

```csharp
var converter = new EdmxConverter();
await converter.ConvertToEdmxFileAsync(dbContext, "Model.edmx");
```

CSDL `Summary` comes from EF comments / `MS_Description`. SQL Server `EasyAF_LongDescription` extended properties map to CSDL `LongDescription`.

The `EasyAF` CLI (`dotnet easyaf database generate`) is the usual entry point.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
