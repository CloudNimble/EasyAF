using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Generators.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudNimble.EasyAF.Analyzers.EF6.SourceGeneration
{

    /// <summary>
    /// Generates SimpleMessageBus message classes using incremental source generation.
    /// </summary>
    public class SimpleMessageBusSourceGenerator : SourceGeneratorBase
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMessageBusSourceGenerator"/> class.
        /// </summary>
        /// <param name="edmxLoader">The EDMX loader containing entity metadata.</param>
        /// <param name="settings">The source generator settings.</param>
        public SimpleMessageBusSourceGenerator(EdmxLoader edmxLoader, SourceGeneratorSettings settings) : base(edmxLoader, settings)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates SimpleMessageBus message classes for all entities.
        /// </summary>
        /// <param name="context">The source production context.</param>
        public void Generate(SourceProductionContext context)
        {
            List<string> extraUsings =
            [
                "System",
                "System.Collections.Generic",
                "System.Collections.Concurrent",
                "CloudNimble.SimpleMessageBus.Core",
                Settings.CoreNamespace,  // Use CoreNamespace for entity types
            ];

            // The generated files should go in the project's namespace (RootNamespace)
            var targetNamespace = Settings.ProjectNamespace;

            // Generate base class once
            var firstEntity = EdmxLoader.Entities.FirstOrDefault();
            if (firstEntity != null)
            {
                using var baseGenerator = new SimpleMessageBusGenerator(
                    extraUsings, 
                    targetNamespace,  // Use the project's namespace
                    firstEntity, 
                    "Base");
                
                baseGenerator.Generate();
                context.AddSource("DbEntityMessageBase.g.cs", SourceText.From(baseGenerator.ToString(), Encoding.UTF8));
            }

            // Generate message classes for each entity
            foreach (var entity in EdmxLoader.Entities)
            {
                // Generate Created message
                using var createdGenerator = new SimpleMessageBusGenerator(
                    extraUsings, 
                    targetNamespace,  // Use the project's namespace
                    entity, 
                    "Created");
                
                createdGenerator.Generate();
                context.AddSource($"{entity.EntityType.Name}Created.g.cs", SourceText.From(createdGenerator.ToString(), Encoding.UTF8));

                // Generate Updated message
                using var updatedGenerator = new SimpleMessageBusGenerator(
                    extraUsings, 
                    targetNamespace,  // Use the project's namespace
                    entity, 
                    "Updated");
                
                updatedGenerator.Generate();
                context.AddSource($"{entity.EntityType.Name}Updated.g.cs", SourceText.From(updatedGenerator.ToString(), Encoding.UTF8));

                // Generate Deleted message
                using var deletedGenerator = new SimpleMessageBusGenerator(
                    extraUsings, 
                    targetNamespace,  // Use the project's namespace
                    entity, 
                    "Deleted");
                
                deletedGenerator.Generate();
                context.AddSource($"{entity.EntityType.Name}Deleted.g.cs", SourceText.From(deletedGenerator.ToString(), Encoding.UTF8));
            }
        }

        #endregion

    }

}
