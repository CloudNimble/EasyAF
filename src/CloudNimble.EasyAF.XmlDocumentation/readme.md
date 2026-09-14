# EasyAF.XmlDocumentation

Parses compiler-generated XML documentation files into typed members, summaries, remarks, and examples.

## Install

```bash
dotnet add package EasyAF.XmlDocumentation
```

## Usage

```csharp
var docs = new AssemblyXmlDocumentation(XDocument.Load("CloudNimble.EasyAF.Core.xml"));
var type = docs.Types["T:CloudNimble.EasyAF.Core.EasyObservableObject"];
```

Used by the EasyAF docs pipeline (`dotnet easyaf mintlify`) to turn XML comments into Mintlify pages.

## Documentation

- Docs: https://easyaf.dev
- Source: https://github.com/CloudNimble/EasyAF

## License

MIT © CloudNimble, Inc.
