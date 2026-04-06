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
#if DEBUG
            if (!Debugger.IsAttached) Debugger.Launch();
#endif
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

            if (settings.ProjectType is ProjectType.Unknown)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "EASYAF001",
                        "EasyAFProjectType not defined",
                        "The EasyAFProjectType property is not defined in the project file",
                        "SourceGeneration",
                        DiagnosticSeverity.Warning,
                        isEnabledByDefault: true),
                    Location.None));
                return;
            }
            else
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "EASYAF001",
                        "EasyAFProjectType found.",
                        $"The EasyAFProjectType property is {settings.ProjectType}",
                        "SourceGeneration",
                        DiagnosticSeverity.Info,
                        isEnabledByDefault: true),
                    Location.None));
            }

            if (edmxFiles.Count() == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "EASYAF002",
                        "EDMX files not found.",
                        "There were no EDMX files found in the project. Please add an 'AdditionalFiles' node to an ItemGroup that references one or more EDMX files and try again.",
                        "SourceGeneration",
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
                            "EASYAF003",
                            "EDMX file has no content.",
                            $"The EDMX file '{edmxFile.Path}' has no content. Please check the file and try again.",
                            "SourceGeneration",
                            DiagnosticSeverity.Warning,
                            isEnabledByDefault: true),
                        Location.None));
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
                                "EASYAF004",
                                "EDMX schema error.",
                                $"The EDMX file '{edmxFile.Path}' has a schema error: {error}",
                                "SourceGeneration",
                                DiagnosticSeverity.Warning,
                                isEnabledByDefault: true),
                            Location.None));
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
                        new EntitySourceGenerator(edmxLoader, settings).Generate(context);
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
                                "EASYAF001",
                                "Unsupported project type",
                                $"The project type '{settings.ProjectType}' is not supported.",
                                "EasyAF", DiagnosticSeverity.Error, isEnabledByDefault: true),
                            Location.None));
                        break;
                }
            }

        }

    }

}
