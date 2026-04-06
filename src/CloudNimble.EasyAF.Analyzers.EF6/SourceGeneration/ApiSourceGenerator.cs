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
    public class ApiSourceGenerator : SourceGeneratorBase
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edmxLoader"></param>
        /// <param name="settings">The generator settings.</param>
        public ApiSourceGenerator(EdmxLoader edmxLoader, SourceGeneratorSettings settings) : base(edmxLoader, settings)
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
                Settings.DataNamespace,
                Settings.BusinessNamespace
            ];

            List<string> extraUsings2 =
            [
                Settings.ApiNamespace,
                Settings.DataNamespace,
            ];

            List<string> apiExtraUsings = [.. extraUsings];
            if (!string.IsNullOrWhiteSpace(Settings.ApiAdditionalUsings))
            {
                var additionalUsings = Settings.ApiAdditionalUsings.Split(';');
                foreach (var usingStatement in additionalUsings)
                {
                    if (!string.IsNullOrWhiteSpace(usingStatement))
                    {
                        apiExtraUsings.Add(usingStatement.Trim());
                    }
                }
            }

            using var restierDI = new RestierDependencyGenerator(extraUsings2, Settings.ApiNamespace, EdmxLoader.EntityContainer, EdmxLoader.IsEFCore);
            restierDI.Generate();
            context.AddSource($"{restierDI.ProjectName}Restier_IServiceCollectionExtensions.g.cs", SourceText.From(restierDI.ToString(), Encoding.UTF8));

            using var authorization = new AuthorizationGenerator(extraUsings, Settings.ApiNamespace, EdmxLoader.EntityContainer);
            authorization.Generate();
            context.AddSource($"{EdmxLoader.EntityContainer.Name}AuthorizationConfig.g.cs", SourceText.From(authorization.ToString(), Encoding.UTF8));

            using var modelBuilder = new ModelBuilderGenerator(extraUsings, Settings.ApiNamespace, EdmxLoader.EntityContainer);
            modelBuilder.Generate();
            context.AddSource($"{EdmxLoader.EntityContainer.Name}ModelBuilder.g.cs", SourceText.From(modelBuilder.ToString(), Encoding.UTF8));

            using var apiController = new ApiControllerGenerator(apiExtraUsings, $"{Settings.ApiNamespace}.Controllers", EdmxLoader.EntityContainer, EdmxLoader.IsEFCore, Settings.ApiInheritance, Settings.ApiBaseClass);
            apiController.Generate();
            context.AddSource($"{EdmxLoader.EntityContainer.Name}Controller.g.cs", SourceText.From(apiController.ToString(), Encoding.UTF8));

            using var adminApiController = new AdminApiControllerGenerator(apiExtraUsings, $"{Settings.ApiNamespace}.Controllers", EdmxLoader, EdmxLoader.IsEFCore, Settings.AdminApiInheritance, Settings.AdminApiBaseClass);
            adminApiController.Generate();
            context.AddSource($"{EdmxLoader.EntityContainer.Name}AdminController.g.cs", SourceText.From(adminApiController.ToString(), Encoding.UTF8));

            foreach (var entity in EdmxLoader.Entities)
            {
                using var interceptors = new InterceptorGenerator(extraUsings, $"{Settings.ApiNamespace}.Controllers", EdmxLoader.EntityContainer, entity);
                interceptors.Generate();
                context.AddSource($"{entity.EntityType.Name}Interceptors.g.cs", SourceText.From(interceptors.ToString(), Encoding.UTF8));
            }

        }

    }

}
