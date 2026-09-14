# EasyAF.MSBuild

Load, edit, and save `.csproj` / `Directory.Build.props` files while keeping original formatting.

## Install

```bash
dotnet add package EasyAF.MSBuild
```

## Usage

```csharp
MSBuildProjectManager.EnsureMSBuildRegistered();

var manager = new MSBuildProjectManager("MyApp.csproj").Load();
manager.SetProperty("Nullable", "enable");
manager.Save();
```

Call `EnsureMSBuildRegistered()` before any MSBuild API. Used by the `EasyAF` CLI.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
