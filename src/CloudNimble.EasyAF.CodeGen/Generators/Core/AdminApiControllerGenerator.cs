using CloudNimble.EasyAF.CodeGen.Legacy;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace CloudNimble.EasyAF.CodeGen.Generators.Core
{

    /// <summary>
    /// 
    /// </summary>
    public class AdminApiControllerGenerator : ApiControllerGenerator
    {

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public EdmxLoader EdmxLoader { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the AdminApiControllerGenerator class.
        /// </summary>
        /// <param name="extraUsings">Additional using statements to include.</param>
        /// <param name="controllerNamespace">The namespace for the generated controller.</param>
        /// <param name="loader">The EdmxLoader containing the entity model.</param>
        /// <param name="isEFCore">Whether the target is Entity Framework Core.</param>
        /// <param name="addInheritance">Whether to include inheritance in the class declaration.</param>
        /// <param name="baseClass">The name of the base class to inherit from.</param>
        public AdminApiControllerGenerator(List<string> extraUsings, string controllerNamespace, EdmxLoader loader, bool isEFCore, bool addInheritance = true, string baseClass = null)
            : base(extraUsings, controllerNamespace, loader.EntityContainer, isEFCore, addInheritance, baseClass)
        {
            EdmxLoader = loader;
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
            ClassBegin(CodeGenerationTools.AdminControllerClassDeclaration(EntityContainer.Name, _addInheritance, _baseClass), "");
            WriteFields();
            WriteProperties();
            WriteConstructors(true);

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
        public new string WriteFile(string directory = null)
        {
            return WriteFile($"{EntityContainer.Name}AdminApi", directory);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        internal void WriteFields()
        {
            RegionBegin("Private Members");
            foreach (var entity in EdmxLoader.Entities)
            {
                _writer.WriteLine($"private {CodeGenerationTools.Escape(entity.EntityType)}Manager {CodeGenerationTools.FieldName(entity.EntityType)}Manager;");
            }
            _writer.WriteLine();
            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteProperties()
        {
            RegionBegin("Public Properties");
            foreach (var entity in EdmxLoader.Entities)
            {
                _writer.WriteLine("/// <summary>");
                _writer.WriteLine("///");
                _writer.WriteLine("/// </summary>");
                _writer.WriteLine($"public {CodeGenerationTools.Escape(entity.EntityType)}Manager {CodeGenerationTools.Escape(entity.EntityType)}Manager");
                _writer.WriteLine("{");
                _writer.Indent++;
                _writer.WriteLine("get");
                _writer.WriteLine("{");
                _writer.Indent++;
                _writer.WriteLine($"if ({CodeGenerationTools.FieldName(entity.EntityType)}Manager is null)");
                _writer.WriteLine("{");
                _writer.Indent++;
                _writer.WriteLine($"{CodeGenerationTools.FieldName(entity.EntityType)}Manager = ServiceProvider.GetService<{CodeGenerationTools.Escape(entity.EntityType)}Manager>();");
                _writer.Indent--;
                _writer.WriteLine("}");
                _writer.WriteLine($"return {CodeGenerationTools.FieldName(entity.EntityType)}Manager;");
                _writer.Indent--;
                _writer.WriteLine("}");
                _writer.Indent--;
                _writer.WriteLine("}");
                _writer.WriteLine();
            }
            RegionEnd();
        }


        #endregion

    }

}
