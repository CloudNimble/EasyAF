# EasyAF.Business.EFCore

Entity managers for Entity Framework Core: CRUD, audit fields, and lifecycle hooks.

## Install

```bash
dotnet add package EasyAF.Business.EFCore
```

## Usage

```csharp
public class UserManager : EntityManager<MyDbContext, User>
{
    public UserManager(MyDbContext context, IMessagePublisher publisher)
        : base(context, publisher) { }

    public override async Task OnInsertingAsync(User entity)
    {
        await base.OnInsertingAsync(entity); // audit fields
        entity.IsActive = true;
    }
}
```

Also includes `IdentifiableEntityManager`, `StatusEntityManager`, and `StateMachineEntityManager`. For EF6, use `EasyAF.Business.EF6`.

On .NET 11, `DirectUpdate` / `DirectDelete` are stubbed until Z.EntityFramework.Plus ships an EF Core 11 package.

Requires `EasyAF.Data.EFCore`.

## Documentation

- Docs: https://easyaf.dev/guides/business-layer
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
