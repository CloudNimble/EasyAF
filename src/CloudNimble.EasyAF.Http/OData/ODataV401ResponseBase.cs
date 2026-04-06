using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Http.OData
{

    /// <summary>
    /// Represents the base class for OData v4.01 responses containing common OData metadata properties.
    /// Provides the foundation for strongly-typed OData v4.01 response handling with simplified context notation.
    /// </summary>
    public class ODataV401ResponseBase
    {

        /// <summary>
        /// Gets or sets the OData context URL that describes the payload using OData v4.01 simplified notation.
        /// This metadata property provides information about the entity set, type, and other context details.
        /// </summary>
        [JsonPropertyName("@context")]
        public string ODataContext { get; set; }

    }

}
