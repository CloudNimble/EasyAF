# EasyAF.NewtonsoftJson.Compatibility

Makes Newtonsoft.Json honor System.Text.Json serialization attributes.

## Install

```bash
dotnet add package EasyAF.NewtonsoftJson.Compatibility
```

## Usage

```csharp
var settings = new JsonSerializerSettings
{
    ContractResolver = new SystemTextJsonContractResolver()
};
```

Types decorated with `System.Text.Json.Serialization.JsonPropertyName` and `JsonIgnore` then serialize correctly under Newtonsoft.Json. `EasyAF.Http.NewtonsoftJson` uses this resolver automatically.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
