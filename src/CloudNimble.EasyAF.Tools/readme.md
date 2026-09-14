# EasyAF

.NET global tool (`dotnet easyaf`) for initializing projects, scaffolding EDMX, generating code, and publishing docs.

## Install

```bash
dotnet tool install --global EasyAF
```

Previews:

```bash
dotnet tool install --global EasyAF --prerelease
```

## Usage

```bash
dotnet easyaf --help
dotnet easyaf init --help
dotnet easyaf setup --help
dotnet easyaf database generate --context-name MyAppDbContext
dotnet easyaf code generate --context-name MyAppDbContext
dotnet easyaf mintlify --input-path MyApp.xml --output-path docs/
```

The tool command name is `dotnet-easyaf` (invoked as `dotnet easyaf`).

## Documentation

- Docs: https://easyaf.dev/quickstart
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
