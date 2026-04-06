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
    public class ModelBuilderGenerator : ContainerGeneratorBase
    {

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extraUsings"></param>
        /// <param name="controllerNamespace"></param>
        /// <param name="container"></param>
        public ModelBuilderGenerator(List<string> extraUsings, string controllerNamespace, EntityContainer container)
            : base(extraUsings, controllerNamespace, container)
        {
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
            ClassBegin(CodeGenerationTools.ModelBuilderClassDeclaration(EntityContainer), "");
            WriteConfigure();
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
            return WriteFile($"{CodeGenerationTools.Escape(EntityContainer)}ModelBuilder", directory);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        internal void WriteConfigure()
        {
            RegionBegin("Public Methods");
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("/// ");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine("/// <param name=\"context\"></param>");
            _writer.WriteLine("/// <returns></returns>");
            _writer.WriteLine("public IEdmModel GetModel(ModelContext context)");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("var modelBuilder = new ODataConventionModelBuilder();");
            foreach (var entitySet in EntityContainer.BaseEntitySets.OfType<EntitySet>().OrderBy(c => c.Name))
            {
                _writer.WriteLine($"modelBuilder.EntitySet<{CodeGenerationTools.GetTypeName(entitySet.ElementType)}>(\"{CodeGenerationTools.Escape(entitySet)}\").IgnoreTrackingFields();");
            }
            _writer.WriteLine("ExtendModel(modelBuilder);");
            _writer.WriteLine("return modelBuilder.GetEdmModel();");
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
            _writer.WriteLine($"/// If implemented outside this generated code, allows for the partial class to register additional resoucres on the model.");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"/// <param name=\"modelBuilder\">The ODataModelBuilder instance to add models data to.</param>");
            _writer.WriteLine($"partial void ExtendModel(ODataModelBuilder modelBuilder);");
            _writer.WriteLine();
            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void AddUsings()
        {
            ExtraUsings.Add("Microsoft.AspNet.OData.Builder");
            ExtraUsings.Add("Microsoft.OData.Edm");
            ExtraUsings.Add("Microsoft.Restier.Core.Model");
            //ExtraUsings.Add("System.Threading");
            //ExtraUsings.Add("System.Threading.Tasks");
        }

        #endregion

    }

}
