# EasyAF.Data.EFCore

Entity Framework Core helpers for EasyAF entities (ignore change-tracking fields on `DbObservableObject`).

## Install

```bash
dotnet add package EasyAF.Data.EFCore
```

## Usage

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>().IgnoreTrackingFields();
}
```

`IgnoreTrackingFields` excludes `IsChanged`, `IsGraphChanged`, `ShouldTrackChanges`, and `OriginalValues` from the store model.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
