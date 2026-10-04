namespace CloudNimble.EasyAF.Analyzers.EF6.SourceGeneration
{

    /// <summary>
    /// Provides constants used throughout the EasyAF source generation process.
    /// </summary>
    /// <remarks>
    /// Every diagnostic string lives here so the text is readable in one place and can move to resources for localization later.
    /// Message formats use <c>{0}</c>-style placeholders that are filled by the diagnostic's message arguments.
    /// </remarks>
    internal static class SourceGeneratorConstants
    {

        #region Fields

        /// <summary>
        /// The default base class name for API controllers.
        /// </summary>
        internal const string ApiBaseClassName = "EasyAFEntityFrameworkApi";

        #endregion

        #region Diagnostic Categories

        /// <summary>
        /// The category for diagnostics about project and EDMX setup.
        /// </summary>
        internal const string SourceGenerationCategory = "SourceGeneration";

        /// <summary>
        /// The category for diagnostics about the EasyAF model conventions.
        /// </summary>
        internal const string EasyAFCategory = "EasyAF";

        #endregion

        #region EASYAF001: Project Type Missing

        /// <summary>
        /// The diagnostic ID reported when the EasyAFProjectType property is missing.
        /// </summary>
        internal const string ProjectTypeMissingDiagnosticId = "EASYAF001";

        /// <summary>
        /// The title reported when the EasyAFProjectType property is missing.
        /// </summary>
        internal const string ProjectTypeMissingTitle = "EasyAFProjectType not defined";

        /// <summary>
        /// The message reported when the EasyAFProjectType property is missing.
        /// </summary>
        internal const string ProjectTypeMissingMessage = "The EasyAFProjectType property is not defined in the project file";

        #endregion

        #region EASYAF002: EDMX Files Missing

        /// <summary>
        /// The diagnostic ID reported when the project has no EDMX files.
        /// </summary>
        internal const string EdmxFilesMissingDiagnosticId = "EASYAF002";

        /// <summary>
        /// The title reported when the project has no EDMX files.
        /// </summary>
        internal const string EdmxFilesMissingTitle = "EDMX files not found.";

        /// <summary>
        /// The message reported when the project has no EDMX files.
        /// </summary>
        internal const string EdmxFilesMissingMessage =
            "There were no EDMX files found in the project. Please add an 'AdditionalFiles' node to an ItemGroup that references one or more EDMX files and try again.";

        #endregion

        #region EASYAF003: EDMX File Empty

        /// <summary>
        /// The diagnostic ID reported when an EDMX file has no content.
        /// </summary>
        internal const string EdmxFileEmptyDiagnosticId = "EASYAF003";

        /// <summary>
        /// The title reported when an EDMX file has no content.
        /// </summary>
        internal const string EdmxFileEmptyTitle = "EDMX file has no content.";

        /// <summary>
        /// The message reported when an EDMX file has no content. Argument 0 is the EDMX file path.
        /// </summary>
        internal const string EdmxFileEmptyMessage = "The EDMX file '{0}' has no content. Please check the file and try again.";

        #endregion

        #region EASYAF004: EDMX Schema Error

        /// <summary>
        /// The diagnostic ID reported when an EDMX file has a schema error.
        /// </summary>
        internal const string EdmxSchemaErrorDiagnosticId = "EASYAF004";

        /// <summary>
        /// The title reported when an EDMX file has a schema error.
        /// </summary>
        internal const string EdmxSchemaErrorTitle = "EDMX schema error.";

        /// <summary>
        /// The message reported when an EDMX file has a schema error. Argument 0 is the EDMX file path; argument 1 is the error.
        /// </summary>
        internal const string EdmxSchemaErrorMessage = "The EDMX file '{0}' has a schema error: {1}";

        #endregion

        #region EASYAF005: Nullable CreatedById

        /// <summary>
        /// The diagnostic ID reported when an Entity's CreatedById allows nulls.
        /// </summary>
        internal const string NullableCreatedByIdDiagnosticId = "EASYAF005";

        /// <summary>
        /// The title reported when an Entity's CreatedById allows nulls.
        /// </summary>
        internal const string NullableCreatedByIdTitle = "Nullable CreatedById disables creator tracking";

        /// <summary>
        /// The message reported when an Entity's CreatedById allows nulls. Argument 0 is the Entity name.
        /// </summary>
        internal const string NullableCreatedByIdMessage =
            "{0}.CreatedById is nullable, so ICreatorTrackable was not applied and CreatedById will not be set automatically. " +
            "To track the creator, make CreatedById required on every table, and assign built-in or seed data to a system user ID that has zero permissions.";

        #endregion

        #region EASYAF006: Project Type Found

        /// <summary>
        /// The diagnostic ID reported (as info) with the EasyAFProjectType the generator is using.
        /// </summary>
        internal const string ProjectTypeFoundDiagnosticId = "EASYAF006";

        /// <summary>
        /// The title reported when the EasyAFProjectType property is found.
        /// </summary>
        internal const string ProjectTypeFoundTitle = "EasyAFProjectType found.";

        /// <summary>
        /// The message reported when the EasyAFProjectType property is found. Argument 0 is the project type.
        /// </summary>
        internal const string ProjectTypeFoundMessage = "The EasyAFProjectType property is {0}";

        #endregion

        #region EASYAF007: Project Type Unsupported

        /// <summary>
        /// The diagnostic ID reported when the EasyAFProjectType is not supported.
        /// </summary>
        internal const string ProjectTypeUnsupportedDiagnosticId = "EASYAF007";

        /// <summary>
        /// The title reported when the EasyAFProjectType is not supported.
        /// </summary>
        internal const string ProjectTypeUnsupportedTitle = "Unsupported project type";

        /// <summary>
        /// The message reported when the EasyAFProjectType is not supported. Argument 0 is the project type.
        /// </summary>
        internal const string ProjectTypeUnsupportedMessage = "The project type '{0}' is not supported.";

        #endregion

        #region EASYAF008: Legacy GenerateViews Property

        /// <summary>
        /// The diagnostic ID reported when a project still sets the legacy GenerateViews MSBuild property.
        /// </summary>
        internal const string LegacyGenerateViewsDiagnosticId = "EASYAF008";

        /// <summary>
        /// The title reported when a project still sets the legacy GenerateViews MSBuild property.
        /// </summary>
        internal const string LegacyGenerateViewsTitle = "GenerateViews is deprecated; use EasyAFGenerateViews";

        /// <summary>
        /// The message reported when a project still sets the legacy GenerateViews MSBuild property.
        /// </summary>
        internal const string LegacyGenerateViewsMessage =
            "The GenerateViews MSBuild property is deprecated and will be removed in a future release. Rename it to EasyAFGenerateViews. " +
            "Until then, GenerateViews overrides EasyAFGenerateViews.";

        #endregion

    }

}
