using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.EFCoreToEdmx
{

    /// <summary>
    /// Scaffolds Entity Framework Core DbContext and entities from database schemas.
    /// Enhanced with OnModelCreating method extraction capabilities.
    /// </summary>
    public partial class DatabaseScaffolder
    {
        /// <summary>
        /// List of system tables that should never be scaffolded.
        /// </summary>
        private static readonly HashSet<string> SystemTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "__EFMigrationsHistory",
            "sysdiagrams",
            
            // PostgreSQL system schemas (tables within these schemas)
            "pg_catalog",
            "pg_toast",
            "pg_temp",
            "information_schema",
            
            // Common PostgreSQL system tables
            "pg_stat_statements",
            "pg_stat_activity"
        };

        /// <summary>
        /// Represents the result of database scaffolding including the OnModelCreating method.
        /// </summary>
        public class ScaffoldingResult
        {
            /// <summary>
            /// Gets or sets the scaffolded DbContext instance.
            /// </summary>
            public DbContext Context { get; set; }

            /// <summary>
            /// Gets or sets the cleanup action to dispose resources.
            /// </summary>
            public Action Cleanup { get; set; }

            /// <summary>
            /// Gets or sets the extracted OnModelCreating method body.
            /// </summary>
            public string OnModelCreatingBody { get; set; } = string.Empty;
        }

        /// <summary>
        /// Scaffolds a DbContext from the database using the specified configuration.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="config">The EDMX configuration containing scaffolding options.</param>
        /// <returns>A <see cref="ScaffoldingResult"/> containing the context, cleanup action, and OnModelCreating method.</returns>
        /// <exception cref="ArgumentException">Thrown when parameters are null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when scaffolding fails or the provider is unsupported.</exception>
        public async Task<ScaffoldingResult> ScaffoldFromDatabaseAsync(string connectionString, EdmxConfig config)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));
            ArgumentNullException.ThrowIfNull(config, nameof(config));

            var tempDirectory = Path.Combine(Path.GetTempPath(), "EasyAF_Scaffold_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(tempDirectory);

            try
            {
                // Create the scaffolding services
                var services = CreateScaffoldingServices(config.Provider, connectionString);
                var scaffolder = services.GetRequiredService<IReverseEngineerScaffolder>();

                // Determine namespaces with fallbacks
                var contextNamespace = string.IsNullOrWhiteSpace(config.DbContextNamespace) ? "TempScaffold" : config.DbContextNamespace;
                var modelNamespace = string.IsNullOrWhiteSpace(config.ObjectsNamespace) ? "TempScaffold" : config.ObjectsNamespace;

                // Configure scaffolding options
                var options = new ReverseEngineerOptions
                {
                    ConnectionString = connectionString,
                    ContextName = config.ContextName,
                    ContextNamespace = contextNamespace,
                    ModelNamespace = modelNamespace,
                    NoPluralize = !config.UsePluralizer,
                    UseDataAnnotations = config.UseDataAnnotations,
                    OverwriteFiles = true,
                    UseDatabaseNames = true  // Preserve database column names to generate HasColumnName() calls
                };

                // Apply table filters
                ApplyTableFilters(options, config);

                Console.WriteLine($"Scaffolding with DbContext namespace: {contextNamespace}");
                Console.WriteLine($"Scaffolding with Objects namespace: {modelNamespace}");

                // Generate the code
                var scaffoldedModel = scaffolder.ScaffoldModel(
                    connectionString,
                    new DatabaseModelFactoryOptions(options.Tables, options.Schemas),
                    new ModelReverseEngineerOptions
                    {
                        NoPluralize = options.NoPluralize,
                        UseDatabaseNames = true  // Ensure database names are preserved in the model
                    },
                    new ModelCodeGenerationOptions
                    {
                        UseDataAnnotations = options.UseDataAnnotations,
                        Language = "C#",
                        ContextName = options.ContextName,
                        ContextNamespace = options.ContextNamespace,
                        ModelNamespace = options.ModelNamespace,
                        SuppressConnectionStringWarning = true,
                        SuppressOnConfiguring = false  // We WANT OnConfiguring so we get a parameterless constructor
                    }
                );

                // Extract OnModelCreating method before modifying the context
                var rawOnModelCreating = ExtractOnModelCreatingMethod(scaffoldedModel.ContextFile.Code);
                
                // Enhance the OnModelCreating with IgnoreTrackingFields and HasColumnName calls
                var onModelCreatingBody = EnhanceOnModelCreating(rawOnModelCreating, config.PropertyNameOverrides);

                Console.WriteLine("Enhanced OnModelCreating method");
                if (!string.IsNullOrWhiteSpace(onModelCreatingBody))
                {
                    var lineCount = onModelCreatingBody.Split('\n').Length;
                    Console.WriteLine($"OnModelCreating method contains {lineCount} lines");
                }
                else
                {
                    Console.WriteLine("No OnModelCreating method found or extraction failed");
                }

                // Write the generated code to temp files, but modify the DbContext to include OnConfiguring
                var contextPath = Path.Combine(tempDirectory, $"{config.ContextName}.cs");
                var modifiedContextCode = AddOnConfiguringToDbContext(scaffoldedModel.ContextFile.Code, config.Provider, connectionString);
                await File.WriteAllTextAsync(contextPath, modifiedContextCode);

                foreach (var entityFile in scaffoldedModel.AdditionalFiles)
                {
                    var entityPath = Path.Combine(tempDirectory, entityFile.Path);
                    await File.WriteAllTextAsync(entityPath, entityFile.Code);
                }

                // Compile and load the assembly
                var assembly = await CompileScaffoldedCodeAsync(tempDirectory, scaffoldedModel, config, modifiedContextCode);

                // Create an instance of the DbContext (now uses parameterless constructor with OnConfiguring)
                var context = CreateDbContextInstance(assembly, config, connectionString);

                // Return the context and cleanup action
                Action cleanup = () =>
                {
                    context?.Dispose();
                    try
                    {
                        if (Directory.Exists(tempDirectory))
                            Directory.Delete(tempDirectory, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                };

                return new ScaffoldingResult
                {
                    Context = context,
                    Cleanup = cleanup,
                    OnModelCreatingBody = onModelCreatingBody
                };
            }
            catch
            {
                // Clean up on error
                try
                {
                    if (Directory.Exists(tempDirectory))
                        Directory.Delete(tempDirectory, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }

                throw;
            }
        }

        /// <summary>
        /// Extracts the complete OnModelCreating method from the generated DbContext code using Roslyn.
        /// </summary>
        /// <param name="contextCode">The generated DbContext source code.</param>
        /// <returns>The complete OnModelCreating method as a string, or empty string if not found.</returns>
        private static string ExtractOnModelCreatingMethod(string contextCode)
        {
            try
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(contextCode);
                var root = syntaxTree.GetRoot();

                // Find the class declaration
                var classDeclaration = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => c.BaseList?.Types.Any(t => t.ToString().Contains("DbContext")) == true);

                if (classDeclaration is null)
                {
                    Console.WriteLine("Could not find DbContext class declaration.");
                    return string.Empty;
                }

                // Find the OnModelCreating method
                var onModelCreatingMethod = classDeclaration.Members
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(m => m.Identifier.ValueText == "OnModelCreating");

                if (onModelCreatingMethod is null)
                {
                    Console.WriteLine("Could not find OnModelCreating method.");
                    return string.Empty;
                }

                // Extract the complete method including signature and braces
                var completeMethod = onModelCreatingMethod.ToString();
                
                Console.WriteLine($"Extracted raw OnModelCreating method ({completeMethod.Split('\n').Length} lines)");

                return completeMethod;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting OnModelCreating method: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Fixes the OnModelCreating method formatting by adjusting indentation and improving navigation property naming.
        /// </summary>
        /// <param name="onModelCreatingMethod">The raw OnModelCreating method from scaffolding.</param>
        /// <returns>The formatted method with proper indentation and improved naming.</returns>
        /// <remarks>
        /// This method:
        /// 1. Adds 4 spaces of indentation to all non-blank lines for proper code formatting
        /// 2. Replaces "InverseParent" with "Children" for better self-referencing relationship naming
        /// 3. Reduces excessive consecutive blank lines to single blank lines
        /// </remarks>
        private static string FixOnModelCreatingFormatting(string onModelCreatingMethod)
        {
            if (string.IsNullOrWhiteSpace(onModelCreatingMethod))
                return onModelCreatingMethod;

            // Fix self-referencing relationship naming: InverseParent → Children
            onModelCreatingMethod = onModelCreatingMethod.Replace(".InverseParent)", ".Children)").Replace("p.InverseParent", "p.Children");

            // Normalize line endings to avoid Windows \r\n issues
            onModelCreatingMethod = onModelCreatingMethod.Replace("\r\n", "\n").Replace("\r", "\n");

            // Split on normalized line endings
            var lines = onModelCreatingMethod.Split('\n');
            var processedLines = new List<string>();
            var consecutiveBlankLines = 0;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    consecutiveBlankLines++;
                    // Only add blank line if it's the first consecutive blank line
                    if (consecutiveBlankLines == 1)
                    {
                        processedLines.Add(string.Empty);
                    }
                }
                else
                {
                    consecutiveBlankLines = 0;
                    // Add 4 spaces to non-blank lines
                    processedLines.Add("    " + line);
                }
            }

            return string.Join(Environment.NewLine, processedLines);
        }

        /// <summary>
        /// Enhances the OnModelCreating method by adding IgnoreTrackingFields calls and HasColumnName mappings.
        /// </summary>
        /// <param name="onModelCreatingMethod">The raw OnModelCreating method from scaffolding.</param>
        /// <param name="propertyNameOverrides">Optional property name overrides for column mappings.</param>
        /// <returns>The enhanced OnModelCreating method with injected calls.</returns>
        /// <remarks>
        /// This method uses Roslyn to:
        /// 1. Parse the OnModelCreating method
        /// 2. Add IgnoreTrackingFields() call for each entity
        /// 3. Add HasColumnName() calls based on PropertyNameOverrides
        /// 4. Fix self-referencing relationship naming (InverseParent → Children)
        /// 5. Maintain proper indentation and formatting
        /// </remarks>
        private static string EnhanceOnModelCreating(string onModelCreatingMethod, Dictionary<string, Dictionary<string, string>> propertyNameOverrides = null)
        {
            if (string.IsNullOrWhiteSpace(onModelCreatingMethod))
                return onModelCreatingMethod;

            try
            {
                // Parse the method as a complete C# document
                var syntaxTree = CSharpSyntaxTree.ParseText(onModelCreatingMethod);
                var root = syntaxTree.GetRoot();

                // Find the OnModelCreating method
                var methodDeclaration = root.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(m => m.Identifier.ValueText == "OnModelCreating");

                if (methodDeclaration is null || methodDeclaration.Body is null)
                {
                    // Fall back to simple formatting if parsing fails
                    return FixOnModelCreatingFormatting(onModelCreatingMethod);
                }

                var sb = new StringBuilder();
                sb.AppendLine("    protected override void OnModelCreating(ModelBuilder modelBuilder)");
                sb.AppendLine("    {");

                // Get all the statements in the method body
                var statements = methodDeclaration.Body.Statements;

                foreach (var statement in statements)
                {
                    // Get the full statement string (preserves semicolons!)
                    var statementString = statement.ToString();
                    
                    // Check if this is an entity configuration statement
                    if (statement is ExpressionStatementSyntax expressionStatement &&
                        expressionStatement.Expression is InvocationExpressionSyntax invocation)
                    {
                        var invocationString = invocation.ToString();
                        
                        // Check if this is a modelBuilder.Entity<T> call
                        if (invocationString.StartsWith("modelBuilder.Entity<"))
                        {
                            // Extract entity name from the invocation
                            string entityName = null;
                            var startIndex = invocationString.IndexOf('<') + 1;
                            var endIndex = invocationString.IndexOf('>');
                            if (startIndex > 0 && endIndex > startIndex)
                            {
                                entityName = invocationString.Substring(startIndex, endIndex - startIndex);
                            }

                            // Process the full statement (with semicolon)
                            var processedStatement = ProcessEntityStatement(statementString, entityName, propertyNameOverrides);
                            
                            // Add proper indentation (already includes line breaks from processing)
                            sb.Append(processedStatement);
                        }
                        else
                        {
                            // Not an entity configuration, just add it with indentation
                            var lines = statementString.Split('\n');
                            foreach (var line in lines)
                            {
                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    sb.AppendLine("        " + line.Trim());
                                }
                            }
                        }
                    }
                    else
                    {
                        // Other statement types, add with indentation
                        var lines = statementString.Split('\n');
                        foreach (var line in lines)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                sb.AppendLine("        " + line.Trim());
                            }
                        }
                    }
                    
                    // Add a blank line between statements for readability
                    sb.AppendLine();
                }

                sb.AppendLine("    }");
                
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enhancing OnModelCreating with Roslyn: {ex.Message}");
                // Fall back to simple formatting if Roslyn processing fails
                return FixOnModelCreatingFormatting(onModelCreatingMethod);
            }
        }

        /// <summary>
        /// Processes an entity configuration statement to add IgnoreTrackingFields and HasColumnName calls.
        /// </summary>
        /// <param name="statementString">The complete entity configuration statement including semicolon.</param>
        /// <param name="entityName">The name of the entity being configured.</param>
        /// <param name="propertyNameOverrides">Optional property name overrides for column mappings.</param>
        /// <returns>The processed statement with injected calls and proper indentation.</returns>
        private static string ProcessEntityStatement(string statementString, string entityName, Dictionary<string, Dictionary<string, string>> propertyNameOverrides)
        {
            // Fix self-referencing relationship naming
            statementString = statementString.Replace(".InverseParent)", ".Children)").Replace("p.InverseParent", "p.Children");

            // If no entity name, return with basic indentation
            if (string.IsNullOrEmpty(entityName))
            {
                var simpleLines = statementString.Split('\n');
                var sb = new StringBuilder();
                foreach (var line in simpleLines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        sb.AppendLine("        " + line.Trim());
                    }
                }
                return sb.ToString();
            }

            // Check if we have property overrides for this entity
            var entityOverrides = propertyNameOverrides?.ContainsKey(entityName) == true 
                ? propertyNameOverrides[entityName] 
                : null;

            // Split into lines for processing
            var lines = statementString.Split('\n');
            var result = new StringBuilder();
            var ignoreTrackingFieldsAdded = false;
            var currentIndentLevel = 2; // Start with 2 levels (8 spaces)

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Track indent level based on braces
                if (line.Contains('{'))
                {
                    // Add the line with current indentation
                    result.AppendLine(new string(' ', currentIndentLevel * 4) + line);
                    currentIndentLevel++;

                    // Add IgnoreTrackingFields after the opening brace of entity configuration
                    if (!ignoreTrackingFieldsAdded && i > 0 && lines[i - 1].Contains($"modelBuilder.Entity<{entityName}>(entity =>"))
                    {
                        result.AppendLine(new string(' ', currentIndentLevel * 4) + "entity.IgnoreTrackingFields();");
                        result.AppendLine(); // Add blank line for readability
                        ignoreTrackingFieldsAdded = true;
                    }
                }
                else if (line.StartsWith('}'))
                {
                    currentIndentLevel--;
                    result.AppendLine(new string(' ', currentIndentLevel * 4) + line);
                }
                else
                {
                    // Check if this is a property configuration that needs HasColumnName
                    bool hasColumnNameAdded = false;
                    if (entityOverrides != null && line.Contains("entity.Property("))
                    {
                        var propertyMatch = PropertyRegex().Match(line);
                        if (propertyMatch.Success)
                        {
                            var propertyName = propertyMatch.Groups[1].Value;
                            
                            // Check if we have an override for this property
                            foreach (var kvp in entityOverrides)
                            {
                                var dbColumn = kvp.Key;
                                var clrProperty = kvp.Value;
                                
                                if (clrProperty == propertyName)
                                {
                                    // Add the property line first
                                    result.AppendLine(new string(' ', currentIndentLevel * 4) + line);
                                    
                                    // Check if the next lines already have HasColumnName
                                    bool hasColumnNameExists = false;
                                    for (int j = i + 1; j < lines.Length && j < i + 5; j++)
                                    {
                                        var nextLine = lines[j].Trim();
                                        if (nextLine.Contains(".HasColumnName("))
                                        {
                                            hasColumnNameExists = true;
                                            break;
                                        }
                                        if (!nextLine.StartsWith('.'))
                                        {
                                            break;
                                        }
                                    }

                                    // Add HasColumnName if it doesn't exist
                                    if (!hasColumnNameExists)
                                    {
                                        result.AppendLine(new string(' ', (currentIndentLevel + 1) * 4) + $".HasColumnName(\"{dbColumn}\")");
                                    }
                                    
                                    hasColumnNameAdded = true;
                                    break;
                                }
                            }
                        }
                    }

                    // If we didn't add HasColumnName, just add the line normally
                    if (!hasColumnNameAdded)
                    {
                        // Check if this is a continuation line (starts with .)
                        if (line.StartsWith("."))
                        {
                            result.AppendLine(new string(' ', (currentIndentLevel + 1) * 4) + line);
                        }
                        else
                        {
                            result.AppendLine(new string(' ', currentIndentLevel * 4) + line);
                        }
                    }
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Creates the scaffolding services for the specified database provider.
        /// </summary>
        /// <param name="provider">The database provider ("SqlServer" or "PostgreSQL").</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <returns>A configured service provider for scaffolding operations.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<Pending>")]
        private static IServiceProvider CreateScaffoldingServices(string provider, string connectionString)
        {
            var services = new ServiceCollection();

            services.AddEntityFrameworkDesignTimeServices();
            services.AddLogging();

            switch (provider)
            {
                case "SqlServer":
                    services.AddEntityFrameworkSqlServer();
                    // Register SQL Server design-time services
                    new Microsoft.EntityFrameworkCore.SqlServer.Design.Internal.SqlServerDesignTimeServices()
                        .ConfigureDesignTimeServices(services);
                    break;

                case "PostgreSQL":
                    services.AddEntityFrameworkNpgsql();
                    // Register standard Npgsql design-time services first
                    new Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal.NpgsqlDesignTimeServices()
                        .ConfigureDesignTimeServices(services);
                    
                    // Then override with our custom design-time services for enhanced type mapping
                    new CloudNimble.EasyAF.EFCoreToEdmx.PostgreSQL.PostgreSQLDesignTimeServices()
                        .ConfigureDesignTimeServices(services);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported database provider: {provider}");
            }

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Applies table inclusion and exclusion filters to the scaffolding options.
        /// </summary>
        /// <param name="options">The reverse engineering options to configure.</param>
        /// <param name="config">The EDMX configuration containing table filters.</param>
        private static void ApplyTableFilters(ReverseEngineerOptions options, EdmxConfig config)
        {
            // Always exclude system tables
            var tablesToExclude = new HashSet<string>(SystemTables, StringComparer.OrdinalIgnoreCase);

            if (config.IncludedTables?.Count > 0)
            {
                // Include only specified tables (minus system tables)
                // This provides explicit control over which tables to scaffold
                options.Tables = config.IncludedTables
                    .Where(table => !SystemTables.Contains(table))
                    .ToList();

                Console.WriteLine($"Scaffolding {options.Tables.Count} specified tables.");
            }
            else
            {
                // No inclusion list specified - scaffold all tables (minus system tables)
                // This is the default behavior that allows automatic discovery of new tables
                options.Tables = null; // null means "all tables"
                
                Console.WriteLine("Scaffolding all tables (except system tables). This allows automatic discovery of new tables when refreshing.");

                if (config.ExcludedTables?.Count > 0)
                {
                    // Add user-specified exclusions to system table exclusions
                    foreach (var table in config.ExcludedTables)
                    {
                        tablesToExclude.Add(table);
                    }

                    Console.WriteLine($"Note: {config.ExcludedTables.Count} excluded tables specified, but exclusion filtering at the EF Core level is not fully implemented.");
                    Console.WriteLine("Excluded tables may still appear in the scaffolded model and will need to be filtered post-scaffolding.");
                    
                    // Note: EF Core scaffolding doesn't have direct exclusion support
                    // Future enhancement: Implement post-scaffolding filtering to remove excluded tables
                }
            }

            // Fix for PostgreSQL: Ensure schemas is not null (defaults to "public" schema)
            if (options.Schemas is null && config.Provider == "PostgreSQL")
            {
                options.Schemas = new List<string> { "public" };
                Console.WriteLine("PostgreSQL: Set default schema to 'public' to prevent null reference exception.");
            }
        }

        /// <summary>
        /// Modifies the generated DbContext code to include an OnConfiguring override with the connection string.
        /// </summary>
        /// <param name="originalCode">The original generated DbContext code.</param>
        /// <param name="provider">The database provider.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <returns>The modified DbContext code with OnConfiguring override.</returns>
        private static string AddOnConfiguringToDbContext(string originalCode, string provider, string connectionString)
        {
            Console.WriteLine("Modifying DbContext to add OnConfiguring override...");
            
            var lines = originalCode.Split('\n');
            var modifiedLines = new List<string>();
            var onConfiguringAdded = false;
            var inOnConfiguring = false;
            var onConfiguringBraceCount = 0;
            var skipLine = false;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                
                // Check if we're entering an existing OnConfiguring method
                if (line.Contains("protected override void OnConfiguring") || line.Contains("protected virtual void OnConfiguring"))
                {
                    Console.WriteLine("Found existing OnConfiguring method, replacing it...");
                    inOnConfiguring = true;
                    onConfiguringBraceCount = 0;
                    skipLine = true;
                    
                    // Add our own OnConfiguring method instead
                    var providerCall = provider switch
                    {
                        "SqlServer" => $"optionsBuilder.UseSqlServer(@\"{connectionString.Replace("\"", "\"\"")}\");",
                        "PostgreSQL" => $"optionsBuilder.UseNpgsql(@\"{connectionString.Replace("\"", "\"\"")}\");",
                        _ => throw new InvalidOperationException($"Unsupported provider: {provider}")
                    };

                    modifiedLines.Add("        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
                    modifiedLines.Add("        {");
                    modifiedLines.Add("            if (!optionsBuilder.IsConfigured)");
                    modifiedLines.Add("            {");
                    modifiedLines.Add($"                {providerCall}");
                    modifiedLines.Add("            }");
                    modifiedLines.Add("        }");
                    onConfiguringAdded = true;
                    continue;
                }

                // If we're inside an existing OnConfiguring method, skip until we're out
                if (inOnConfiguring)
                {
                    onConfiguringBraceCount += line.Count(c => c == '{') - line.Count(c => c == '}');
                    
                    // Skip this line and check if we're done with the method
                    if (onConfiguringBraceCount <= 0)
                    {
                        inOnConfiguring = false;
                    }
                    continue; // Skip all lines inside the existing OnConfiguring
                }

                if (!skipLine)
                {
                    modifiedLines.Add(line);
                }
                skipLine = false;
            }

            // If we didn't find an existing OnConfiguring, add one at the end of the class
            if (!onConfiguringAdded)
            {
                Console.WriteLine("No existing OnConfiguring found, adding new one...");
                
                // Find the last closing brace of the class
                for (int i = modifiedLines.Count - 1; i >= 0; i--)
                {
                    if (modifiedLines[i].Trim() == "}")
                    {
                        var providerCall = provider switch
                        {
                            "SqlServer" => $"optionsBuilder.UseSqlServer(@\"{connectionString.Replace("\"", "\"\"")}\");",
                            "PostgreSQL" => $"optionsBuilder.UseNpgsql(@\"{connectionString.Replace("\"", "\"\"")}\");",
                            _ => throw new InvalidOperationException($"Unsupported provider: {provider}")
                        };

                        modifiedLines.Insert(i, "");
                        modifiedLines.Insert(i + 1, "        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
                        modifiedLines.Insert(i + 2, "        {");
                        modifiedLines.Insert(i + 3, "            if (!optionsBuilder.IsConfigured)");
                        modifiedLines.Insert(i + 4, "            {");
                        modifiedLines.Insert(i + 5, $"                {providerCall}");
                        modifiedLines.Insert(i + 6, "            }");
                        modifiedLines.Insert(i + 7, "        }");
                        break;
                    }
                }
            }

            var result = string.Join('\n', modifiedLines);
            Console.WriteLine("DbContext modification completed.");
            
            return result;
        }

        /// <summary>
        /// Compiles the scaffolded code into a loadable assembly.
        /// </summary>
        /// <param name="tempDirectory">The temporary directory containing the generated code.</param>
        /// <param name="scaffoldedModel">The scaffolded model information.</param>
        /// <param name="config">The EDMX configuration.</param>
        /// <param name="modifiedContextCode">The modified DbContext code (with OnConfiguring override).</param>
        /// <returns>The compiled assembly containing the DbContext and entities.</returns>
        private static Task<Assembly> CompileScaffoldedCodeAsync(
            string tempDirectory, 
            ScaffoldedModel scaffoldedModel, 
            EdmxConfig config,
            string modifiedContextCode)
        {
            // Collect all source code
            var sourceTexts = new List<string>();
            
            // Add the modified DbContext source (with OnConfiguring override)
            sourceTexts.Add(modifiedContextCode);
            
            // Add all entity sources
            foreach (var entityFile in scaffoldedModel.AdditionalFiles)
            {
                sourceTexts.Add(entityFile.Code);
            }

            // Parse syntax trees
            var syntaxTrees = sourceTexts.Select(source => 
                CSharpSyntaxTree.ParseText(source)).ToArray();

            // Get required assembly references
            var references = GetRequiredReferences(config.Provider);

            // Create compilation
            var compilation = CSharpCompilation.Create(
                assemblyName: $"TempScaffold_{Guid.NewGuid():N}",
                syntaxTrees: syntaxTrees,
                references: references,
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    allowUnsafe: false,
                    nullableContextOptions: NullableContextOptions.Disable
                )
            );

            // Compile to memory stream
            using var memoryStream = new MemoryStream();
            var emitResult = compilation.Emit(memoryStream);

            if (!emitResult.Success)
            {
                var errors = emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.ToString())
                    .ToList();

                throw new InvalidOperationException(
                    $"Failed to compile scaffolded code. Errors:\n{string.Join('\n', errors)}"
                );
            }

            // Load the assembly from the compiled bytes
            memoryStream.Seek(0, SeekOrigin.Begin);
            var assemblyBytes = memoryStream.ToArray();
            var assembly = Assembly.Load(assemblyBytes);

            return Task.FromResult(assembly);
        }

        /// <summary>
        /// Creates a DbContext instance from the scaffolded and compiled code.
        /// </summary>
        /// <param name="assembly">The compiled assembly containing the DbContext.</param>
        /// <param name="config">The EDMX configuration.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <returns>A DbContext instance.</returns>
        private static DbContext CreateDbContextInstance(Assembly assembly, EdmxConfig config, string connectionString)
        {
            var contextNamespace = string.IsNullOrWhiteSpace(config.DbContextNamespace) ? "TempScaffold" : config.DbContextNamespace;
            var contextType = assembly.GetType($"{contextNamespace}.{config.ContextName}")
                ?? throw new InvalidOperationException($"Generated context type '{config.ContextName}' not found in compiled assembly.");

            Console.WriteLine($"Found context type: {contextType.FullName}");

            // List all constructors for debugging
            var constructors = contextType.GetConstructors();
            Console.WriteLine($"Available constructors:");
            foreach (var ctor in constructors)
            {
                var paramTypes = string.Join(", ", ctor.GetParameters().Select(p => p.ParameterType.Name));
                Console.WriteLine($"  - {ctor.Name}({paramTypes})");
            }

            // Try parameterless constructor first
            var parameterlessConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);
            if (parameterlessConstructor is not null)
            {
                try
                {
                    Console.WriteLine("Attempting to create instance using parameterless constructor...");
                    var context = (DbContext)Activator.CreateInstance(contextType);
                    Console.WriteLine("Successfully created DbContext instance.");
                    return context;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create instance with parameterless constructor: {ex.Message}");
                    if (ex.InnerException is not null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No parameterless constructor found.");
            }

            // Try to create with options as fallback
            var optionsConstructor = constructors.FirstOrDefault(c => 
                c.GetParameters().Length == 1 && 
                c.GetParameters()[0].ParameterType.Name.StartsWith("DbContextOptions"));

            if (optionsConstructor is not null)
            {
                try
                {
                    Console.WriteLine("Attempting to create instance using options constructor with null options...");
                    var context = (DbContext)Activator.CreateInstance(contextType, new object[] { null });
                    Console.WriteLine("Successfully created DbContext instance with null options.");
                    return context;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create instance with options constructor: {ex.Message}");
                    if (ex.InnerException is not null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }
            }

            throw new InvalidOperationException($"Failed to create DbContext instance of type '{contextType.Name}'. No suitable constructor found or all constructors failed.");
        }

        /// <summary>
        /// Gets the required assembly references for compilation based on the database provider.
        /// </summary>
        /// <param name="provider">The database provider.</param>
        /// <returns>A collection of metadata references required for compilation.</returns>
        private static IEnumerable<MetadataReference> GetRequiredReferences(string provider)
        {
            var references = new List<MetadataReference>();

            // Core .NET references
            AddReference(references, typeof(object)); // System.Private.CoreLib
            AddReference(references, typeof(Console)); // System.Console
            AddReference(references, typeof(IEnumerable<>)); // System.Linq
            AddReference(references, typeof(System.ComponentModel.DataAnnotations.KeyAttribute)); // System.ComponentModel.Annotations
            AddReference(references, typeof(System.Linq.Expressions.Expression)); // System.Linq.Expressions

            // EntityFramework Core references
            AddReference(references, typeof(DbConnection)); // Microsoft.EntityFrameworkCore
            AddReference(references, typeof(DbContext)); // Microsoft.EntityFrameworkCore
            AddReference(references, typeof(DbSet<>)); // Microsoft.EntityFrameworkCore
            AddReference(references, typeof(KeylessAttribute)); // Microsoft.EntityFrameworkCore.Abstractions
            AddReference(references, typeof(IndexAttribute)); // Microsoft.EntityFrameworkCore.Abstractions
            AddReference(references, typeof(DeleteBehavior)); // Microsoft.EntityFrameworkCore

            // Always add EFCore.Relational for extension methods and types like StoreObjectIdentifier
            TryAddReference(references, "Microsoft.EntityFrameworkCore.Relational");

            // Provider-specific references
            switch (provider)
            {
                case "SqlServer":
                    TryAddReference(references, "Microsoft.EntityFrameworkCore.SqlServer");
                    break;
                case "PostgreSQL":
                    TryAddReference(references, "Npgsql.EntityFrameworkCore.PostgreSQL");
                    break;
            }

            // Additional runtime references
            TryAddReference(references, "System.Runtime.CompilerServices.Unsafe");
            TryAddReference(references, "System.Runtime");
            TryAddReference(references, "System.Collections");

            return references;
        }

        private static void AddReference(List<MetadataReference> references, Type type)
        {
            references.Add(MetadataReference.CreateFromFile(type.Assembly.Location));
        }

        private static void TryAddReference(List<MetadataReference> references, string assemblyName)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
            catch
            {
                // Optional reference, ignore if not found
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [GeneratedRegex(@"entity\.Property\(.*?=>\s*.*?\.(\w+)\)")]
        private static partial System.Text.RegularExpressions.Regex PropertyRegex();
    }
}
