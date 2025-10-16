using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Http.OData
{
    /// <summary>
    /// Represents implementation-specific debugging information for OData errors.
    /// Contains detailed error information such as exception details, stack traces, and nested errors.
    /// </summary>
    public class ODataV4InnerError
    {

        /// <summary>
        /// Gets or sets the detailed error message providing implementation-specific information about the error.
        /// This message is typically more technical than the outer error message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the type name of the exception that caused the error.
        /// This property helps identify the specific type of error that occurred on the server.
        /// </summary>
        [JsonPropertyName("typename")]
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the stack trace information for debugging purposes.
        /// This property provides detailed execution path information when the error occurred.
        /// </summary>
        [JsonPropertyName("stacktrace")]
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets nested inner error information for chained exceptions.
        /// This property allows for hierarchical error reporting when multiple exceptions are involved.
        /// </summary>
        [JsonPropertyName("innererror")]
        public ODataV4InnerError InnerError { get; set; }



    }






}
