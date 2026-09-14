# EasyAF.Business.EF6

Entity managers for Entity Framework 6: CRUD, audit fields, and lifecycle hooks.

## Install

```bash
dotnet add package EasyAF.Business.EF6
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

Also includes `IdentifiableEntityManager`, `StatusEntityManager`, and `StateMachineEntityManager`. For EF Core, use `EasyAF.Business.EFCore`.

Requires `EasyAF.Data.EF6`.

## Documentation

- Docs: https://easyaf.dev/guides/business-layer
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
