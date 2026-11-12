using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Generators.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
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
        /// <param name="edmxFile">The EDMX file the entities come from, used to point diagnostics at the offending line.</param>
        public void Generate(SourceProductionContext context, AdditionalText edmxFile)
        {
            var edmxText = edmxFile?.GetText(context.CancellationToken);

            foreach (var entity in EdmxLoader.Entities)
            {
                if (entity.HasNullableCreatedById)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            SourceGeneratorConstants.NullableCreatedByIdDiagnosticId,
                            SourceGeneratorConstants.NullableCreatedByIdTitle,
                            SourceGeneratorConstants.NullableCreatedByIdMessage,
                            SourceGeneratorConstants.EasyAFCategory,
                            DiagnosticSeverity.Warning,
                            isEnabledByDefault: true),
                        GetConceptualPropertyLocation(edmxFile?.Path, edmxText, entity.EntityType.Name, "CreatedById"),
                        entity.EntityType.Name));
                }

                var entitySource = new EntityGenerator([], Settings.CoreNamespace, entity);
                entitySource.Generate();
                context.AddSource($"{entity.EntityType.Name}.g.cs", SourceText.From(entitySource.ToString(), Encoding.UTF8));
            }
        }

        /// <summary>
        /// Finds a property declaration in the EDMX's conceptual model (not the storage model), so a diagnostic can point at it.
        /// </summary>
        /// <param name="path">The EDMX file path.</param>
        /// <param name="text">The EDMX file contents.</param>
        /// <param name="entityName">The conceptual Entity name.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The location of the property's <c>&lt;Property&gt;</c> element, or <see cref="Location.None"/> if it can't be found.</returns>
        private static Location GetConceptualPropertyLocation(string path, SourceText text, string entityName, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(path) || text is null)
            {
                return Location.None;
            }

            var content = text.ToString();
            var conceptualStart = content.IndexOf("<edmx:ConceptualModels>", StringComparison.Ordinal);
            if (conceptualStart < 0)
            {
                return Location.None;
            }

            var entityStart = content.IndexOf($"<EntityType Name=\"{entityName}\"", conceptualStart, StringComparison.Ordinal);
            var entityEnd = entityStart < 0 ? -1 : content.IndexOf("</EntityType>", entityStart, StringComparison.Ordinal);
            if (entityEnd < 0)
            {
                return Location.None;
            }

            var propertyStart = content.IndexOf($"<Property Name=\"{propertyName}\"", entityStart, entityEnd - entityStart, StringComparison.Ordinal);
            if (propertyStart < 0)
            {
                return Location.None;
            }

            var propertyEnd = content.IndexOf("/>", propertyStart, StringComparison.Ordinal);
            var span = TextSpan.FromBounds(propertyStart, propertyEnd < 0 ? propertyStart : propertyEnd + 2);
            return Location.Create(path, span, text.Lines.GetLinePositionSpan(span));
        }

    }

}
