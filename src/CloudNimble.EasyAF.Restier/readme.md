# EasyAF.Restier

Shared Restier helpers for EasyAF OData APIs (operation logging over entity lifecycle).

## Install

```bash
dotnet add package EasyAF.Restier
```

## Usage

```csharp
RestierHelpers.LogOperation(entity, RestierOperationType.Inserting);
```

Pair this with `EasyAF.Restier.EF6` or `EasyAF.Restier.EFCore` for `EasyAFEntityFrameworkApi<TContext>`.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
