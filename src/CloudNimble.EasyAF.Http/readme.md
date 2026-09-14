# EasyAF.Http

HttpClient factory helpers, OData v4 response types, and URI builders for EasyAF APIs.

## Install

```bash
dotnet add package EasyAF.Http
```

## Usage

```csharp
services.AddHttpClients<MyAppConfiguration, DelegatingHandler>(config);
```

```csharp
var uri = new Uri("https://api.example.com/Products")
    .ToODataUri(filter: "IsActive eq true", top: 20, orderby: "Name");
```

Add `EasyAF.Http.SystemTextJson` or `EasyAF.Http.NewtonsoftJson` for JSON deserialization of `HttpResponseMessage`.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
