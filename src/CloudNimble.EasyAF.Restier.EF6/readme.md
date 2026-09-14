# EasyAF.Restier.EF6

Restier Entity Framework 6 API base class with HTTP context, logging, and SimpleMessageBus publishing.

## Install

```bash
dotnet add package EasyAF.Restier.EF6
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

For EF Core, use `EasyAF.Restier.EFCore`.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
