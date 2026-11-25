// "Next to the code you have today" on the home page (index.mdx).
// Pick an API you use now and see the same task in XmlEssentials, side by side.
// The code comes from the migration guides in /guides/migrate.
export const ComingFrom = () => {
  const [active, setActive] = React.useState('xmlserializer');
  const [base, setBase] = React.useState('');

  // The home page is served at "/" on its own site and at "/xmlessentials" inside the
  // EasyAF docs, so guide links are built from wherever this page actually lives.
  React.useEffect(() => {
    setBase(window.location.pathname.replace(/\/(index)?$/, ''));
  }, []);

  const sources = [
    {
      id: 'xmlserializer',
      label: 'XmlSerializer',
      oldName: 'System.Xml.Serialization',
      summary: 'The two serializers share a class name and most attribute names, so most mapped types need a different using directive and little else. What changes is the call.',
      before: String.raw`using System.Xml.Serialization;

var serializer = new XmlSerializer(typeof(PurchaseOrder));

string xml;
using (var writer = new StringWriter())
{
    serializer.Serialize(writer, order);
    xml = writer.ToString();
}

PurchaseOrder? copy;
using (var reader = new StringReader(xml))
{
    copy = (PurchaseOrder?)serializer.Deserialize(reader);
}`,
      after: String.raw`using CloudNimble.Text.Xml;

var options = new XmlSerializerOptions { WriteIndented = true, NewLine = "\n" };
string xml = XmlSerializer.Serialize(order, options);

PurchaseOrder? copy = XmlSerializer.Deserialize<PurchaseOrder>(xml, options);`,
      changes: [
        'There\u2019s no serializer object to build or cache. You call a static generic method.',
        'The defaults differ: no `xmlns:xsi` or `xmlns:xsd` on the root, no indentation unless you set `WriteIndented`, and collections are written without a wrapper element.',
        'Reading and writing objects is 1.8\u00d7 to 5.9\u00d7 faster in the benchmarks.'
      ],
      guide: '/guides/migrate/from-xmlserializer',
      guideLabel: 'Migrate from XmlSerializer'
    },
    {
      id: 'readerwriter',
      label: 'XmlReader and XmlWriter',
      oldName: 'System.Xml',
      summary: 'Utf8XmlReader and Utf8XmlWriter are the XML counterparts of Utf8JsonReader and Utf8JsonWriter. This is a shortened version of the writer pair in the guide.',
      before: String.raw`var settings = new XmlWriterSettings
{
    Indent = true,
    NewLineChars = "\n",
    Encoding = new UTF8Encoding(false)
};

using (var writer = XmlWriter.Create(stream, settings))
{
    writer.WriteStartDocument();
    writer.WriteStartElement("inv", "Inventory", ns);

    writer.WriteStartElement("Price", ns);
    writer.WriteValue(49.90m);
    writer.WriteEndElement();

    writer.WriteEndElement();
    writer.WriteEndDocument();
    writer.Flush();
}`,
      after: String.raw`var options = new XmlWriterOptions
{
    Indented = true,
    NewLine = "\n"
};

using (var writer = new Utf8XmlWriter(stream, options))
{
    writer.WriteStartElement("inv", "Inventory", ns);

    writer.WriteStartElement("Price", ns);
    writer.WriteNumberValue(49.90m);
    writer.WriteEndElement();

    writer.WriteEndElement();
    writer.Flush();
}`,
      changes: [
        'You construct the writer. There\u2019s no `Create` factory.',
        '`WriteStartDocument` and `WriteEndDocument` are gone. The XML declaration is written for you.',
        '`WriteValue` becomes `WriteNumberValue`, `WriteBooleanValue`, or `WriteStringValue`, as in `Utf8JsonWriter`.',
        '`Utf8XmlReader` reads real documents 1.5\u00d7 to 2.3\u00d7 faster than `XmlReader` in the benchmarks.'
      ],
      guide: '/guides/migrate/from-xmlreader-xmlwriter',
      guideLabel: 'Migrate from XmlReader and XmlWriter'
    },
    {
      id: 'xdocument',
      label: 'XDocument',
      oldName: 'LINQ to XML',
      summary: 'XmlEssentials has two document models: a read-only XmlDocument for loading and reading, and a mutable XmlNode tree for building and changing. This pair builds a document and saves it.',
      before: String.raw`var order = new XElement("Order",
    new XAttribute("id", 1042),
    new XElement("Item", "Keyboard"),
    new XElement("Item", "Mouse"));

using var stream = new MemoryStream();
new XDocument(order).Save(stream);`,
      after: String.raw`var order = new XmlElementNode("Order");
order.SetAttribute("id", "1042");
order.AddElement("Item").Add(new XmlTextNode("Keyboard"));
order.AddElement("Item").Add(new XmlTextNode("Mouse"));

using var stream = new MemoryStream();
var options = new XmlWriterOptions { Indented = true, NewLine = "\n" };

using (var writer = new Utf8XmlWriter(stream, options))
{
    order.WriteTo(writer);
}`,
      changes: [
        'There is no functional construction. You create a node and then add to it.',
        'Attribute values and text are strings. LINQ to XML converts `1042` for you; here you write `"1042"`.',
        'LINQ axis methods and XPath have no direct replacement. You write a loop, or use LINQ to Objects over `Children`.',
        'The read-only `XmlDocument` parses real documents 3.1\u00d7 to 3.4\u00d7 faster than `System.Xml.XmlDocument`. `XDocument` is still faster at parsing, changing, and writing a whole document.'
      ],
      guide: '/guides/migrate/from-xdocument',
      guideLabel: 'Migrate from XDocument and XmlDocument'
    },
    {
      id: 'json',
      label: 'System.Text.Json',
      oldName: 'System.Text.Json',
      summary: 'You don\u2019t replace JsonSerializer. You use the serializer that matches the format on the wire, with the same model types and the same option names.',
      before: String.raw`var options = new JsonSerializerOptions
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    NewLine = "\n",
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
};

string json = JsonSerializer.Serialize(item, options);

CatalogItem? copy = JsonSerializer.Deserialize<CatalogItem>(json, options);`,
      after: String.raw`var options = new XmlSerializerOptions
{
    DefaultIgnoreCondition = XmlIgnoreCondition.WhenWritingNull,
    NewLine = "\n",
    PropertyNamingPolicy = XmlNamingPolicy.CamelCase,
    WriteIndented = true,
};

string xml = XmlSerializer.Serialize(item, options);

CatalogItem? copy = XmlSerializer.Deserialize<CatalogItem>(xml, options);`,
      changes: [
        'Most of what you know carries over under a different prefix.',
        'The document starts with a declaration and the object gets a root element name.',
        'A list is a repeated element. XML has no arrays.'
      ],
      guide: '/guides/migrate/from-system-text-json',
      guideLabel: 'Coming from System.Text.Json'
    }
  ];

  const current = sources.find((source) => source.id === active);

  // A small C# highlighter: comments, strings, keywords, type names, and numbers.
  const tokenPattern = /(\/\/[^\n]*|"(?:\\.|[^"\\])*"|\b(?:using|var|new|string|typeof|true|false|null)\b|\b[A-Z][A-Za-z0-9_]*\b|\b\d+(?:\.\d+)?m?\b)/g;

  const tokenClass = (token) => {
    if (token.startsWith('//')) return 'xe-tok-comment';
    if (token.startsWith('"')) return 'xe-tok-string';
    if (/^\d/.test(token)) return 'xe-tok-number';
    if (/^[A-Z]/.test(token)) return 'xe-tok-type';
    return 'xe-tok-keyword';
  };

  const highlight = (code) =>
    code.split(tokenPattern).map((part, index) =>
      index % 2 === 1 ? <span key={index} className={tokenClass(part)}>{part}</span> : part
    );

  // Renders `backticked` words in a sentence as inline code.
  const inline = (text) =>
    text.split('`').map((part, index) =>
      index % 2 === 1 ? <code key={index} className="xe-inline">{part}</code> : part
    );

  const lineCount = (code) => code.split('\n').length;

  return (
    <div className="xe-from">
      <style>{`
        .xe-from {
          --xe-accent: rgb(var(--primary, 65 154 197));
          --xe-ink: #111827;
          --xe-muted: #4b5563;
          --xe-line: rgba(17, 24, 39, 0.14);
          --xe-surface: rgba(17, 24, 39, 0.03);
          --xe-code-bg: #0d1117;
          --xe-code-ink: #e6edf3;
          margin: 1.5rem 0 2rem;
          color: var(--xe-ink);
        }
        html.dark .xe-from {
          --xe-accent: rgb(var(--primary-light, 65 154 197));
          --xe-ink: #f3f4f6;
          --xe-muted: #a3acb9;
          --xe-line: rgba(243, 244, 246, 0.16);
          --xe-surface: rgba(243, 244, 246, 0.04);
        }
        .xe-from button { font: inherit; color: inherit; cursor: pointer; }
        .xe-from :focus-visible { outline: 2px solid var(--xe-accent); outline-offset: 2px; }
        .xe-picks {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(9.5rem, 1fr));
          gap: 0.5rem;
        }
        .xe-pick {
          display: flex;
          flex-direction: column;
          gap: 0.1rem;
          padding: 0.7rem 0.85rem;
          text-align: left;
          border: 1px solid var(--xe-line);
          border-bottom: 3px solid var(--xe-line);
          border-radius: 6px;
          background: var(--xe-surface);
          transition: border-color 0.15s ease, background-color 0.15s ease;
        }
        .xe-pick:hover { border-color: var(--xe-accent); }
        .xe-pick[aria-pressed="true"] { border-color: var(--xe-accent); border-bottom-color: var(--xe-accent); }
        .xe-pick-from { font-size: 0.75rem; color: var(--xe-muted); }
        .xe-pick-name { font-weight: 650; font-size: 0.95rem; line-height: 1.25; }
        .xe-summary { margin: 1.1rem 0 0; color: var(--xe-muted); }
        .xe-panes {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(19rem, 1fr));
          gap: 0.75rem;
          margin-top: 1rem;
        }
        .xe-pane {
          display: flex;
          flex-direction: column;
          min-width: 0;
          border: 1px solid var(--xe-line);
          border-radius: 8px;
          overflow: hidden;
          background: var(--xe-code-bg);
        }
        .xe-pane-ours { border-color: var(--xe-accent); }
        .xe-pane-head {
          display: flex;
          justify-content: space-between;
          gap: 0.75rem;
          padding: 0.5rem 0.85rem;
          font-size: 0.8rem;
          font-weight: 600;
          color: #9aa4b2;
          border-bottom: 1px solid rgba(255, 255, 255, 0.1);
        }
        .xe-pane-ours .xe-pane-head { color: var(--xe-accent); }
        .xe-pane-lines { font-weight: 400; white-space: nowrap; }
        .xe-code {
          flex: 1 1 auto;
          margin: 0;
          padding: 0.85rem;
          overflow-x: auto;
          font-family: "Cascadia Code", ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
          font-size: 0.78rem;
          line-height: 1.6;
          color: var(--xe-code-ink);
          background: transparent;
          white-space: pre;
          tab-size: 4;
        }
        .xe-tok-keyword { color: #ff7b72; }
        .xe-tok-type { color: #79c0ff; }
        .xe-tok-string { color: #a5d6ff; }
        .xe-tok-number { color: #f2cc60; }
        .xe-tok-comment { color: #8b949e; }
        .xe-changes-title { margin: 1.25rem 0 0.5rem; font-weight: 650; font-size: 0.95rem; }
        .xe-changes { color: var(--xe-muted); }
        .xe-change { position: relative; margin-top: 0.4rem; padding-left: 1.1rem; }
        .xe-change::before { content: ""; position: absolute; left: 0.15rem; top: 0.62em; width: 0.35rem; height: 0.35rem; border-radius: 50%; background: var(--xe-accent); }
        .xe-inline {
          font-family: "Cascadia Code", ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
          font-size: 0.85em;
          padding: 0.1em 0.35em;
          border-radius: 4px;
          background: var(--xe-surface);
          border: 1px solid var(--xe-line);
          color: var(--xe-ink);
        }
        .xe-guide {
          display: inline-block;
          margin-top: 1.1rem;
          font-weight: 650;
          color: var(--xe-accent);
          text-decoration: underline;
          text-underline-offset: 3px;
        }
        @media (prefers-reduced-motion: reduce) {
          .xe-pick { transition: none; }
        }
      `}</style>

      <div className="xe-picks" role="group" aria-label="What you use today">
        {sources.map((source) => (
          <button
            key={source.id}
            type="button"
            className="xe-pick"
            aria-pressed={source.id === active}
            onClick={() => setActive(source.id)}
            onMouseEnter={() => setActive(source.id)}
            onFocus={() => setActive(source.id)}
          >
            <span className="xe-pick-from">Coming from</span>
            <span className="xe-pick-name">{source.label}</span>
          </button>
        ))}
      </div>

      <div aria-live="polite">
        <p className="xe-summary">{current.summary}</p>

        <div className="xe-panes">
          <div className="xe-pane">
            <div className="xe-pane-head">
              <span>{current.oldName}</span>
              <span className="xe-pane-lines">{lineCount(current.before)} lines</span>
            </div>
            <div className="xe-code">{highlight(current.before)}</div>
          </div>
          <div className="xe-pane xe-pane-ours">
            <div className="xe-pane-head">
              <span>XmlEssentials</span>
              <span className="xe-pane-lines">{lineCount(current.after)} lines</span>
            </div>
            <div className="xe-code">{highlight(current.after)}</div>
          </div>
        </div>

        <p className="xe-changes-title">What changes</p>
        <div className="xe-changes">
          {current.changes.map((change) => (
            <div key={change} className="xe-change">{inline(change)}</div>
          ))}
        </div>

        <a className="xe-guide" href={base + current.guide}>{current.guideLabel}</a>
      </div>
    </div>
  );
};
