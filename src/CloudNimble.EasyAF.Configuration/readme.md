# EasyAF.Configuration

Binds app settings (API root, app root, HTTP handler mode) into a typed configuration object for EasyAF apps.

## Install

```bash
dotnet add package EasyAF.Configuration
```

## Usage

```csharp
builder.Services.AddConfigurationBase<MyAppConfiguration>(builder.Configuration, "AppSettings");
```

```json
{
  "AppSettings": {
    "ApiRoot": "https://api.example.com",
    "AppRoot": "https://app.example.com",
    "HttpHandlerMode": "Add"
  }
}
```

Use `ConfigurationPlusAdminBase` when the app also needs an admin API client.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
