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
    public class AuthorizationGenerator : ContainerGeneratorBase
    {

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extraUsings"></param>
        /// <param name="controllerNamespace"></param>
        /// <param name="container"></param>
        public AuthorizationGenerator(List<string> extraUsings, string controllerNamespace, EntityContainer container) : base(extraUsings, controllerNamespace, container)
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
            ClassBegin(CodeGenerationTools.AuthorizationClassDeclaration(EntityContainer), "");
            WriteConfigure();
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
            return WriteFile($"{CodeGenerationTools.Escape(EntityContainer)}AuthorizationConfig", directory);
        }

        #endregion

        #region Private Methods

        internal void WriteConfigure()
        {
            RegionBegin("Public Methods");
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("///");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"public static void Configure()");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("bool trueAction() => true;");
            _writer.WriteLine("bool adminAction() => ClaimsPrincipal.Current.IsInRole(\"Admin\");");
            _writer.WriteLine();
            _writer.WriteLine("var entries = new List<AuthorizationEntry>");
            _writer.WriteLine("{");
            _writer.Indent++;
            foreach (var entitySet in EntityContainer.BaseEntitySets.OfType<EntitySet>().OrderBy(c => c.Name))
            {
                _writer.WriteLine($"new AuthorizationEntry(typeof({CodeGenerationTools.GetTypeName(entitySet.ElementType)}), trueAction, adminAction, adminAction),");
            }
            _writer.Indent--;
            _writer.WriteLine("};");
            _writer.WriteLine("AuthorizationFactory.RegisterEntries(entries);");
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
            ExtraUsings.Add("Microsoft.Extensions.DependencyInjection");
            ExtraUsings.Add("Microsoft.Restier.Core.Authorization");
            ExtraUsings.Add("System.Collections.Generic");
            ExtraUsings.Add("System.Linq");
            ExtraUsings.Add("System.Security.Claims");
        }

        #endregion

    }

}
