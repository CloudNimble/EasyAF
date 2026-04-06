using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Http.OData
{

    /// <summary>
    /// Represents the base class for OData v4.0 responses containing common OData metadata properties.
    /// Provides the foundation for strongly-typed OData response handling.
    /// </summary>
    public class ODataV4ResponseBase
    {

        /// <summary>
        /// Gets or sets the OData context URL that describes the payload.
        /// This metadata property provides information about the entity set, type, and other context details.
        /// </summary>
        [JsonPropertyName("@odata.context")]
        public string ODataContext { get; set; }

    }

}
