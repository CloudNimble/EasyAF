using CloudNimble.EasyAF.CodeGen.Generators.Base;
using CloudNimble.EasyAF.CodeGen.Legacy;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace CloudNimble.EasyAF.CodeGen.Generators.Core
{

    /// <summary>
    /// 
    /// </summary>
    public class BusinessDependencyGenerator : ContainerGeneratorBase
    {

        /// <summary>
        /// 
        /// </summary>
        public string ProjectName { get; private set; }

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extraUsings"></param>
        /// <param name="businessNamespace"></param>
        /// <param name="container"></param>
        public BusinessDependencyGenerator(List<string> extraUsings, string businessNamespace, EntityContainer container)
            : base(extraUsings, businessNamespace, container)
        {
            AddUsings();
            var parts = businessNamespace.Split('.');
            if (parts.Length < 2)
            {
                // Handle error: not enough parts
                throw new ArgumentException("businessNamespace must contain at least two segments.", nameof(businessNamespace));
            }
#if NET8_0_OR_GREATER
            var secondToLast = parts[^2];
#else

            var secondToLast = parts[parts.Length - 2];
#endif
            ProjectName = secondToLast;
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
            NamespaceBegin("Microsoft.Extensions.DependencyInjection");
            ClassBegin(CodeGenerationTools.BusinessDependencyClassDeclaration(ProjectName), "");
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
            return WriteFile($"{ProjectName}Business_IServiceCollectionExtensions", directory);
        }

        #endregion

        #region Private Methods

        internal void WriteConfigure()
        {
            RegionBegin("Public Methods");
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("///");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"public static IServiceCollection Add{ProjectName}BusinessDependencies(this IServiceCollection services)");
            _writer.WriteLine("{");
            _writer.Indent++;
            foreach (var entitySet in EntityContainer.BaseEntitySets.OfType<EntitySet>().OrderBy(c => c.Name))
            {
                _writer.WriteLine($"services.AddScoped<{CodeGenerationTools.GetTypeName(entitySet.ElementType)}Manager>();");
            }
            _writer.WriteLine("return services;");
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
        }

        #endregion

    }

}
