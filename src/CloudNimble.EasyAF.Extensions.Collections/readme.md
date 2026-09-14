# EasyAF.Extensions.Collections

Bulk add/insert/remove for `Collection<T>` and `ObservableCollection<T>` (C# 14 extension members).

## Install

```bash
dotnet add package EasyAF.Extensions.Collections
```

## Usage

```csharp
var items = new ObservableCollection<string>();
items.AddRange(["alpha", "beta", "gamma"]);
```

`ObservableCollection<T>` overloads raise a single change notification. `Collection<T>` overloads call `InsertItem` / `RemoveItem` so subclass validation still runs.

Targets .NET 10.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
