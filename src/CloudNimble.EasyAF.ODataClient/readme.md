# EasyAF.ODataClient

A preconfigured Simple.OData.Client v4 client that uses EasyAF HttpClient and configuration.

## Install

```bash
dotnet add package EasyAF.ODataClient
```

## Usage

```csharp
services.AddSingleton<ApiClient>();

var client = serviceProvider.GetRequiredService<ApiClient>();
var products = await client.For<Product>("Products").FindEntriesAsync();
```

`ApiClient` is constructed from `IHttpClientFactory` and `ConfigurationBase`. `ApiBatch` wraps `ODataBatch` with the same settings.

Requires `EasyAF.Configuration` and `EasyAF.Http`.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
