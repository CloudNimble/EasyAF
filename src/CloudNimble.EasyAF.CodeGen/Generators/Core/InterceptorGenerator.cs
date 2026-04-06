using CloudNimble.EasyAF.CodeGen.Generators.Base;
using CloudNimble.EasyAF.CodeGen.Legacy;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace CloudNimble.EasyAF.CodeGen.Generators.Core
{

    /// <summary>
    /// 
    /// </summary>
    public class InterceptorGenerator : EntityGeneratorBase
    {

        #region Private Members

        /// <summary>
        /// 
        /// </summary>
        public EntityContainer EntityContainer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string TypeName => CodeGenerationTools.Escape(Entity.EntityType);

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extraUsings"></param>
        /// <param name="controllerNamespace"></param>
        /// <param name="container"></param>
        /// <param name="entity"></param>
        public InterceptorGenerator(List<string> extraUsings, string controllerNamespace, EntityContainer container, EntityComposition entity) : base(extraUsings, controllerNamespace, entity)
        {
            EntityContainer = container;
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
            ClassBegin(CodeGenerationTools.ControllerClassDeclaration(EntityContainer.Name), "");
            WriteFields();
            WriteProperties();
            WriteMethodAuthorization();
            WriteFilter();
            WriteInterceptors();
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
            return WriteFile($"{Entity.EntityType.Name}Interceptors", directory);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        internal void WriteFields()
        {
            RegionBegin("Private Members");
            _writer.WriteLine($"private {TypeName}Manager {CodeGenerationTools.FieldName(Entity.EntityType)}Manager;");
            _writer.WriteLine();
            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteFilter()
        {
            RegionBegin("EntitySet Filter");
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Limits the results of <see cref=\"{TypeName}\" /> queries by a pre-determined set of criteria.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"protected internal IQueryable<{TypeName}> OnFilter{EntityContainer.EntitySets.FirstOrDefault(c => c.ElementType == Entity.EntityType).Name}(IQueryable<{TypeName}> entitySet)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine($"RestierHelpers.LogOperation(\"{TypeName}\", RestierOperationType.Filtered);");
            _writer.WriteLine($"return {TypeName}Manager.OnFilter(entitySet);");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteInterceptor(string eventName)
        {
            var isAsync = eventName.EndsWith("ed");
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"///");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{TypeName}\" /> instance.</param>");
            _writer.WriteLine($"protected internal async Task On{eventName}{TypeName}Async({TypeName} entity)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine($"await {TypeName}Manager.On{eventName}Async(entity);");
            _writer.WriteLine($"RestierHelpers.LogOperation(entity, RestierOperationType.{eventName});");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteInterceptors()
        {
            RegionBegin("Interceptors");
            WriteInterceptor("Inserting");
            WriteInterceptor("Inserted");
            WriteInterceptor("Updating");
            WriteInterceptor("Updated");
            WriteInterceptor("Deleting");
            WriteInterceptor("Deleted");
            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteMethodAuthorization()
        {
            RegionBegin("Method Authorization");
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"///");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"protected internal bool CanInsert{TypeName}() => AuthorizationFactory.ForType<{TypeName}>().CanInsertAction();");
            _writer.WriteLine();
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"///");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"protected internal bool CanUpdate{TypeName}() => AuthorizationFactory.ForType<{TypeName}>().CanUpdateAction();");
            _writer.WriteLine();
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"///");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"protected internal bool CanDelete{TypeName}() => AuthorizationFactory.ForType<{TypeName}>().CanDeleteAction();");
            _writer.WriteLine();
            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteOverrides()
        {
            RegionBegin("Object Validation");
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Validate the {CodeGenerationTools.Escape(Entity.EntityType)} before it is inserted into the database.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{CodeGenerationTools.Escape(Entity.EntityType)}\" /> instance that is being inserted.</param>");
            _writer.WriteLine($"public override async Task OnInsertingAsync({CodeGenerationTools.Escape(Entity.EntityType)} entity)");
            _writer.WriteLine("{");
            _writer.Indent++;
            if (Entity.HasState || Entity.HasStatus)
            {
                _writer.WriteLine("Initialize();");
            }
            _writer.WriteLine("base.OnInserting(entity);");
            _writer.WriteLine("OnInsertingInternal(entity);");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();

            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Validate the {CodeGenerationTools.Escape(Entity.EntityType)} before it is updated in the database.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{CodeGenerationTools.Escape(Entity.EntityType)}\" /> instance that is being updated.</param>");
            _writer.WriteLine($"public override async Task OnUpdatingAsync({CodeGenerationTools.Escape(Entity.EntityType)} entity)");
            _writer.WriteLine("{");
            _writer.Indent++;
            if (Entity.HasState || Entity.HasStatus)
            {
                _writer.WriteLine("Initialize();");
            }
            _writer.WriteLine("base.OnUpdating(entity);");
            _writer.WriteLine("OnUpdatingInternal(entity);");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteProperties()
        {
            RegionBegin("Public Properties");
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("///");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"public {TypeName}Manager {TypeName}Manager");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("get");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine($"if ({CodeGenerationTools.FieldName(Entity.EntityType)}Manager is null)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine($"{CodeGenerationTools.FieldName(Entity.EntityType)}Manager = ServiceProvider.GetService<{TypeName}Manager>();");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine($"return {CodeGenerationTools.FieldName(Entity.EntityType)}Manager;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void AddUsings()
        {
            ExtraUsings.Add("CloudNimble.EasyAF.Restier");
            ExtraUsings.Add("Microsoft.Extensions.DependencyInjection");
            ExtraUsings.Add("Microsoft.Restier.Core.Authorization");
            ExtraUsings.Add("System.Linq");
            ExtraUsings.Add("System.Threading.Tasks");
        }

        #endregion

    }

}
