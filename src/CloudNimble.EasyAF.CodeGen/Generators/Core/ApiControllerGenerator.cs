using CloudNimble.EasyAF.CodeGen.Generators.Base;
using CloudNimble.EasyAF.CodeGen.Legacy;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Reflection;

namespace CloudNimble.EasyAF.CodeGen.Generators.Core
{

    /// <summary>
    /// 
    /// </summary>
    public class ApiControllerGenerator : ContainerGeneratorBase
    {

        #region Fields

        internal readonly bool _isEFCore;
        internal readonly bool _addInheritance;
        internal readonly string _baseClass;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ApiControllerGenerator class.
        /// </summary>
        /// <param name="extraUsings">Additional using statements to include.</param>
        /// <param name="controllerNamespace">The namespace for the generated controller.</param>
        /// <param name="container">The EntityContainer to generate the controller for.</param>
        /// <param name="isEFCore">Whether the target is Entity Framework Core.</param>
        /// <param name="addInheritance">Whether to include inheritance in the class declaration.</param>
        /// <param name="baseClass">The name of the base class to inherit from.</param>
        public ApiControllerGenerator(List<string> extraUsings, string controllerNamespace, EntityContainer container, bool isEFCore, bool addInheritance = true, string baseClass = null)
            : base(extraUsings, controllerNamespace, container)
        {
            _isEFCore = isEFCore;
            _addInheritance = addInheritance;
            _baseClass = baseClass ?? CodeGenConstants.ApiBaseClassName;
            AddUsings();
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///
        /// </summary>
        public override void Generate()
        {
            if (IsGenerated) return;
            Header();
            WriteUsings();
            NamespaceBegin(Namespace);
            ClassBegin(CodeGenerationTools.ControllerClassDeclaration(EntityContainer.Name, _addInheritance, _baseClass), "");
            WriteConstructors();

            // Only write IsOnline() if we have inheritance, can resolve the base type,
            // and the IsOnline method doesn't already exist in the base type
            bool canResolveBaseType = !_addInheritance || GetBaseConstructorParameters() is not null;
            bool baseHasIsOnline = BaseTypeHasIsOnlineMethod();

            RegionBegin("Public Methods");
            if (canResolveBaseType && !baseHasIsOnline)
            {
                WriteIsOnline();
            }
            else if (!canResolveBaseType)
            {
                _writer.WriteLine("// IsOnline() method skipped: unable to resolve base class dependencies.");
                _writer.WriteLine("// Please implement IsOnline() method manually if needed.");
                _writer.WriteLine();
            }
            else if (baseHasIsOnline)
            {
                _writer.WriteLine("// IsOnline() method skipped: method already exists in base class.");
                _writer.WriteLine();
            }
            RegionEnd();
            ClassEnd();
            NamespaceEnd();
            IsGenerated = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string WriteFile(string directory = null)
        {
            return WriteFile($"{EntityContainer.Name}Api", directory);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///
        /// </summary>
        internal void WriteConstructors(bool isAdmin = false)
        {
            // Skip constructor generation when not using inheritance
            if (!_addInheritance)
            {
                return;
            }

            var constructorParams = GetBaseConstructorParameters();

            // If we couldn't determine the constructor parameters (e.g., external type not available),
            // skip constructor generation entirely. The user will need to provide their own constructor.
            if (constructorParams == null)
            {
                // Optionally, we could generate a comment explaining why no constructor was generated
                RegionBegin("Constructors");
                _writer.WriteLine("// Constructor generation skipped: unable to determine base class constructor parameters.");
                _writer.WriteLine("// Please provide a constructor that calls the appropriate base class constructor.");
                RegionEnd();
                return;
            }

            RegionBegin("Constructors");

            var className = isAdmin ? $"{EntityContainer.Name}AdminApi" : $"{EntityContainer.Name}Api";

            // Generate XML documentation
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("/// Initializes a new instance of the class.");
            _writer.WriteLine("/// </summary>");

            foreach (var param in constructorParams)
            {
                _writer.WriteLine($"/// <param name=\"{param.Name}\">{GetParameterDocumentation(param.Type, param.Name)}</param>");
            }

            // Generate constructor signature
            _writer.WriteLine($"public {className}(");
            _writer.Indent++;

            for (var i = 0; i < constructorParams.Count; i++)
            {
                var param = constructorParams[i];
                var paramType = GetParameterTypeString(param.Type, param.Name, className);
                var comma = i < constructorParams.Count - 1 ? "," : ")";
                _writer.WriteLine($"{paramType} {param.Name}{comma}");
            }

            // Generate base constructor call
            var baseArgs = string.Join(", ", constructorParams.Select(p => p.Name));
            _writer.WriteLine($": base({baseArgs})");
            _writer.Indent--;

            _writer.WriteLine("{");
            _writer.WriteLine("}");
            _writer.WriteLine();
            RegionEnd();
        }
        
        /// <summary>
        /// Checks if the IsOnline method already exists in the base type.
        /// </summary>
        /// <returns>True if the IsOnline method exists in the base type, false otherwise.</returns>
        protected bool BaseTypeHasIsOnlineMethod()
        {
            if (!_addInheritance)
            {
                return false;
            }

            var baseType = FindBaseType(_baseClass);
            if (baseType is null)
            {
                return false;
            }

            // Look for a public method named "IsOnline" that returns bool and takes no parameters
            var isOnlineMethod = baseType.GetMethod("IsOnline",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);

            return isOnlineMethod is not null && isOnlineMethod.ReturnType == typeof(bool);
        }

        /// <summary>
        /// Gets the constructor parameters for the base class using reflection.
        /// </summary>
        /// <returns>A list of constructor parameters, or null if the type cannot be resolved.</returns>
        protected List<(Type Type, string Name)> GetBaseConstructorParameters()
        {
            // Handle the default EasyAF base class with known parameters
            if (_baseClass == CodeGenConstants.ApiBaseClassName)
            {
                // For the default base class, use a special marker to indicate we should use known parameter types
                // This avoids compile-time dependencies on types that may not be available
                return GetDefaultBaseClassParameters();
            }

            // Try to find the base type using reflection
            var baseType = FindBaseType(_baseClass);

            // If we can't find the type (common when it's in an external assembly not loaded in the generator context),
            // return null to indicate constructor generation should be skipped
            if (baseType == null)
            {
                return null;
            }

            // Try to get constructor information
            try
            {
                var constructors = baseType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                var constructor = constructors.FirstOrDefault();

                if (constructor == null)
                {
                    // No public constructor found
                    return null;
                }

                var parameters = constructor.GetParameters();
                var result = new List<(Type, string)>();

                foreach (var param in parameters)
                {
                    result.Add((param.ParameterType, param.Name));
                }

                return result;
            }
            catch
            {
                // If any error occurs during reflection, return null
                return null;
            }
        }
        
        /// <summary>
        /// Gets the known constructor parameters for the default EasyAF base class.
        /// </summary>
        /// <returns>A list of parameter types and names for the default base class constructor.</returns>
        private List<(Type Type, string Name)> GetDefaultBaseClassParameters()
        {
            // Use marker types for the default base class parameters to avoid compile-time dependencies
            // The actual parameter type strings will be handled by the WriteConstructors method
            return
            [
                (typeof(IServiceProvider), "serviceProvider"),
                (typeof(object), "httpContextAccessor"),  // Placeholder - actual type is IHttpContextAccessor
                (typeof(object), "messagePublisher"),    // Placeholder - actual type is IMessagePublisher
                (typeof(object), "logger")               // Placeholder - actual type is ILogger<T>
            ];
        }
        
        /// <summary>
        /// Parses a generic type name to extract the base type name and generic arguments.
        /// </summary>
        /// <param name="typeName">The full type name, possibly including generic arguments.</param>
        /// <returns>A tuple containing the base type name and list of generic argument names.</returns>
        private (string BaseTypeName, List<string> GenericArguments) ParseGenericTypeName(string typeName)
        {
            var genericArguments = new List<string>();
            var baseTypeName = typeName;

            // Check if this is a generic type (contains < and >)
            var genericStartIndex = typeName.IndexOf('<');
            if (genericStartIndex > 0)
            {
                var genericEndIndex = typeName.LastIndexOf('>');
                if (genericEndIndex > genericStartIndex)
                {
                    baseTypeName = typeName.Substring(0, genericStartIndex);
                    var genericArgsString = typeName.Substring(genericStartIndex + 1, genericEndIndex - genericStartIndex - 1);

                    // Parse generic arguments (handle nested generics by counting brackets)
                    var currentArg = string.Empty;
                    var bracketDepth = 0;

                    foreach (var ch in genericArgsString)
                    {
                        if (ch == '<')
                        {
                            bracketDepth++;
                            currentArg += ch;
                        }
                        else if (ch == '>')
                        {
                            bracketDepth--;
                            currentArg += ch;
                        }
                        else if (ch == ',' && bracketDepth == 0)
                        {
                            genericArguments.Add(currentArg.Trim());
                            currentArg = string.Empty;
                        }
                        else
                        {
                            currentArg += ch;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(currentArg))
                    {
                        genericArguments.Add(currentArg.Trim());
                    }
                }
            }

            return (baseTypeName, genericArguments);
        }

        /// <summary>
        /// Resolves a generic argument type name to an actual Type.
        /// </summary>
        /// <param name="argumentTypeName">The name of the generic argument type.</param>
        /// <returns>The resolved Type, or null if not found.</returns>
        private Type ResolveGenericArgumentType(string argumentTypeName)
        {
            // First check if it's referring to the EntityContainer (DbContext)
            // The EntityContainer.Name typically does not include "DbContext" suffix
            if (argumentTypeName == EntityContainer.Name + "DbContext" ||
                argumentTypeName == EntityContainer.Name + "Context" ||
                argumentTypeName == EntityContainer.Name ||
                argumentTypeName == "TContext")
            {
                // Try to find the DbContext type with common naming patterns
                var dbContextType = FindNonGenericType(EntityContainer.Name + "DbContext");
                if (dbContextType != null) return dbContextType;

                dbContextType = FindNonGenericType(EntityContainer.Name + "Context");
                if (dbContextType != null) return dbContextType;

                dbContextType = FindNonGenericType(EntityContainer.Name);
                if (dbContextType != null && IsDbContextType(dbContextType)) return dbContextType;

                // Return a marker type to indicate we need the DbContext
                // Use DbContext from EF Core if available, otherwise EF6
                var efCoreDbContextType = Type.GetType("Microsoft.EntityFrameworkCore.DbContext, Microsoft.EntityFrameworkCore");
                if (efCoreDbContextType != null) return efCoreDbContextType;

                return typeof(DbContext);
            }

            // Try to resolve as a regular type
            return FindNonGenericType(argumentTypeName);
        }

        /// <summary>
        /// Checks if a type is or derives from DbContext.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is or derives from DbContext.</returns>
        private bool IsDbContextType(Type type)
        {
            if (type == null) return false;

            // Check for EF Core DbContext
            var efCoreDbContextType = Type.GetType("Microsoft.EntityFrameworkCore.DbContext, Microsoft.EntityFrameworkCore");
            if (efCoreDbContextType != null && efCoreDbContextType.IsAssignableFrom(type))
            {
                return true;
            }

            // Check for EF6 DbContext
            if (typeof(System.Data.Entity.DbContext).IsAssignableFrom(type))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to find a non-generic type using the available using statements.
        /// </summary>
        /// <param name="typeName">The name of the type to find.</param>
        /// <returns>The Type if found, null otherwise.</returns>
        private Type FindNonGenericType(string typeName)
        {
            // Try to find the type in the current app domain
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // Try the type name directly
                    var type = assembly.GetType(typeName);
                    if (type is not null) return type;

                    // Try with each using namespace
                    foreach (var usingStatement in ExtraUsings)
                    {
                        var fullTypeName = $"{usingStatement}.{typeName}";
                        type = assembly.GetType(fullTypeName);
                        if (type is not null) return type;
                    }
                }
                catch
                {
                    // Ignore exceptions when accessing assemblies
                }
            }

            return null;
        }

        /// <summary>
        /// Attempts to find the base type using the available using statements, including support for generic types.
        /// Enhanced with multiple assembly loading strategies.
        /// </summary>
        /// <param name="typeName">The name of the type to find, possibly including generic arguments.</param>
        /// <returns>The Type if found, null otherwise.</returns>
        private Type FindBaseType(string typeName)
        {
            // Parse the type name to extract base type and generic arguments
            var (baseTypeName, genericArguments) = ParseGenericTypeName(typeName);

            // If no generic arguments, use the enhanced non-generic lookup
            if (genericArguments.Count == 0)
            {
                return FindTypeWithEnhancedLoading(baseTypeName);
            }

            // Find the generic type definition using enhanced loading
            var genericTypeDefinition = FindGenericTypeDefinition(baseTypeName, genericArguments.Count);
            if (genericTypeDefinition == null)
            {
                return null;
            }

            // Resolve the generic argument types
            var argumentTypes = new List<Type>();
            foreach (var argName in genericArguments)
            {
                var argType = ResolveGenericArgumentType(argName);
                if (argType == null)
                {
                    // If we can't resolve a generic argument, we can't construct the type
                    return null;
                }
                argumentTypes.Add(argType);
            }

            // Construct the generic type
            try
            {
                return genericTypeDefinition.MakeGenericType(argumentTypes.ToArray());
            }
            catch
            {
                // Failed to construct the generic type
                return null;
            }
        }

        /// <summary>
        /// Finds a generic type definition using enhanced assembly loading strategies.
        /// </summary>
        /// <param name="baseTypeName">The base type name without generic arguments.</param>
        /// <param name="genericParameterCount">The number of generic parameters.</param>
        /// <returns>The generic type definition if found, null otherwise.</returns>
        private Type FindGenericTypeDefinition(string baseTypeName, int genericParameterCount)
        {
            var genericTypeName = $"{baseTypeName}`{genericParameterCount}";

            // Strategy 1: Search loaded assemblies
            var type = SearchLoadedAssemblies(genericTypeName);
            if (type != null) return type;

            // Strategy 2: Try Type.GetType with assembly-qualified names
            type = TryGetTypeWithAssemblyQualifiedName(genericTypeName);
            if (type != null) return type;

            // Strategy 3: Try to load from common assembly patterns
            type = TryLoadFromCommonAssemblyPatterns(baseTypeName, genericTypeName);
            if (type != null) return type;

            return null;
        }

        /// <summary>
        /// Enhanced type finding with multiple loading strategies for non-generic types.
        /// </summary>
        /// <param name="typeName">The type name to find.</param>
        /// <returns>The Type if found, null otherwise.</returns>
        private Type FindTypeWithEnhancedLoading(string typeName)
        {
            // Strategy 1: Search loaded assemblies (existing logic)
            var type = FindNonGenericType(typeName);
            if (type != null) return type;

            // Strategy 2: Try Type.GetType with assembly-qualified names
            type = TryGetTypeWithAssemblyQualifiedName(typeName);
            if (type != null) return type;

            // Strategy 3: Try to load from common assembly patterns
            type = TryLoadFromCommonAssemblyPatterns(typeName, typeName);
            if (type != null) return type;

            return null;
        }

        /// <summary>
        /// Searches through all currently loaded assemblies for the specified type.
        /// </summary>
        /// <param name="typeName">The type name to search for.</param>
        /// <returns>The Type if found, null otherwise.</returns>
        private Type SearchLoadedAssemblies(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // Try the type name directly
                    var type = assembly.GetType(typeName);
                    if (type is not null) return type;

                    // Try with each using namespace
                    foreach (var usingStatement in ExtraUsings)
                    {
                        var fullTypeName = $"{usingStatement}.{typeName}";
                        type = assembly.GetType(fullTypeName);
                        if (type is not null) return type;
                    }
                }
                catch
                {
                    // Ignore exceptions when accessing assemblies
                }
            }
            return null;
        }

        /// <summary>
        /// Attempts to get the type using Type.GetType with assembly-qualified names.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <returns>The Type if found, null otherwise.</returns>
        private Type TryGetTypeWithAssemblyQualifiedName(string typeName)
        {
            try
            {
                // Try direct Type.GetType (works for types in mscorlib and currently loaded assemblies)
                var type = Type.GetType(typeName);
                if (type != null) return type;

                // Try with using namespaces
                foreach (var usingStatement in ExtraUsings)
                {
                    var fullTypeName = $"{usingStatement}.{typeName}";
                    type = Type.GetType(fullTypeName);
                    if (type != null) return type;

                    // Try with common assembly names if we have a namespace
                    var assemblyName = usingStatement.Split('.')[0]; // First part of namespace often matches assembly
                    var assemblyQualifiedName = $"{fullTypeName}, {assemblyName}";
                    type = Type.GetType(assemblyQualifiedName);
                    if (type != null) return type;
                }
            }
            catch
            {
                // Type.GetType can throw various exceptions
            }
            return null;
        }

        /// <summary>
        /// Attempts to load the type from common assembly naming patterns.
        /// </summary>
        /// <param name="baseTypeName">The base type name (without generic arguments).</param>
        /// <param name="fullTypeName">The full type name (with generic arity if applicable).</param>
        /// <returns>The Type if found, null otherwise.</returns>
        private Type TryLoadFromCommonAssemblyPatterns(string baseTypeName, string fullTypeName)
        {
            try
            {
                // Try to infer assembly names from using statements
                foreach (var usingStatement in ExtraUsings)
                {
                    var possibleAssemblyNames = new List<string>
                    {
                        usingStatement, // Full namespace as assembly name
                        usingStatement.Split('.')[0], // First part of namespace
                        $"{usingStatement.Split('.')[0]}.{usingStatement.Split('.')[1]}" // First two parts
                    };

                    foreach (var assemblyName in possibleAssemblyNames)
                    {
                        try
                        {
                            // Try to load the assembly by name
                            var assembly = Assembly.LoadFrom($"{assemblyName}.dll");
                            var type = assembly.GetType($"{usingStatement}.{fullTypeName}");
                            if (type != null) return type;
                        }
                        catch
                        {
                            // Assembly loading can fail for many reasons
                        }

                        try
                        {
                            // Try Assembly.Load (for GAC assemblies or already loaded)
                            var assembly = Assembly.Load(assemblyName);
                            var type = assembly.GetType($"{usingStatement}.{fullTypeName}");
                            if (type != null) return type;
                        }
                        catch
                        {
                            // Assembly loading can fail for many reasons
                        }
                    }
                }
            }
            catch
            {
                // Any exception in assembly loading
            }

            return null;
        }
        
        /// <summary>
        /// Gets the string representation of a parameter type for code generation.
        /// </summary>
        /// <param name="paramType">The parameter type.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <param name="className">The generated class name for logger types.</param>
        /// <returns>The type string for code generation.</returns>
        private string GetParameterTypeString(Type paramType, string paramName, string className)
        {
            // Handle special cases for default base class when using placeholders
            if (_baseClass == CodeGenConstants.ApiBaseClassName && paramType == typeof(object))
            {
                return paramName switch
                {
                    "httpContextAccessor" => "IHttpContextAccessor",
                    "messagePublisher" => "IMessagePublisher",
                    "logger" => $"ILogger<{className}>",
                    _ => "object"
                };
            }
            
            if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Microsoft.Extensions.Logging.ILogger<>))
            {
                return $"ILogger<{className}>";
            }
            
            if (paramType.IsGenericType)
            {
                var genericTypeName = paramType.Name.Substring(0, paramType.Name.IndexOf('`'));
                var genericArgs = string.Join(", ", paramType.GetGenericArguments().Select(arg => GetSimpleTypeName(arg)));
                return $"{genericTypeName}<{genericArgs}>";
            }
            
            return GetSimpleTypeName(paramType);
        }
        
        /// <summary>
        /// Gets a simple type name for code generation.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The simple type name.</returns>
        private string GetSimpleTypeName(Type type)
        {
            // Map common types to their C# keywords
            var typeMap = new Dictionary<Type, string>
            {
                { typeof(string), "string" },
                { typeof(int), "int" },
                { typeof(bool), "bool" },
                { typeof(object), "object" }
            };
            
            if (typeMap.TryGetValue(type, out var value))
            {
                return value;
            }
            
            // For interface types, use just the interface name
            if (type.IsInterface)
            {
                return type.Name;
            }
            
            return type.Name;
        }
        
        /// <summary>
        /// Gets appropriate XML documentation for a parameter type.
        /// </summary>
        /// <param name="paramType">The parameter type.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <returns>The documentation string.</returns>
        private string GetParameterDocumentation(Type paramType, string paramName)
        {
            if (paramType == typeof(IServiceProvider) || paramType.Name == "IServiceProvider")
            {
                return "The service provider for dependency injection.";
            }
            
            // Handle special cases for default base class when using placeholders
            if (_baseClass == CodeGenConstants.ApiBaseClassName && paramType == typeof(object))
            {
                return paramName switch
                {
                    "httpContextAccessor" => "The <see cref=\"IHttpContextAccessor\"/> for the current HTTP context.",
                    "messagePublisher" => "The <see cref=\"IMessagePublisher\"/> used for publishing messages to SimpleMessageBus.",
                    "logger" => "The <see cref=\"ILogger{T}\"/> instance for writing log traces.",
                    _ => $"The {paramName} parameter."
                };
            }
            
            if (paramType.Name == "IHttpContextAccessor")
            {
                return "The <see cref=\"IHttpContextAccessor\"/> for the current HTTP context.";
            }
            
            if (paramType.Name == "IMessagePublisher")
            {
                return "The <see cref=\"IMessagePublisher\"/> used for publishing messages to SimpleMessageBus.";
            }
            
            if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Microsoft.Extensions.Logging.ILogger<>))
            {
                return "The <see cref=\"ILogger{T}\"/> instance for writing log traces.";
            }
            
            return $"The {paramType.Name} parameter.";
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteIsOnline()
        {
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("/// ");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine("/// <returns></returns>");
            _writer.WriteLine("[UnboundOperation]");
            _writer.WriteLine("public bool IsOnline()");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("try");
            _writer.WriteLine("{");
            _writer.Indent++;
            if (_isEFCore)
            {
                _writer.WriteLine("return DbContext.Database.CanConnect();");
            }
            else
            {
                _writer.WriteLine("return DbContext.Database.Exists();");
            }
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine("#pragma warning disable CA1031 // Do not catch general exception types");
            _writer.WriteLine("catch (Exception ex)");
            _writer.WriteLine("#pragma warning restore CA1031 // Do not catch general exception types");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("Debug.WriteLine(ex);");
            _writer.WriteLine("return false;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
        }

        /// <summary>
        /// 
        /// </summary>

        /// <summary>
        /// 
        /// </summary>
        internal void AddUsings()
        {
            ExtraUsings.Add("CloudNimble.EasyAF.Restier");
            ExtraUsings.Add("CloudNimble.SimpleMessageBus.Publish");
            ExtraUsings.Add("Microsoft.AspNetCore.Http");
            ExtraUsings.Add("Microsoft.Extensions.DependencyInjection");
            ExtraUsings.Add("Microsoft.Extensions.Logging");
            ExtraUsings.Add("Microsoft.Restier.AspNetCore.Model");
            ExtraUsings.Add("System");
            ExtraUsings.Add("System.Linq");
            ExtraUsings.Add("System.Reflection");
            if (_isEFCore)
            {
                ExtraUsings.Add("Microsoft.Restier.EntityFrameworkCore");
            }
            else
            {
                ExtraUsings.Add("Microsoft.Restier.EntityFramework");
            }
            ExtraUsings.Add("System.Collections.Generic");
            ExtraUsings.Add("System.Diagnostics");
        }

        #endregion

    }

}
