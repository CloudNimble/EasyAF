namespace CloudNimble.EasyAF.Http.OData
{

    /// <summary>
    /// A set of constants that specify different string values that OData uses.
    /// </summary>
    public static class ODataConstants
    {

        /// <summary>
        /// Specifies the Accept HTTP header required for OData calls.
        /// </summary>
        public const string DefaultAcceptHeader = "application/json;odata.metadata=full";

        /// <summary>
        /// Specifies the Accept HTTP header required for OData calls.
        /// </summary>
        public const string MinimalAcceptHeader = "application/json;odata.metadata=minimal";

    }

}
