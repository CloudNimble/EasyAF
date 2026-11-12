namespace CloudNimble.EasyAF.CodeGen
{

    /// <summary>
    /// Provides constants used throughout the EasyAF code generation process.
    /// </summary>
    public static class CodeGenConstants
    {

        #region Fields

        /// <summary>
        /// The default base class name for API controllers.
        /// </summary>
        public const string ApiBaseClassName = "EasyAFEntityFrameworkApi";

        /// <summary>
        /// The default namespace suffix for SimpleMessageBus message types.
        /// </summary>
        public const string DefaultSimpleMessageBusNamespace = "SimpleMessageBus.Core";

        /// <summary>
        /// The warning <c>dotnet easyaf code generate</c> prints when an Entity's CreatedById allows nulls. Format argument 0 is the Entity name.
        /// </summary>
        /// <remarks>
        /// The source generator reports the same condition as EASYAF005, with its text in the Analyzers assembly's SourceGeneratorConstants.
        /// </remarks>
        public const string NullableCreatedByIdWarning =
            "{0}.CreatedById is nullable, so ICreatorTrackable was not applied and CreatedById will not be set automatically. " +
            "To track the creator, make CreatedById required on every table, and assign built-in or seed data to a system user ID that has zero permissions.";

        #endregion

    }

}