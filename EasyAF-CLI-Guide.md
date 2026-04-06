# EasyAF CLI Guide

The EasyAF CLI provides powerful tools for managing Entity Framework projects with EDMX generation, code scaffolding, and project configuration. This guide covers all available commands and their usage patterns.

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Command Overview](#command-overview)
- [Core Commands](#core-commands)
  - [init - Initialize New Project](#init---initialize-new-project)
  - [setup - Join Existing Project](#setup---join-existing-project)
- [Database Commands](#database-commands)
  - [database generate](#database-generate)
  - [database refresh](#database-refresh)
- [Code Generation Commands](#code-generation-commands)
- [EDMX Commands](#edmx-commands)
- [Documentation Commands](#documentation-commands)
- [Common Workflows](#common-workflows)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

## Installation

Install the EasyAF CLI as a global .NET tool:

```bash
dotnet tool install --global EasyAF.Tools
```

Update to the latest version:

```bash
dotnet tool update --global EasyAF.Tools
```

Verify installation:

```bash
dotnet easyaf --help
```

## Quick Start

### Starting a New EasyAF Project

```bash
# Initialize with SQL Server
dotnet easyaf init \
  --connection-string "Server=localhost;Database=MyApp;Integrated Security=true;" \
  --context-name "MyAppDbContext" \
  --provider SqlServer

# Initialize with PostgreSQL
dotnet easyaf init \
  --connection-string "Host=localhost;Database=myapp;Username=postgres;Password=mypassword" \
  --context-name "MyAppDbContext" \
  --provider PostgreSQL
```

### Joining an Existing EasyAF Project

```bash
# Set up your local development environment
dotnet easyaf setup \
  --connection-string "Server=localhost;Database=MyApp;Integrated Security=true;"

# If multiple contexts exist, specify which one
dotnet easyaf setup \
  --context-name "MyAppDbContext" \
  --connection-string "Server=localhost;Database=MyApp;Integrated Security=true;"
```

## Command Overview

| Command | Purpose | Use Case |
|---------|---------|----------|
| `init` | Initialize new EasyAF project | Setting up EasyAF in a new solution |
| `setup` | Configure existing EasyAF project | New developer joining existing project |
| `database generate` | Generate EDMX from database | Creating/updating data models |
| `database refresh` | Refresh existing EDMX | Updating models after schema changes |
| `code generate` | Generate C# code from EDMX | Creating business logic and APIs |
| `edmx validate` | Validate EDMX files | Ensuring model integrity |
| `mintlify` | Generate documentation | Creating API documentation |

## Core Commands

### `init` - Initialize New Project

The `init` command sets up EasyAF in a new solution, configuring project types, namespaces, and database scaffolding.

#### Basic Usage

```bash
dotnet easyaf init \
  --connection-string "connection-string-or-source" \
  --context-name "MyDbContext" \
  --provider SqlServer
```

#### Options

| Option | Short | Description | Required | Example |
|--------|-------|-------------|----------|---------|
| `--connection-string` | `-c` | Connection string or source reference | ✅ | `"Server=localhost;Database=MyApp;Trusted_Connection=true;"` |
| `--context-name` | `-x` | DbContext class name | ✅ | `"MyAppDbContext"` |
| `--provider` | `-p` | Database provider | ✅ | `SqlServer` or `PostgreSQL` |
| `--solution-folder` | `-s` | Solution directory | ❌ | `"/path/to/solution"` |
| `--dbcontext-namespace` | | DbContext namespace | ❌ | `"MyApp.Data"` |
| `--objects-namespace` | | Entity objects namespace | ❌ | `"MyApp.Core"` |
| `--tables` | `-t` | Specific tables to include | ❌ | `"Users,Products,Orders"` |
| `--exclude-tables` | `-e` | Tables to exclude | ❌ | `"__EFMigrationsHistory,AspNetUsers"` |
| `--no-data-annotations` | | Use Fluent API instead of data annotations | ❌ | |
| `--no-pluralizer` | | Disable entity name pluralization | ❌ | |

#### Connection String Options

The `--connection-string` parameter accepts either:

1. **Actual connection string**: Automatically stored in user secrets
   ```bash
   --connection-string "Server=localhost;Database=MyApp;Integrated Security=true;"
   ```

2. **Source reference**: Points to external configuration
   ```bash
   --connection-string "appsettings.json:ConnectionStrings:DefaultConnection"
   --connection-string "secrets:ConnectionStrings:MyAppConnection"
   --connection-string "environment:DATABASE_URL"
   ```

#### What `init` Does

1. **Discovers project structure**: Finds `.Data` project and analyzes solution
2. **Configures project types**: Sets `<EasyAFProjectType>` for Api, Business, Core, Data projects
3. **Detects namespaces**: Determines common namespace and sets `<EasyAFNamespace>`
4. **Sets up Directory.Build.props**: Centralizes configuration and analyzer references
5. **Manages user secrets**: Securely stores connection strings with centralized UserSecretsId
6. **Creates EDMX configuration**: Generates `{ContextName}.edmx.config` file

#### Examples

**Basic SQL Server setup:**
```bash
dotnet easyaf init \
  --connection-string "Server=localhost;Database=MyApp;Integrated Security=true;" \
  --context-name "MyAppDbContext" \
  --provider SqlServer
```

**PostgreSQL with custom namespaces:**
```bash
dotnet easyaf init \
  --connection-string "Host=localhost;Database=myapp;Username=postgres;Password=mypassword" \
  --context-name "MyAppDbContext" \
  --provider PostgreSQL \
  --dbcontext-namespace "MyCompany.MyApp.Data" \
  --objects-namespace "MyCompany.MyApp.Core"
```

**Include specific tables only:**
```bash
dotnet easyaf init \
  --connection-string "Server=localhost;Database=MyApp;Integrated Security=true;" \
  --context-name "MyAppDbContext" \
  --provider SqlServer \
  --tables "Users,Products,Orders,Categories"
```

**Exclude system tables:**
```bash
dotnet easyaf init \
  --connection-string "Server=localhost;Database=MyApp;Integrated Security=true;" \
  --context-name "MyAppDbContext" \
  --provider SqlServer \
  --exclude-tables "__EFMigrationsHistory,AspNetUsers,AspNetRoles"
```

### `setup` - Join Existing Project

The `setup` command configures your local development environment for an existing EasyAF project.

#### Basic Usage

```bash
dotnet easyaf setup \
  --connection-string "your-local-connection-string"
```

#### Options

| Option | Short | Description | Required | Example |
|--------|-------|-------------|----------|---------|
| `--connection-string` | `-c` | Local connection string | ✅ | `"Server=localhost;Database=MyApp;Integrated Security=true;"` |
| `--context-name` | `-x` | DbContext name (if multiple exist) | ❌ | `"MyAppDbContext"` |
| `--solution-folder` | `-s` | Solution directory | ❌ | `"/path/to/solution"` |
| `--dry-run` | | Show what would be configured | ❌ | |

#### What `setup` Does

1. **Discovers existing configuration**: Finds `*.edmx.config` files in the solution
2. **Analyzes connection string sources**: Determines where connection strings should be stored
3. **Reads UserSecretsId**: Gets the centralized UserSecretsId from Directory.Build.props
4. **Stores local connection string**: Uses the same secret key as the existing configuration
5. **Validates setup**: Ensures the local environment matches the project structure

#### Examples

**Single context project:**
```bash
dotnet easyaf setup \
  --connection-string "Server=localhost;Database=MyApp;Integrated Security=true;"
```

**Multiple context project:**
```bash
dotnet easyaf setup \
  --context-name "MyAppDbContext" \
  --connection-string "Server=localhost;Database=MyApp;Integrated Security=true;"
```

**See what would be configured:**
```bash
dotnet easyaf setup \
  --connection-string "Server=localhost;Database=MyApp;Integrated Security=true;" \
  --dry-run
```

## Database Commands

### `database generate`

Generates EDMX files from database schema using existing configuration.

```bash
dotnet easyaf database generate --context-name "MyAppDbContext"
```

### `database refresh`

Refreshes existing EDMX files with latest database schema changes.

```bash
dotnet easyaf database refresh --context-name "MyAppDbContext"
```

## Code Generation Commands

### `code generate`

Generates C# code (business logic, APIs, etc.) from EDMX files.

```bash
dotnet easyaf code generate --context-name "MyAppDbContext"
```

## EDMX Commands

### `edmx validate`

Validates EDMX files for consistency and correctness.

```bash
dotnet easyaf edmx validate --context-name "MyAppDbContext"
```

## Documentation Commands

### `mintlify`

Converts .NET XML documentation to Mintlify MDX format.

```bash
dotnet easyaf mintlify --input-path "MyApp.xml" --output-path "docs/"
```

## Common Workflows

### Setting Up a New EasyAF Project

1. **Initialize the project:**
   ```bash
   dotnet easyaf init \
     --connection-string "Server=localhost;Database=MyApp;Integrated Security=true;" \
     --context-name "MyAppDbContext" \
     --provider SqlServer
   ```

2. **Generate initial EDMX:**
   ```bash
   dotnet easyaf database generate --context-name "MyAppDbContext"
   ```

3. **Generate code:**
   ```bash
   dotnet easyaf code generate --context-name "MyAppDbContext"
   ```

### Joining an Existing Project

1. **Clone the repository:**
   ```bash
   git clone https://github.com/company/myapp.git
   cd myapp
   ```

2. **Set up local environment:**
   ```bash
   dotnet easyaf setup \
     --connection-string "Server=localhost;Database=MyApp;Integrated Security=true;"
   ```

3. **Generate EDMX files:**
   ```bash
   dotnet easyaf database generate --context-name "MyAppDbContext"
   ```

### Updating After Schema Changes

1. **Refresh EDMX:**
   ```bash
   dotnet easyaf database refresh --context-name "MyAppDbContext"
   ```

2. **Regenerate code:**
   ```bash
   dotnet easyaf code generate --context-name "MyAppDbContext"
   ```

3. **Validate changes:**
   ```bash
   dotnet easyaf edmx validate --context-name "MyAppDbContext"
   ```

## Best Practices

### Project Structure

Ensure your solution follows the EasyAF naming conventions:

```
MyApp/
├── MyApp.Api/          # Web API project
├── MyApp.Business/     # Business logic
├── MyApp.Core/         # Entity models
├── MyApp.Data/         # Data access layer
└── Directory.Build.props
```

### Connection String Management

- **Development**: Use `init` or `setup` to store connection strings in user secrets
- **Production**: Reference external configuration sources:
  ```bash
  --connection-string "appsettings.json:ConnectionStrings:DefaultConnection"
  ```

### Database Providers

- **SQL Server**: Use `SqlServer` provider for Microsoft SQL Server
- **PostgreSQL**: Use `PostgreSQL` provider for PostgreSQL databases
- Both providers support the full range of EasyAF features

### Table Management

- **Include specific tables**: Use `--tables` for focused data models
- **Exclude system tables**: Use `--exclude-tables` for cleaner models
- **Never use both**: `--tables` and `--exclude-tables` are mutually exclusive

## Troubleshooting

### Common Issues

**"No .Data project found"**
- Ensure you have a project ending in `.Data` in your solution
- Use `--solution-folder` to specify the correct directory

**"Multiple contexts found"**
- Use `--context-name` to specify which context to use
- The tool will list available contexts in the error message

**"No UserSecretsId found"**
- Run `dotnet easyaf init` first to properly initialize the project
- Check that `Directory.Build.props` exists and contains `<UserSecretsId>`

**"Connection string source not found"**
- Verify the connection string source format is correct
- For existing projects, ensure the `.edmx.config` file exists

### Getting Help

**Command-specific help:**
```bash
dotnet easyaf init --help
dotnet easyaf setup --help
dotnet easyaf database generate --help
```

**Global help:**
```bash
dotnet easyaf --help
```

### Debug Information

**Use dry-run mode:**
```bash
dotnet easyaf setup --dry-run --connection-string "..."
```

**Check configuration files:**
- `Directory.Build.props` - Contains EasyAF configuration
- `*.edmx.config` - Contains database scaffolding settings
- User secrets storage - Contains connection strings

---

## Support

For issues, feature requests, or contributions, visit the [EasyAF GitHub repository](https://github.com/CloudNimble/EasyAF).

For documentation and examples, see the [EasyAF Documentation](https://docs.easyaf.cloud).