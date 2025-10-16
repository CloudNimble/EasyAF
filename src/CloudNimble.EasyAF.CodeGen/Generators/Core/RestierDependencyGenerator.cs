using CloudNimble.EasyAF.CodeGen.Generators.Base;
using CloudNimble.EasyAF.CodeGen.Legacy;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace CloudNimble.EasyAF.CodeGen.Generators.Core
{

    /// <summary>
    /// 
    /// </summary>
    public class RestierDependencyGenerator : ContainerGeneratorBase
    {

        #region Fields

        private readonly bool _isEFCore;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public string ProjectName { get; private set; }

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extraUsings"></param>
        /// <param name="apiNamespace"></param>
        /// <param name="container"></param>
        /// <param name="isEFCore"></param>
        public RestierDependencyGenerator(List<string> extraUsings, string apiNamespace, EntityContainer container, bool isEFCore)
            : base(extraUsings, apiNamespace, container)
        {
            AddUsings();
            var parts = apiNamespace.Split('.');
            if (parts.Length < 2)
            {
                // Handle error: not enough parts
                throw new ArgumentException("apiNamespace must contain at least two segments.", nameof(apiNamespace));
            }
#if NET8_0_OR_GREATER
            var secondToLast = parts[^2];
#else

            var secondToLast = parts[parts.Length - 2];
#endif
            ProjectName = secondToLast;
            _isEFCore = isEFCore;
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
            ClassBegin(CodeGenerationTools.RestierDependencyClassDeclaration(ProjectName), "");
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
            return WriteFile($"{ProjectName}Restier_IServiceCollectionExtensions", directory);
        }

        #endregion

        #region Private Methods

        internal void WriteConfigure()
        {
            RegionBegin("Public Methods");
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine("///");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"public static IServiceCollection Add{ProjectName}RestierCoreDependencies(this IServiceCollection services, IConfiguration configuration)");
            _writer.WriteLine("{");
            _writer.WriteLine("return services");
            _writer.Indent++;
            _writer.WriteLine(".AddHttpContextAccessor()");
            _writer.WriteLine(".AddScoped(sp => configuration)");
            if (_isEFCore)
            {
                _writer.WriteLine($".AddEFCoreProviderServices<{CodeGenerationTools.Escape(EntityContainer)}>()");
            }
            else
            {
                _writer.WriteLine($".AddEF6ProviderServices<{CodeGenerationTools.Escape(EntityContainer)}>()");
            }
            _writer.WriteLine($".AddChainedService<IModelBuilder, {CodeGenerationTools.Escape(EntityContainer)}ModelBuilder>()");
            _writer.WriteLine(".AddSingleton(new ODataValidationSettings");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine("MaxTop = 100,");
            _writer.WriteLine("MaxAnyAllExpressionDepth = 4,");
            _writer.WriteLine("MaxExpansionDepth = 4,");
            _writer.Indent--;
            _writer.WriteLine("})");
            _writer.WriteLine($".Add{ProjectName}BusinessDependencies();");
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
            ExtraUsings.Add("Microsoft.AspNet.OData.Query");
            ExtraUsings.Add("Microsoft.Extensions.Configuration");
            ExtraUsings.Add("Microsoft.Restier.Core.Model");
        }

        #endregion

    }

}
