using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Http.OData
{

    /// <summary>
    /// Represents more details about an OData error.
    /// </summary>
    public class ODataV4ErrorDetail
    {

        /// <summary>
        /// Gets or sets the error code to be used in payloads.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the target of the particular error.
        /// </summary>
        /// <remarks>
        /// For example, the name of the property in error.
        /// </remarks>
        [JsonPropertyName("target")]
        public string Target { get; set; }

    }

}
