using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Http.OData
{

    /// <summary>
    /// The wrapper around an <see cref="ODataV4Error"/> returned from an OData service.
    /// </summary>
    public class ODataV4ErrorResponse
    {

        /// <summary>
        /// Gets or sets the OData error information returned from the service.
        /// Contains detailed error information including code, message, and optional debugging details.
        /// </summary>
        [JsonPropertyName("error")]
        public ODataV4Error Error { get; set; }

    }

}
