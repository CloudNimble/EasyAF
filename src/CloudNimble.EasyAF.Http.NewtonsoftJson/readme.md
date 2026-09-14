# EasyAF.Http.NewtonsoftJson

`HttpResponseMessage` JSON helpers using Newtonsoft.Json.

## Install

```bash
dotnet add package EasyAF.Http.NewtonsoftJson
```

## Usage

```csharp
var (products, error) = await response.DeserializeResponseAsync<ODataV4List<Product>>();
```

Use this package with `EasyAF.Http` when the app serializes with Newtonsoft.Json. For System.Text.Json, use `EasyAF.Http.SystemTextJson` instead.

Depends on `EasyAF.NewtonsoftJson.Compatibility` so `[JsonPropertyName]` / `[JsonIgnore]` from System.Text.Json still apply.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
