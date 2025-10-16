using CloudNimble.EasyAF.CodeGen.Generators.Base;
using CloudNimble.EasyAF.CodeGen.Legacy;
using System.Collections.Generic;

namespace CloudNimble.EasyAF.CodeGen.Generators.Core
{

    /// <summary>
    /// 
    /// </summary>
    public class ManagerGenerator : EntityGeneratorBase
    {

        #region Private Members

        /// <summary>
        /// 
        /// </summary>
        public string DbContextName { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extraUsings"></param>
        /// <param name="managerNamespace"></param>
        /// <param name="entity"></param>
        /// <param name="dbContextName"></param>
        public ManagerGenerator(List<string> extraUsings, string managerNamespace, EntityComposition entity, string dbContextName)
            : base(extraUsings, managerNamespace, entity)
        {
            DbContextName = dbContextName;
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
            ClassBegin(CodeGenerationTools.ManagerClassDeclaration(Entity, DbContextName), "");
            WriteConstructors();
            RegionBegin("Public Methods");
            WriteFilter();
            WriteOverrides();
            RegionEnd();
            WritePartialMethods();
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
            return WriteFile($"{Entity.EntityType.Name}Manager", directory);
        }

        #endregion

        #region Private Methods

        internal void WriteConstructors()
        {
            RegionBegin("Constructors");
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("/// ");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine("/// <param name=\"dataContext\"></param>");
            _writer.WriteLine("/// <param name=\"messagePublisher\"></param>");
            _writer.WriteLine($"public {Entity.EntityType.Name}Manager({DbContextName} dataContext, IMessagePublisher messagePublisher) : base(dataContext, messagePublisher)");
            _writer.WriteLine("{");
            _writer.WriteLine("}");
            _writer.WriteLine();
            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteFilter()
        {
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Limits the results of <see cref=\"{CodeGenerationTools.Escape(Entity.EntityType)}\" /> queries by a pre-determined set of criteria.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"public IQueryable<{CodeGenerationTools.Escape(Entity.EntityType)}> OnFilter(IQueryable<{CodeGenerationTools.Escape(Entity.EntityType)}> entitySet)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("OnFilterInternal(ref entitySet);");
            _writer.WriteLine("return entitySet;");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
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
            _writer.WriteLine("await base.OnInsertingAsync(entity);");
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
            _writer.WriteLine("await base.OnUpdatingAsync(entity);");
            _writer.WriteLine("OnUpdatingInternal(entity);");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();

            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// Validate the {CodeGenerationTools.Escape(Entity.EntityType)} before it is deleted from the database.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{CodeGenerationTools.Escape(Entity.EntityType)}\" /> instance that is being deleted.</param>");
            _writer.WriteLine($"public override async Task OnDeletingAsync({CodeGenerationTools.Escape(Entity.EntityType)} entity)");
            _writer.WriteLine("{");
            _writer.Indent++;
            if (Entity.HasState || Entity.HasStatus)
            {
                _writer.WriteLine("Initialize();");
            }
            _writer.WriteLine("await base.OnDeletingAsync(entity);");
            _writer.WriteLine("OnDeletingInternal(entity);");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WritePartialMethods()
        {
            RegionBegin("Partial Methods");
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// If implemented outside this generated code, allows for additional business logic to run to further reduce the amount of data returned from the request.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entitySet\">The DbSet that needs to be filtered.</param>");
            _writer.WriteLine($"/// <param name=\"clientAppId\">If implemented, allows you to totally change the shape of the data based on the application calling this API.</param>");
            _writer.WriteLine($"partial void OnFilterInternal(ref IQueryable<{CodeGenerationTools.Escape(Entity.EntityType)}> entitySet, string clientAppId = null);");
            _writer.WriteLine();

            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// If implemented outside this generated code, allows for additional business logic to run before the {CodeGenerationTools.Escape(Entity.EntityType)} is committed to the database.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{CodeGenerationTools.Escape(Entity.EntityType)}\" /> instance that is being committed to the database.</param>");
            _writer.WriteLine($"partial void OnInsertingInternal({CodeGenerationTools.Escape(Entity.EntityType)} entity);");
            _writer.WriteLine();

            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// If implemented outside this generated code, allows for additional business logic to run before {CodeGenerationTools.Escape(Entity.EntityType)} edits are committed to the database.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{CodeGenerationTools.Escape(Entity.EntityType)}\" /> instance that is being edited.</param>");
            _writer.WriteLine($"partial void OnUpdatingInternal({CodeGenerationTools.Escape(Entity.EntityType)} entity);");
            _writer.WriteLine();

            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// If implemented outside this generated code, allows for additional business logic to run before the {CodeGenerationTools.Escape(Entity.EntityType)} is deleted from the database.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"entity\">The <see cref=\"{CodeGenerationTools.Escape(Entity.EntityType)}\" /> instance being committed to the database.</param>");
            _writer.WriteLine($"partial void OnDeletingInternal({CodeGenerationTools.Escape(Entity.EntityType)} entity);");
            _writer.WriteLine();
            RegionEnd();
        }

        //internal void WriteCascadeDelete()
        //{
        //    RegionBegin("Partial Methods");
        //    _writer.WriteLine("/// <summary>");
        //    _writer.WriteLine($"/// ");
        //    _writer.WriteLine("/// </summary>");
        //    _writer.WriteLine($"public void CascadeDelete(ref IQueryable<{CodeGenerationTools.Escape(Entity.EntityType)}> entitySet, string clientAppId = null);");
        //    _writer.WriteLine();
        //    RegionEnd();
        //}

        /// <summary>
        /// 
        /// </summary>
        internal void AddUsings()
        {
            ExtraUsings.Add("CloudNimble.EasyAF.Business");
            ExtraUsings.Add("CloudNimble.SimpleMessageBus.Publish");
            ExtraUsings.Add("System");
            ExtraUsings.Add("System.Linq");
            ExtraUsings.Add("System.Security.Claims");
            ExtraUsings.Add("System.Threading.Tasks");
        }

        #endregion

    }

}
