using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Generators.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CloudNimble.EasyAF.Analyzers.EF6.SourceGeneration
{

    /// <summary>
    /// 
    /// </summary>
    public class DataSourceGenerator : SourceGeneratorBase
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edmxLoader"></param>
        /// <param name="settings">The generator settings.</param>
        public DataSourceGenerator(EdmxLoader edmxLoader, SourceGeneratorSettings settings) : base(edmxLoader, settings)
        {
        }

        /// <summary>
        /// Generates the entity classes.
        /// </summary>
        /// <param name="context">The source production context.</param>
        public void Generate(SourceProductionContext context)
        {
            using var dbContext = new DbContextPartialGenerator([Settings.CoreNamespace], Settings.DataNamespace, EdmxLoader.EntityContainer, EdmxLoader.OnModelCreatingMethod, EdmxLoader.FilePath);
            dbContext.Generate();
            context.AddSource($"{ EdmxLoader.EntityContainer.Name}.g.cs", SourceText.From(dbContext.ToString(), Encoding.UTF8));

            //if (!Settings.GenerateViews) return;

            //using var views = new DbViewGenerator([], Settings.DataNamespace, EdmxLoader.EntityContainer, EdmxLoader.Mappings);
            //views.Generate();
            //context.AddSource($"{ EdmxLoader.EntityContainer.Name}.Views.g.cs", SourceText.From(views.ToString(), Encoding.UTF8));
        }

    }

}
