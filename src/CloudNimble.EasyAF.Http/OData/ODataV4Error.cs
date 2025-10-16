using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Http.OData
{

    /// <summary>
    /// Represents an OData error payload.
    /// </summary>
    public class ODataV4Error
    {

        /// <summary>
        /// Gets or sets a collection of additional error details providing more specific information about the error.
        /// This property may contain multiple error details for scenarios with multiple validation failures.
        /// </summary>
        [JsonPropertyName("details")]
        public List<ODataV4ErrorDetail> Details { get; set; }

        /// <summary>
        /// Gets or sets the error code to be used in payloads.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>
        /// >Gets or sets the implementation-specific debugging information to help determine the cause of the error.
        /// </summary>
        [JsonPropertyName("innererror")]
        public ODataV4InnerError InnerError { get; set; }

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
