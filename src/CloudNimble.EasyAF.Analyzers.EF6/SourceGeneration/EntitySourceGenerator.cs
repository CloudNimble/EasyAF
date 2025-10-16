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
    public class EntitySourceGenerator : SourceGeneratorBase
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edmxLoader"></param>
        /// <param name="settings">The generator settings.</param>
        public EntitySourceGenerator(EdmxLoader edmxLoader, SourceGeneratorSettings settings) : base(edmxLoader, settings)
        {
        }

        /// <summary>
        /// Generates the entity classes.
        /// </summary>
        /// <param name="context">The source production context.</param>
        public void Generate(SourceProductionContext context)
        {
            foreach (var entity in EdmxLoader.Entities)
            {
                var entitySource = new EntityGenerator([], Settings.CoreNamespace, entity);
                entitySource.Generate();
                context.AddSource($"{entity.EntityType.Name}.g.cs", SourceText.From(entitySource.ToString(), Encoding.UTF8));
            }
        }

    }

}
