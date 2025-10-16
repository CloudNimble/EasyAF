using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Generators.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Text;

namespace CloudNimble.EasyAF.Analyzers.EF6.SourceGeneration
{

    /// <summary>
    /// 
    /// </summary>
    public class BusinessSourceGenerator : SourceGeneratorBase
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edmxLoader"></param>
        /// <param name="settings">The generator settings.</param>
        public BusinessSourceGenerator(EdmxLoader edmxLoader, SourceGeneratorSettings settings) : base(edmxLoader, settings)
        {
        }

        /// <summary>
        /// Generates the entity classes.
        /// </summary>
        /// <param name="context">The source production context.</param>
        public void Generate(SourceProductionContext context)
        {
            List<string> extraUsings =
            [
                Settings.CoreNamespace, 
                Settings.DataNamespace
            ];

            using var businessDI = new BusinessDependencyGenerator([Settings.BusinessNamespace], Settings.BusinessNamespace, EdmxLoader.EntityContainer);
            businessDI.Generate();
            context.AddSource($"{businessDI.ProjectName}Business_IServiceCollectionExtensions.g.cs", SourceText.From(businessDI.ToString(), Encoding.UTF8));

            foreach (var entity in EdmxLoader.Entities)
            {
                using var manager = new ManagerGenerator(extraUsings, Settings.BusinessNamespace, entity, EdmxLoader.EntityContainer.Name);
                manager.Generate();
                context.AddSource($"{entity.EntityType.Name}Manager.g.cs", SourceText.From(manager.ToString(), Encoding.UTF8));
            }
        }

    }

}
