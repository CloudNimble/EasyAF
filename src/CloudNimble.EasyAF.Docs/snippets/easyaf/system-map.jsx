// Interactive map of the EasyAF stack for the home page (index.mdx). Styles live in /home.css.
export const SystemMap = () => {
  const [mode, setMode] = React.useState('person');
  const [selected, setSelected] = React.useState(null);

  const nodes = {
    blazor: {
      name: 'Blazor UI',
      lib: 'BlazorEssentials',
      text: 'The front end people use. BlazorEssentials adds client-side databases for offline-capable, local-first Blazor apps, and the app reaches your data through the API.',
      href: '/blazoressentials/guides',
      go: 'Read the Blazor UI guides',
      links: ['api']
    },
    agent: {
      name: 'AI agent',
      lib: 'OData MCP',
      text: 'OData MCP points an agent at your live OData API. MCP Essentials and ClaudeEssentials cover MCP servers and Claude Code hooks.',
      href: 'https://si.nimbleapps.cloud',
      go: 'Open the SuperIntelligence docs',
      links: ['api']
    },
    api: {
      name: 'REST API',
      lib: 'EasyAF.Restier',
      text: 'A Restier host that exposes your entities over OData. Browse $metadata and query collections the same way you would any Restier API.',
      href: '/restier',
      go: 'Read the REST API docs',
      links: ['blazor', 'agent', 'managers', 'data', 'events']
    },
    managers: {
      name: 'Managers',
      lib: 'EasyAF.Business',
      text: 'One manager per entity. Managers wrap create, update, and delete, fill audit fields, and give you lifecycle hooks for your own rules. Call them from the API or from another manager.',
      href: '/guides/business-layer',
      go: 'Read the business layer guide',
      links: ['api', 'data', 'events', 'core']
    },
    events: {
      name: 'Async events',
      lib: 'SimpleMessageBus',
      text: 'The API and every manager receive a message publisher. Publish a message during a request and a handler picks it up afterwards on the message bus.',
      href: '/simplemessagebus',
      go: 'Read the async events docs',
      links: ['api', 'managers']
    },
    data: {
      name: 'Data',
      lib: 'EasyAF.Data.EFCore',
      text: 'An Entity Framework DbContext generated for your schema. The API and the managers both work through it. EF Core and EF6 each have their own package.',
      href: '/guides/table-design',
      go: 'Read the table design guide',
      links: ['api', 'managers', 'database', 'core']
    },
    database: {
      name: 'Database',
      lib: 'SQL Server or PostgreSQL',
      text: 'Your schema is the starting point. The CLI reads it and generates the layers above it.',
      href: '/quickstart',
      go: 'Start the quickstart',
      links: ['data', 'cli']
    },
    cli: {
      name: 'CLI and code generation',
      lib: 'dotnet easyaf',
      text: 'Scaffolds the solution, then generates the entities, the DbContext, a manager per entity, and the API surface from your database.',
      href: '/quickstart',
      go: 'Start the quickstart',
      links: ['database', 'core', 'data', 'managers', 'api']
    },
    core: {
      name: 'Core',
      lib: 'EasyAF.Core',
      text: 'Your entities and the interfaces they compose, such as IIdentifiable<T> and ICreatedAuditable, plus observable objects and intervals. It works on its own in any .NET project.',
      href: '/guides',
      go: 'Read the guides',
      links: ['cli', 'data', 'managers']
    },
    testing: {
      name: 'Testing',
      lib: 'Breakdance',
      text: 'The scaffolded solution includes Breakdance test projects. Breakdance tests against real infrastructure instead of mocks.',
      href: '/breakdance',
      go: 'Read the testing docs',
      links: ['api', 'managers']
    },
    xml: {
      name: 'XML',
      lib: 'XmlEssentials',
      text: 'A standalone XML library shaped like System.Text.Json: UTF-8 first, source generated, and Native AOT ready. Use it anywhere you read or write XML.',
      href: '/xmlessentials',
      go: 'Read the XML docs',
      links: []
    }
  };

  const paths = {
    person: {
      label: 'A person uses your app',
      title: 'From a click to the database',
      nodes: ['blazor', 'api', 'managers', 'events', 'data', 'database'],
      steps: [
        'A Blazor app calls your API over HTTP.',
        'Restier exposes your entities over OData, so the app can query them directly.',
        'Managers hold your rules. They wrap create, update, and delete and fill the audit fields.',
        'The DbContext reads and writes SQL Server or PostgreSQL.',
        'The API and the managers can publish a message, so slow work runs on the message bus instead of in the request.'
      ]
    },
    agent: {
      label: 'An agent uses your app',
      title: 'From a prompt to the database',
      nodes: ['agent', 'api', 'managers', 'events', 'data', 'database'],
      steps: [
        'OData MCP points an AI agent at your live OData API.',
        'The agent queries the same Restier API a person’s app would use.',
        'Managers hold your rules. They wrap create, update, and delete and fill the audit fields.',
        'The DbContext reads and writes SQL Server or PostgreSQL.',
        'The API and the managers can publish a message, so slow work runs on the message bus instead of in the request.'
      ]
    }
  };

  const around = ['cli', 'core', 'testing', 'xml'];
  const path = paths[mode];
  const current = selected ? nodes[selected] : null;

  const isLinked = (id) =>
    selected !== null &&
    id !== selected &&
    (nodes[selected].links.includes(id) || nodes[id].links.includes(selected));

  const isOn = (id) => (selected ? id === selected || isLinked(id) : path.nodes.includes(id));

  const linkedIds = selected ? Object.keys(nodes).filter(isLinked) : [];

  const nodeClass = (id) => {
    if (id === selected) return 'eaf-node is-selected';
    if (isOn(id)) return 'eaf-node is-on';
    if (!selected && around.includes(id)) return 'eaf-node';
    return 'eaf-node is-dim';
  };

  const wireClass = (base, a, b) => (isOn(a) && isOn(b) ? base + ' is-on' : base);

  const pickMode = (next) => {
    setMode(next);
    setSelected(null);
  };

  const renderNode = (id) => (
    <button
      type="button"
      className={nodeClass(id)}
      aria-pressed={id === selected}
      onClick={() => setSelected(id === selected ? null : id)}
    >
      <span className="eaf-node-name">{nodes[id].name}</span>
      <span className="eaf-node-lib">{nodes[id].lib}</span>
    </button>
  );

  return (
    <div className="eaf-map" data-mode={mode}>
      <div className="eaf-toggle" role="group" aria-label="Who is using the app">
        {Object.keys(paths).map((key) => (
          <button
            key={key}
            type="button"
            className="eaf-toggle-btn"
            aria-pressed={mode === key}
            onClick={() => pickMode(key)}
          >
            {paths[key].label}
          </button>
        ))}
      </div>

      <div className="eaf-map-body">
        <div>
          <div className="eaf-stack">
            <div className="eaf-clients" style={{ gridRow: 1 }}>
              {renderNode('blazor')}
              {renderNode('agent')}
            </div>
            <div className="eaf-wires" style={{ gridRow: 2 }}>
              <div className={wireClass('eaf-wire', 'blazor', 'api')} />
              <div className={wireClass('eaf-wire', 'agent', 'api')} />
            </div>
            <div className="eaf-main" style={{ gridRow: 3 }}>{renderNode('api')}</div>
            <div className={wireClass('eaf-hwire', 'api', 'events')} style={{ gridRow: 3 }} />
            <div className={wireClass('eaf-wire', 'api', 'managers')} style={{ gridRow: 4 }} />
            <div className="eaf-main" style={{ gridRow: 5 }}>{renderNode('managers')}</div>
            <div className={wireClass('eaf-hwire', 'managers', 'events')} style={{ gridRow: 5 }} />
            <div className="eaf-side">{renderNode('events')}</div>
            <div className={wireClass('eaf-wire', 'managers', 'data')} style={{ gridRow: 6 }} />
            <div className="eaf-main" style={{ gridRow: 7 }}>{renderNode('data')}</div>
            <div className={wireClass('eaf-wire', 'data', 'database')} style={{ gridRow: 8 }} />
            <div className="eaf-main" style={{ gridRow: 9 }}>{renderNode('database')}</div>
          </div>

          <div className="eaf-around">
            <p className="eaf-around-label">Around the stack</p>
            <div className="eaf-around-grid">
              {renderNode('cli')}
              {renderNode('core')}
              {renderNode('testing')}
              {renderNode('xml')}
            </div>
          </div>
        </div>

        <div className="eaf-detail" aria-live="polite">
          {current ? (
            <div>
              <h3 className="eaf-detail-title">{current.name}</h3>
              <p className="eaf-detail-lib">{current.lib}</p>
              <p className="eaf-detail-text">{current.text}</p>
              {linkedIds.length > 0 && (
                <div>
                  <p className="eaf-detail-label">Works with</p>
                  <div className="eaf-chips">
                    {linkedIds.map((id) => (
                      <button key={id} type="button" className="eaf-chip" onClick={() => setSelected(id)}>
                        {nodes[id].name}
                      </button>
                    ))}
                  </div>
                </div>
              )}
              <div className="eaf-detail-actions">
                <a className="eaf-detail-link" href={current.href}>{current.go}</a>
                <button type="button" className="eaf-detail-reset" onClick={() => setSelected(null)}>
                  Show the whole path
                </button>
              </div>
            </div>
          ) : (
            <div>
              <h3 className="eaf-detail-title">{path.title}</h3>
              <ol className="eaf-detail-steps">
                {path.steps.map((step) => (
                  <li key={step}>{step}</li>
                ))}
              </ol>
              <p className="eaf-detail-label">Select any part to see what it does.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
