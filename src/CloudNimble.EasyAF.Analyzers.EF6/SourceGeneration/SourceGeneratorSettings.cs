using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace CloudNimble.EasyAF.Analyzers.EF6.SourceGeneration
{

    /// <summary>
    /// Represents the settings for the EasyAF source generators.
    /// </summary>
    public record SourceGeneratorSettings
    {

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to generate EF Views.
        /// </summary>
        public bool GenerateViews { get; set; }

        /// <summary>
        /// Gets or sets the type of the project (Entity, Data, Business, Api).
        /// </summary>
        public ProjectType ProjectType { get; set; }

        /// <summary>
        /// Gets or sets the namespace for the generated code.
        /// </summary>
        public string EasyAFNamespace { get; set; }

        /// <summary>
        /// Gets or sets whether API controllers should inherit from a base class.
        /// </summary>
        public bool ApiInheritance { get; set; } = true;

        /// <summary>
        /// Gets or sets whether Admin API controllers should inherit from a base class.
        /// </summary>
        public bool AdminApiInheritance { get; set; } = true;

        /// <summary>
        /// Gets or sets the base class for API controllers.
        /// </summary>
        public string ApiBaseClass { get; set; } = SourceGeneratorConstants.ApiBaseClassName;

        /// <summary>
        /// Gets or sets the base class for Admin API controllers.
        /// </summary>
        public string AdminApiBaseClass { get; set; } = SourceGeneratorConstants.ApiBaseClassName;

        /// <summary>
        /// Gets or sets additional using statements for API controllers (semicolon-separated).
        /// </summary>
        public string ApiAdditionalUsings { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ProjectNamespace { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ApiNamespace => ProjectType switch
        {
            ProjectType.Api => ProjectNamespace.Split('.').Last().StartsWith("Api") ? ProjectNamespace : $"{ProjectNamespace}.Api",
            _ => $"{EasyAFNamespace}.Api"
        };

        /// <summary>
        /// 
        /// </summary>
        public string BusinessNamespace => ProjectType switch
        {
            ProjectType.Business => ProjectNamespace.Split('.').Last().StartsWith("Business") ? ProjectNamespace : $"{ProjectNamespace}.Business",
            _ => $"{EasyAFNamespace}.Business"
        };

        /// <summary>
        /// 
        /// </summary>
        public string CoreNamespace => ProjectType switch
        {
            ProjectType.Core => ProjectNamespace.Split('.').Last().StartsWith("Core") ? ProjectNamespace : $"{ProjectNamespace}.Core",
            _ => $"{EasyAFNamespace}.Core"
        };

        /// <summary>
        /// 
        /// </summary>
        public string DataNamespace => ProjectType switch
        {
            ProjectType.Data => ProjectNamespace.Split('.').Last().StartsWith("Data") ? ProjectNamespace : $"{ProjectNamespace}.Data",
            _ => $"{EasyAFNamespace}.Data"
        };

        /// <summary>
        /// Gets the namespace for SimpleMessageBus message types.
        /// </summary>
        public string SimpleMessageBusNamespace => ProjectType switch
        {
            ProjectType.SimpleMessageBus => ProjectNamespace.Split('.').Last().StartsWith("SimpleMessageBus") ? ProjectNamespace : $"{ProjectNamespace}.SimpleMessageBus.Core",
            _ => $"{EasyAFNamespace}.SimpleMessageBus.Core"
        };

        #endregion

        #region Static Methods

        /// <summary>
        /// Creates a new instance of SourceGeneratorSettings from the given GeneratorExecutionContext.
        /// </summary>
        /// <param name="context">The generator execution context.</param>
        /// <returns>A new SourceGeneratorSettings instance.</returns>
        public static IncrementalValueProvider<SourceGeneratorSettings> FromContext(IncrementalGeneratorInitializationContext context)
        {
            return context.AnalyzerConfigOptionsProvider
                .Select((options, _) =>
                {
                    var settings = new SourceGeneratorSettings();
                    if (options.GlobalOptions.TryGetValue("build_property.EasyAFProjectType", out var projectType))
                    {
                        settings.ProjectType = Enum.TryParse(projectType, out ProjectType projectTypeEnum) ? projectTypeEnum : ProjectType.Unknown;
                    }
                    if (options.GlobalOptions.TryGetValue("build_property.EasyAFNamespace", out var ns))
                    {
                        settings.EasyAFNamespace = ns;
                    }
                    if (options.GlobalOptions.TryGetValue("build_property.GenerateViews", out var generateViews))
                    {
                        bool.TryParse(generateViews, out var value);
                        settings.GenerateViews = value;
                    }
                    if (options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNs))
                    {
                        settings.ProjectNamespace = rootNs;
                    }
                    if (options.GlobalOptions.TryGetValue("build_property.EasyAFApiInheritance", out var apiInheritance))
                    {
                        if (bool.TryParse(apiInheritance, out var value))
                        {
                            settings.ApiInheritance = value;
                        }
                    }
                    if (options.GlobalOptions.TryGetValue("build_property.EasyAFAdminApiInheritance", out var adminApiInheritance))
                    {
                        if (bool.TryParse(adminApiInheritance, out var value))
                        {
                            settings.AdminApiInheritance = value;
                        }
                    }
                    if (options.GlobalOptions.TryGetValue("build_property.EasyAFApiBaseClass", out var apiBaseClass) && settings.ApiInheritance && !string.IsNullOrWhiteSpace(apiBaseClass))
                    {
                        settings.ApiBaseClass = apiBaseClass;
                    }
                    if (options.GlobalOptions.TryGetValue("build_property.EasyAFAdminApiBaseClass", out var adminApiBaseClass) && settings.AdminApiInheritance && !string.IsNullOrWhiteSpace(adminApiBaseClass))
                    {
                        settings.AdminApiBaseClass = adminApiBaseClass;
                    }
                    if (options.GlobalOptions.TryGetValue("build_property.EasyAFApiAdditionalUsings", out var apiAdditionalUsings))
                    {
                        settings.ApiAdditionalUsings = apiAdditionalUsings;
                    }
                    return settings;
                });
        }

        #endregion

    }

}
