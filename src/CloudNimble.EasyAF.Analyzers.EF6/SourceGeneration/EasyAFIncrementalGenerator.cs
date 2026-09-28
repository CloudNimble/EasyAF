using CloudNimble.EasyAF.CodeGen;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;

namespace CloudNimble.EasyAF.Analyzers.EF6.SourceGeneration
{

    /// <summary>
    /// 
    /// </summary>
#pragma warning disable RS1038 // Compiler extensions should be implemented in assemblies with compiler-provided references
    [Generator]
#pragma warning restore RS1038 // Compiler extensions should be implemented in assemblies with compiler-provided references
    public class EasyAFIncrementalGenerator : IIncrementalGenerator
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            try
            {
                DbConfiguration.SetConfiguration(new EF6Configuration());
            }
            catch (Exception ex)
            {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
                Console.WriteLine(ex);
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            }

            var options = SourceGeneratorSettings.FromContext(context);

            // Register the generator logic
            var edmxFiles = context.AdditionalTextsProvider
                .Where(file => file.Path.EndsWith(".edmx", StringComparison.OrdinalIgnoreCase));

            var compilationAndEdmxFiles = context.CompilationProvider.Combine(edmxFiles.Collect()).Combine(options);

            context.RegisterSourceOutput(compilationAndEdmxFiles, Execute);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="args"></param>
        private void Execute(SourceProductionContext context, ((Compilation compilation, ImmutableArray<AdditionalText> edmxFiles), SourceGeneratorSettings settings) args)
        {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            Console.WriteLine("EASYAF: Executing EasyAFIncrementalGenerator");
#pragma warning restore RS1035 // Do not use APIs banned for analyzers

            ((var compilation, var edmxFiles), var settings) = args;

#if DEBUG
            // Debug builds of the analyzer only, and opt-in: set <EasyAFLaunchDebugger>true</EasyAFLaunchDebugger> in the consuming project.
            if (settings.LaunchDebugger && !Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif

            if (settings.UsesLegacyGenerateViews)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        SourceGeneratorConstants.LegacyGenerateViewsDiagnosticId,
                        SourceGeneratorConstants.LegacyGenerateViewsTitle,
                        SourceGeneratorConstants.LegacyGenerateViewsMessage,
                        SourceGeneratorConstants.SourceGenerationCategory,
                        DiagnosticSeverity.Warning,
                        isEnabledByDefault: true),
                    Location.None));
            }

            if (settings.ProjectType is ProjectType.Unknown)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        SourceGeneratorConstants.ProjectTypeMissingDiagnosticId,
                        SourceGeneratorConstants.ProjectTypeMissingTitle,
                        SourceGeneratorConstants.ProjectTypeMissingMessage,
                        SourceGeneratorConstants.SourceGenerationCategory,
                        DiagnosticSeverity.Warning,
                        isEnabledByDefault: true),
                    Location.None));
                return;
            }
            else
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        SourceGeneratorConstants.ProjectTypeFoundDiagnosticId,
                        SourceGeneratorConstants.ProjectTypeFoundTitle,
                        SourceGeneratorConstants.ProjectTypeFoundMessage,
                        SourceGeneratorConstants.SourceGenerationCategory,
                        DiagnosticSeverity.Info,
                        isEnabledByDefault: true),
                    Location.None,
                    settings.ProjectType));
            }

            if (edmxFiles.Count() == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        SourceGeneratorConstants.EdmxFilesMissingDiagnosticId,
                        SourceGeneratorConstants.EdmxFilesMissingTitle,
                        SourceGeneratorConstants.EdmxFilesMissingMessage,
                        SourceGeneratorConstants.SourceGenerationCategory,
                        DiagnosticSeverity.Warning,
                        isEnabledByDefault: true),
                    Location.None));
                return;
            }

            foreach (var edmxFile in edmxFiles)
            {
                var edmxContent = edmxFile.GetText(context.CancellationToken)?.ToString();

                if (string.IsNullOrEmpty(edmxContent))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            SourceGeneratorConstants.EdmxFileEmptyDiagnosticId,
                            SourceGeneratorConstants.EdmxFileEmptyTitle,
                            SourceGeneratorConstants.EdmxFileEmptyMessage,
                            SourceGeneratorConstants.SourceGenerationCategory,
                            DiagnosticSeverity.Warning,
                            isEnabledByDefault: true),
                        Location.None,
                        edmxFile.Path));
                    continue;
                }

                var edmxLoader = new EdmxLoader(edmxFile.Path);
                edmxLoader.Load(edmxContent);

                if (edmxLoader.EdmxSchemaErrors.Count > 0)
                {
                    foreach (var error in edmxLoader.EdmxSchemaErrors)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            new DiagnosticDescriptor(
                                SourceGeneratorConstants.EdmxSchemaErrorDiagnosticId,
                                SourceGeneratorConstants.EdmxSchemaErrorTitle,
                                SourceGeneratorConstants.EdmxSchemaErrorMessage,
                                SourceGeneratorConstants.SourceGenerationCategory,
                                DiagnosticSeverity.Warning,
                                isEnabledByDefault: true),
                            Location.None,
                            edmxFile.Path,
                            error));
                    }
                    continue;
                }

                switch (settings.ProjectType)
                {
                    case ProjectType.Api:
                        new ApiSourceGenerator(edmxLoader, settings).Generate(context);
                        break;

                    case ProjectType.Business:
                        new BusinessSourceGenerator(edmxLoader, settings).Generate(context);
                        break;

                    case ProjectType.Core:
                        new EntitySourceGenerator(edmxLoader, settings).Generate(context, edmxFile);
                        break;

                    case ProjectType.Data:
                        new DataSourceGenerator(edmxLoader, settings).Generate(context);
                        break;

                    case ProjectType.SimpleMessageBus:
                        new SimpleMessageBusSourceGenerator(edmxLoader, settings).Generate(context);
                        break;

                    default:
                        context.ReportDiagnostic(Diagnostic.Create(
                            new DiagnosticDescriptor(
                                SourceGeneratorConstants.ProjectTypeUnsupportedDiagnosticId,
                                SourceGeneratorConstants.ProjectTypeUnsupportedTitle,
                                SourceGeneratorConstants.ProjectTypeUnsupportedMessage,
                                SourceGeneratorConstants.EasyAFCategory,
                                DiagnosticSeverity.Error,
                                isEnabledByDefault: true),
                            Location.None,
                            settings.ProjectType));
                        break;
                }
            }

        }

    }

}
