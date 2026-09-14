# EasyAF.Restier.EFCore

Restier Entity Framework Core API base class with HTTP context, logging, and SimpleMessageBus publishing.

## Install

```bash
dotnet add package EasyAF.Restier.EFCore
```

## Usage

```csharp
public class MyApi : EasyAFEntityFrameworkApi<MyDbContext>
{
    public MyApi(
        IServiceProvider serviceProvider,
        IHttpContextAccessor httpContextAccessor,
        IMessagePublisher messagePublisher,
        ILogger<EasyAFEntityFrameworkApi<MyDbContext>> logger)
        : base(serviceProvider, httpContextAccessor, messagePublisher, logger)
    {
    }
}
```

For EF6, use `EasyAF.Restier.EF6`.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
