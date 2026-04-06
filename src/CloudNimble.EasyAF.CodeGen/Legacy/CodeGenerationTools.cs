using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Data.Entity.Core.Metadata.Edm;
using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using CloudNimble.EasyAF.Core;

namespace CloudNimble.EasyAF.CodeGen.Legacy
{

    /// <summary>
    /// Responsible for helping to create source code that is
    /// correctly formatted and functional
    /// </summary>
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    public static class CodeGenerationTools
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
    {

        #region Private Members

        private static readonly CSharpCodeProvider _code;
        private const string ExternalTypeNameAttributeName = @"http://schemas.microsoft.com/ado/2006/04/codegeneration:ExternalTypeName";

        #endregion

        #region Public Static Properties

        /// <summary>
        /// When true, all types that are not being generated
        /// are fully qualified to keep them from conflicting with
        /// types that are being generated. Useful when you have
        /// something like a type being generated named System.
        ///
        /// Default is false.
        /// </summary>
        public static bool FullyQualifySystemTypes { get; set; }

        /// <summary>
        /// When true, the field names are Camel Cased,
        /// otherwise they will preserve the case they
        /// start with.
        ///
        /// Default is true.
        /// </summary>
        public static bool CamelCaseFields { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new CodeGenerationTools object with the TextTransformation (T4 generated class)
        /// that is currently running
        /// </summary>
        static CodeGenerationTools()
        {
            _code = new CSharpCodeProvider();
            FullyQualifySystemTypes = false;
            CamelCaseFields = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the abstract option if the entity is Abstract, otherwise returns String.Empty.
        /// </summary>
        public static string AbstractOption(EntityType entity)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));

            return entity.Abstract ? "abstract" : string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string AuthorizationClassDeclaration(EntityContainer container)
        {
            return $"public static class {Escape(container)}AuthorizationConfig";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string BusinessDependencyClassDeclaration(string projectName)
        {
            return $"public static class {projectName}Business_IServiceCollectionExtensions";
        }

        /// <summary>
        /// Returns the passed in identifier with the first letter changed to lowercase.
        /// </summary>
        public static string CamelCase(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return identifier;

            return $"{identifier[0].ToString(CultureInfo.InvariantCulture).ToLowerInvariant()}{(identifier.Length > 1 ? identifier.Substring(1) : string.Empty)}";
        }

        /// <summary>
        /// Generates the class declaration for an Admin API controller.
        /// </summary>
        /// <param name="dbContextName">The name of the DbContext class.</param>
        /// <param name="addInheritance">Whether to include inheritance in the class declaration.</param>
        /// <param name="baseClass">The name of the base class to inherit from. Defaults to the value in CodeGenConstants.ApiBaseClassName.</param>
        /// <returns>The class declaration string for the Admin API controller.</returns>
        public static string AdminControllerClassDeclaration(string dbContextName, bool addInheritance = false, string baseClass = null)
        {
            if (!addInheritance)
            {
                return $"public partial class {dbContextName}AdminApi";
            }

            var actualBaseClass = baseClass ?? CodeGenConstants.ApiBaseClassName;

            // Check if the base class already includes generic type arguments
            if (actualBaseClass.Contains('<'))
            {
                // Base class already has generic arguments, use as-is
                return $"public partial class {dbContextName}AdminApi : {actualBaseClass}";
            }
            else
            {
                // Base class needs generic argument, add the DbContext type
                return $"public partial class {dbContextName}AdminApi : {actualBaseClass}<{dbContextName}>";
            }
        }

        /// <summary>
        /// Generates the class declaration for an API controller.
        /// </summary>
        /// <param name="dbContextName">The name of the DbContext class.</param>
        /// <param name="addInheritance">Whether to include inheritance in the class declaration.</param>
        /// <param name="baseClass">The name of the base class to inherit from. Defaults to the value in CodeGenConstants.ApiBaseClassName.</param>
        /// <returns>The class declaration string for the API controller.</returns>
        public static string ControllerClassDeclaration(string dbContextName, bool addInheritance = false, string baseClass = null)
        {
            if (!addInheritance)
            {
                return $"public partial class {dbContextName}Api";
            }

            var actualBaseClass = baseClass ?? CodeGenConstants.ApiBaseClassName;

            // Check if the base class already includes generic type arguments
            if (actualBaseClass.Contains('<'))
            {
                // Base class already has generic arguments, use as-is
                return $"public partial class {dbContextName}Api : {actualBaseClass}";
            }
            else
            {
                // Base class needs generic argument, add the DbContext type
                return $"public partial class {dbContextName}Api : {actualBaseClass}<{dbContextName}>";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static string DbContextClassDeclaration(EntityContainer container)
        {
            return $"{Accessibility.ForType(container)} partial class {Escape(container)} : DbContext";
        }

        /// <summary>
        /// Creates the class declaration for a given <see cref="EntityComposition"/>.
        /// </summary>
        /// <param name="entity">The <see cref="EntityComposition"/> instance that contains the EasyAF breakdowns plus the EDMX model metadata for a given Entity.</param>
        /// <returns>A string that contains the Entity class' name and base types.</returns>
        public static string EntityClassDeclaration(EntityComposition entity)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));

            var baseTypes = new List<string> { "DbObservableObject" };

            //RWM: Start with the stuff that is mutually-exclusive. For example, IDbStateEnum already implements IActiveTrackable.
            switch (true)
            {
                case true when entity.IsDbStateEnum:
                    baseTypes.Add("IDbStateEnum");
                    break;
                case true when entity.IsDbStatusEnum:
                    baseTypes.Add("IDbStatusEnum");
                    break;
                case true when entity.HasState:
                    var stateProperty = entity.EntityType.NavigationProperties.FirstOrDefault(c => c.Name.EndsWith("StateType"));
                    baseTypes.Add($"IHasState<{Escape(stateProperty.TypeUsage)}>");
                    break;
                case true when entity.HasStatus:
                    var statusProperty = entity.EntityType.NavigationProperties.FirstOrDefault(c => c.Name.EndsWith("StatusType"));
                    baseTypes.Add($"IHasStatus<{Escape(statusProperty.TypeUsage)}>");
                    break;
                case true when entity.IsDbEnum:
                    baseTypes.Add("IDbEnum");
                    break;
                default:
                    if (entity.IsIdentifiable)
                    {
                        var idProperty = entity.SimpleProperties.FirstOrDefault(c => c.Name == "Id");
                        baseTypes.Add($"IIdentifiable<{Escape(idProperty.TypeUsage).Replace("System.", "")}>");
                    }
                    if (entity.IsActiveTrackable) baseTypes.Add("IActiveTrackable");
                    if (entity.IsHumanReadable) baseTypes.Add("IHumanReadable");
                    if (entity.IsSortable) baseTypes.Add("ISortable");
                    break;
            }

            // RWM: These things can be on any entity.
            if (entity.IsCreatedAuditable) baseTypes.Add("ICreatedAuditable");
            if (entity.IsCreatorTrackable)
            {
                var creatorProperty = entity.EntityType.Properties.FirstOrDefault(c => c.Name == "CreatedById");
                baseTypes.Add($"ICreatorTrackable<{Escape(creatorProperty.TypeUsage, true).Replace("System.", "")}>");
            }
            if (entity.IsUpdatedAuditable) baseTypes.Add("IUpdatedAuditable");
            if (entity.IsUpdaterTrackable)
            {
                var updaterProperty = entity.EntityType.Properties.FirstOrDefault(c => c.Name == "UpdatedById");
                baseTypes.Add($"IUpdaterTrackable<{Escape(updaterProperty.TypeUsage, true).Replace("System.", "")}>");
            }

            return $"public partial class {Escape(entity.EntityType)} : {string.Join(", ", baseTypes)}";
        }

        /// <summary>
        /// Creates the class declaration for a given <see cref="EntityComposition"/>.
        /// </summary>
        /// <param name="entity">The <see cref="EntityComposition"/> instance that contains the EasyAF breakdowns plus the EDMX model metadata for a given Entity.</param>
        /// <param name="dbContextTypeName"></param>
        /// <returns>A string that contains the Entity class' name and base types.</returns>
        public static string ManagerClassDeclaration(EntityComposition entity, string dbContextTypeName)
        {
            Ensure.ArgumentNotNull(entity, nameof(entity));

            var baseType = string.Empty;

            if (!entity.IsIdentifiable)
            {
                baseType = $"EntityManager<{dbContextTypeName}, {Escape(entity.EntityType)}>";
            }
            else
            {
                //RWM: Start with the stuff that is mutually-exclusive. For example, IDbStateEnum already implements IActiveTrackable.
                switch (true)
                {
                     case true when entity.HasState:
                        var stateProperty = entity.EntityType.NavigationProperties.FirstOrDefault(c => c.Name.EndsWith("StateType"));
                        baseType = $"StateMachineEntityManager<{dbContextTypeName}, {Escape(entity.EntityType)}, {GetTypeName(entity.KeyProperties.First().TypeUsage).Replace("System.", "")}, {Escape(stateProperty.TypeUsage)}>";
                        break;
                    case true when entity.HasStatus:
                        var statusProperty = entity.EntityType.NavigationProperties.FirstOrDefault(c => c.Name.EndsWith("StatusType"));
                        baseType = $"StatusEntityManager<{dbContextTypeName}, {Escape(entity.EntityType)}, {GetTypeName(entity.KeyProperties.First().TypeUsage).Replace("System.", "")}, {Escape(statusProperty.TypeUsage)}>";
                        break;
                    default:
                        baseType = $"IdentifiableEntityManager<{dbContextTypeName}, {Escape(entity.EntityType)}, {GetTypeName(entity.KeyProperties.First().TypeUsage).Replace("System.", "")}>";
                        break;
                }
            }

            return $"public partial class {Escape(entity.EntityType)}Manager : {baseType}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static string ModelBuilderClassDeclaration(EntityContainer container)
        {
            return $"public partial class {Escape(container)}ModelBuilder : IModelBuilder";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string RestierDependencyClassDeclaration(string projectName)
        {
            return $"public static class {projectName}Restier_IServiceCollectionExtensions";
        }

        /// <summary>
        /// Returns as full of a name as possible, if a namespace is provided the namespace and name are combined with a period, otherwise just the name is returned.
        /// </summary>
        public static string CreateFullName(string namespaceName, string name)
        {
            return !string.IsNullOrEmpty(namespaceName) ? $"{namespaceName}.{name}" : name;
        }

        /// <summary>
        /// Retuns a literal representing the supplied value.
        /// </summary>
        public static string CreateLiteral(object value)
        {
            if (value is null) return string.Empty;

            var type = value.GetType();

            switch (true)
            {
                case true when type.IsEnum:
                    return $"{type.FullName}.{value.ToString()}";

                case true when type == typeof(Guid):
                    return string.Format(CultureInfo.InvariantCulture, "new Guid(\"{0}\")", ((Guid)value).ToString("D", CultureInfo.InvariantCulture));

                case true when type == typeof(DateTime):
                    return string.Format(CultureInfo.InvariantCulture, "new DateTime({0}, DateTimeKind.Unspecified)", ((DateTime)value).Ticks);

                case true when type == typeof(byte[]):
                    var arrayInit = string.Join(", ", ((byte[])value).Select(b => b.ToString(CultureInfo.InvariantCulture)).ToArray());
                    return string.Format(CultureInfo.InvariantCulture, "new Byte[] {{{0}}}", arrayInit);

                case true when type == typeof(DateTimeOffset):
                    var dto = (DateTimeOffset)value;
                    return string.Format(CultureInfo.InvariantCulture, "new DateTimeOffset({0}, new TimeSpan({1}))", dto.Ticks, dto.Offset.Ticks);

                case true when type == typeof(TimeSpan):
                    return string.Format(CultureInfo.InvariantCulture, "new TimeSpan({0})", ((TimeSpan)value).Ticks);
            }

            var expression = new CodePrimitiveExpression(value);
            var writer = new StringWriter();
            _code.GenerateCodeFromExpression(expression, writer, new CodeGeneratorOptions());
            return writer.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entitySet"></param>
        /// <returns></returns>
        public static string DbSet(EntitySet entitySet)
        {
            return entitySet is null 
                ? string.Empty 
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} virtual DbSet<{1}> {2} {{ get; set; }}",
                    Accessibility.ForReadOnlyProperty(entitySet),
                    GetTypeName(entitySet.ElementType),
                    Escape(entitySet));
        }

        #region Escape

        /// <summary>
        /// Returns a string that is safe for use as an identifier in C#. Keywords are escaped.
        /// </summary>
        public static string Escape(string name)
        {
            return name is not null ? _code.CreateEscapedIdentifier(name) : null;
        }

        /// <summary>
        /// Returns the name of the TypeUsage's EdmType that is safe for use as an identifier.
        /// </summary>
        public static string Escape(TypeUsage typeUsage, bool ignoreNullables = false)
        {
            if (typeUsage is null) return string.Empty;

            switch (true)
            {
                case true when typeUsage.EdmType is ComplexType:
                case true when typeUsage.EdmType is EntityType:
                    return Escape(typeUsage.EdmType.Name);

                case true when typeUsage.EdmType is SimpleType:
                    var clrType = MetadataTools.UnderlyingClrType(typeUsage.EdmType);
                    var typeName = typeUsage.EdmType is EnumType ? Escape(typeUsage.EdmType.Name) : Escape(clrType);
                    if (clrType.IsValueType && MetadataTools.IsNullable(typeUsage) && !ignoreNullables)
                    {
                        return string.Format(CultureInfo.InvariantCulture, "Nullable<{0}>", typeName);
                    }

                    return typeName;
                case true when typeUsage.EdmType is CollectionType:
                    return string.Format(CultureInfo.InvariantCulture, "ICollection<{0}>", Escape(((CollectionType)typeUsage.EdmType).TypeUsage));
            }

            throw new ArgumentException(nameof(typeUsage));
        }

        /// <summary>
        /// Returns the name of the EdmMember that is safe for use as an identifier.
        /// </summary>
        public static string Escape(EdmMember member)
        {
            return member is not null ? Escape(member.Name) : string.Empty;
        }

        /// <summary>
        /// Returns the name of the EdmType that is safe for use as an identifier.
        /// </summary>
        public static string Escape(EdmType type)
        {
            return type is not null ? Escape(type.Name) : string.Empty;
        }

        /// <summary>
        /// Returns the name of the EdmFunction that is safe for use as an identifier.
        /// </summary>
        public static string Escape(EdmFunction function)
        {
            return function is not null ? Escape(function.Name) : string.Empty;
        }

        /// <summary>
        /// Returns the name of the EnumMember that is safe for use as an identifier.
        /// </summary>
        public static string Escape(EnumMember member)
        {
            return member is not null ? Escape(member.Name) : string.Empty;
        }

        /// <summary>
        /// Returns the name of the EntityContainer that is safe for use as an identifier.
        /// </summary>
        public static string Escape(EntityContainer container)
        {
            return container is not null ? Escape(container.Name) : string.Empty;
          }

        /// <summary>
        /// Returns the name of the EntitySet that is safe for use as an identifier.
        /// </summary>
        public static string Escape(EntitySet set)
        {
            return set is not null ? Escape(set.Name) : string.Empty;
        }

        /// <summary>
        /// Returns the name of the StructuralType that is safe for use as an identifier.
        /// </summary>
        public static string Escape(StructuralType type)
        {
            return type is not null ? Escape(type.Name) : string.Empty;
        }

        /// <summary>
        /// Returns the name of the Type object formatted for use in source code.
        /// </summary>
        /// <remarks>
        /// This method changes behavior based on the FullyQualifySystemTypes
        /// setting.
        /// </remarks>
        public static string Escape(Type clrType)
        {
            return Escape(clrType, FullyQualifySystemTypes);
        }

        /// <summary>
        /// Returns the name of the Type object formatted for use in source code.
        /// </summary>
        public static string Escape(Type clrType, bool fullyQualifySystemTypes)
        {
            if (clrType is null) return string.Empty;

            return fullyQualifySystemTypes ? "global::" + clrType.FullName : _code.GetTypeOutput(new CodeTypeReference(clrType));
        }

        #endregion

        /// <summary>
        /// Returns the NamespaceName with each segment safe to use as an identifier.
        /// </summary>
        public static string EscapeNamespace(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName)) return namespaceName;

            return namespaceName.Split('.').Aggregate("", (current, next) => $"{current}.{Escape(next)}");
        }

        #region FieldName

        /// <summary>
        /// Returns the name of the EdmMember formatted for
        /// use as a field identifier.
        ///
        /// This method changes behavior based on the CamelCaseFields
        /// setting.
        /// </summary>
        public static string FieldName(EdmMember member)
        {
            return member is not null ? FieldName(member.Name) : string.Empty;
        }

        /// <summary>
        /// Returns the name of the EntitySet formatted for
        /// use as a field identifier.
        ///
        /// This method changes behavior based on the CamelCaseFields
        /// setting.
        /// </summary>
        public static string FieldName(EntitySet set)
        {
            return set is not null ? FieldName(set.Name) : string.Empty;
        }

        /// <summary>
        /// Returns the name of the EntitySet formatted for
        /// use as a field identifier.
        ///
        /// This method changes behavior based on the CamelCaseFields
        /// setting.
        /// </summary>
        public static string FieldName(EntityType entityType)
        {
            return entityType is not null ? FieldName(entityType.Name) : string.Empty;
        }

        #endregion

        /// <summary>
        /// Returns the names of the items in the supplied collection that correspond to O-Space types.
        /// </summary>
        public static IEnumerable<string> GetAllGlobalItems(EdmItemCollection itemCollection)
        {
            Ensure.ArgumentNotNull(itemCollection, nameof(itemCollection));

            return itemCollection.GetItems<GlobalItem>().Where(i => i is EntityType || i is ComplexType || i is EnumType || i is EntityContainer).Select(g => GetGlobalItemName(g));
        }

        /// <summary>
        /// Returns the name of the supplied GlobalItem.
        /// </summary>
        public static string GetGlobalItemName(GlobalItem item)
        {
            Ensure.ArgumentNotNull(item, nameof(item));

            return item is EdmType ? ((EdmType)item).Name : ((EntityContainer)item).Name;
        }

        /// <summary>
        /// Gets the entity, complex, or enum types for which code should be generated from the given item collection. Any types for which an ExternalTypeName annotation 
        /// has been applied in the conceptual model metadata (CSDL) are filtered out of the returned list.
        /// </summary>
        /// <typeparam name="T">The type of item to return.</typeparam>
        /// <param name="itemCollection">The item collection to look in.</param>
        /// <returns>The items to generate.</returns>
        public static IEnumerable<T> GetItemsToGenerate<T>(ItemCollection itemCollection) where T : GlobalItem
        {
            Ensure.ArgumentNotNull(itemCollection, nameof(itemCollection));

            return itemCollection.GetItems<T>().Where(i => !i.MetadataProperties.Any(p => p.Name == ExternalTypeNameAttributeName));
        }

        #region GetTypeName

        /// <summary>
        /// Returns the escaped type name to use for the given usage of a c-space type in o-space. This might be an external type name if the ExternalTypeName annotation 
        /// has been specified in the conceptual model metadata (CSDL).
        /// </summary>
        /// <param name="typeUsage">The c-space type usage to get a name for.</param>
        /// <returns>The type name to use.</returns>
        public static string GetTypeName(TypeUsage typeUsage)
        {
            return typeUsage is null ? null : GetTypeName(typeUsage.EdmType, MetadataTools.IsNullable(typeUsage), modelNamespace: null);
        }

        /// <summary>
        /// Returns the escaped type name to use for the given c-space type in o-space. This might be an external type name if the ExternalTypeName annotation has been 
        /// specified in the conceptual model metadata (CSDL).
        /// </summary>
        /// <param name="edmType">The c-space type to get a name for.</param>
        /// <returns>The type name to use.</returns>
        public static string GetTypeName(EdmType edmType)
        {
            return GetTypeName(edmType, isNullable: null, modelNamespace: null);
        }

        /// <summary>
        /// Returns the escaped type name to use for the given usage of an c-space type in o-space. This might be an external type name if the ExternalTypeName annotation 
        /// has been specified in the conceptual model metadata (CSDL).
        /// </summary>
        /// <param name="typeUsage">The c-space type usage to get a name for.</param>
        /// <param name="modelNamespace">If not null and the type's namespace does not match this namespace, then a fully qualified name will be returned.</param>
        /// <returns>The type name to use.</returns>
        public static string GetTypeName(TypeUsage typeUsage, string modelNamespace)
        {
            return typeUsage is null ? null : GetTypeName(typeUsage.EdmType, MetadataTools.IsNullable(typeUsage), modelNamespace);
        }

        /// <summary>
        /// Returns the escaped type name to use for the given c-space type in o-space. This might be an external type name if the ExternalTypeName annotation has been specified 
        /// in the conceptual model metadata (CSDL).
        /// </summary>
        /// <param name="edmType">The c-space type to get a name for.</param>
        /// <param name="modelNamespace">If not null and the type's namespace does not match this namespace, then a fully qualified name will be returned.</param>
        /// <returns>The type name to use.</returns>
        public static string GetTypeName(EdmType edmType, string modelNamespace)
        {
            return GetTypeName(edmType, isNullable: null, modelNamespace: modelNamespace);
        }

        /// <summary>
        /// Returns the escaped type name to use for the given c-space type in o-space. This might be an external type name if the ExternalTypeName annotation has been specified 
        /// in the conceptual model metadata (CSDL).
        /// </summary>
        /// <param name="edmType">The c-space type to get a name for.</param>
        /// <param name="isNullable">Set this to true for nullable usage of this type.</param>
        /// <param name="modelNamespace">If not null and the type's namespace does not match this namespace, then a fully qualified name will be returned.</param>
        /// <returns>The type name to use.</returns>
        private static string GetTypeName(EdmType edmType, bool? isNullable, string modelNamespace)
        {
            if (edmType is null) return string.Empty;

            if (edmType is CollectionType collectionType)
            {
                return string.Format(CultureInfo.InvariantCulture, "ObservableCollection<{0}>", GetTypeName(collectionType.TypeUsage, modelNamespace));
            }

            // Try to get an external type name, and if that is null, then try to get escape the name from metadata,
            // possibly namespace-qualifying it.
            var typeName = Escape(edmType.MetadataProperties
                                  .Where(p => p.Name == ExternalTypeNameAttributeName)
                                  .Select(p => (string)p.Value)
                                  .FirstOrDefault())
                ??
                (modelNamespace is not null && edmType.NamespaceName != modelNamespace ?
                 CreateFullName(EscapeNamespace(edmType.NamespaceName), Escape(edmType)) :
                 Escape(edmType));

            if (edmType is StructuralType)
            {
                return typeName;
            }

            if (edmType is SimpleType)
            {
                var clrType = MetadataTools.UnderlyingClrType(edmType);
                if (!(edmType is EnumType))
                {
                    typeName = Escape(clrType);
                }

                return clrType.IsValueType && isNullable == true ?
                    string.Format(CultureInfo.InvariantCulture, "Nullable<{0}>", typeName) :
                    typeName;
            }

            throw new ArgumentException("typeUsage");
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessibility"></param>
        /// <returns></returns>
        public static string PropertyVirtualModifier(string accessibility)
        {
            return accessibility + (accessibility != "private" ? " virtual" : "");
        }

        /// <summary>
        /// If the value parameter is null or empty an empty string is returned, otherwise it retuns value with a single space concatenated on the end.
        /// </summary>
        public static string SpaceAfter(string value)
        {
            return StringAfter(value, " ");
        }

        /// <summary>
        /// If the value parameter is null or empty an empty string is returned, otherwise it retuns value with a single space concatenated on the end.
        /// </summary>
        public static string SpaceBefore(string value)
        {
            return StringBefore(" ", value);
        }

        /// <summary>
        /// If the value parameter is null or empty an empty string is returned, otherwise it retuns value with append concatenated on the end.
        /// </summary>
        public static string StringAfter(string value, string append)
        {
            return !string.IsNullOrWhiteSpace(value) ? $"{value}{append}" : string.Empty;
        }

        /// <summary>
        /// If the value parameter is null or empty an empty string is returned, otherwise it retuns value with prepend concatenated on the front.
        /// </summary>
        public static string StringBefore(string prepend, string value)
        {
            return !string.IsNullOrEmpty(value) ? $"{prepend}{value}" : string.Empty;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string FieldName(string name)
        {
            return $"_{(CamelCaseFields ? CamelCase(name) : name)}";
        }

        #endregion

    }

}
