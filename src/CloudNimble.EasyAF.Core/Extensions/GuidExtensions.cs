namespace System
{

    /// <summary>
    /// Methods to extend <see cref="Guid"/> in useful ways.
    /// </summary>
    public static class EasyAF_GuidExtensions
    {

        /// <summary>
        /// A little syntactical sugar to make sure GUIDs are outputted to a format that ensures accurate string comparisons.
        /// </summary>
        /// <param name="instance">The Guid to convert.</param>
        /// <returns>An upper-case string representing the GUID instance to be compared.</returns>
        /// <remarks>
        /// See https://msdn.microsoft.com/en-us/library/bb386042.aspx for more details.
        /// </remarks>
        public static string ToComparableString(this Guid instance)
        {
            return instance.ToString().ToUpper();
        }

        /// <summary>
        /// A sweet little extension to check if a Nullable Guid has a real value or not.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>A <see cref="bool"/> indicating whether or not the Guid is null or empty.</returns>
        public static bool IsNullOrEmpty(this Guid? instance)
        {
            return instance is null || instance == Guid.Empty;
        }

    }

}
