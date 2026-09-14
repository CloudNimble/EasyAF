# EasyAF.Http.SystemTextJson

`HttpResponseMessage` JSON helpers using System.Text.Json.

## Install

```bash
dotnet add package EasyAF.Http.SystemTextJson
```

## Usage

```csharp
var (products, error) = await response.DeserializeResponseAsync<ODataV4List<Product>>();
```

Use this package with `EasyAF.Http` when the app serializes with System.Text.Json. For Newtonsoft.Json, use `EasyAF.Http.NewtonsoftJson` instead.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
