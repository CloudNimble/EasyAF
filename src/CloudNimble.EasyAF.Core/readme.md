# EasyAF.Core

Observable objects, identity, intervals, and shared primitives for EasyAF applications.

## Install

```bash
dotnet add package EasyAF.Core
```

## Usage

```csharp
using CloudNimble.EasyAF.Core;

public class Person : EasyObservableObject
{
    private string _name;

    public string Name
    {
        get => _name;
        set => Set(nameof(Name), ref _name, value);
    }
}
```

`EasyObservableObject` implements `INotifyPropertyChanged` with strongly typed `Set` helpers. The package also includes `IIdentifiable<T>`, audit interfaces, `Interval` / `MoneyInterval`, and `Ensure` argument guards.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
