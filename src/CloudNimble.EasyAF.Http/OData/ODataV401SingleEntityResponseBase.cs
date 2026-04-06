using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Http.OData
{

    /// <summary>
    /// Represents the base class for OData v4.01 single entity responses containing entity-specific metadata.
    /// Uses simplified OData v4.01 notation for entity type information and identification.
    /// </summary>
    public class ODataV401SingleEntityResponseBase : ODataV401ResponseBase
    {

        /// <summary>
        /// Gets or sets the type annotation specifying the entity type using OData v4.01 simplified notation.
        /// This property provides runtime type information for the entity.
        /// </summary>
        [JsonPropertyName("@type")]
        public string ODataType { get; set; }

        /// <summary>
        /// Gets or sets the canonical URL that identifies the entity using OData v4.01 simplified notation.
        /// This property provides a unique identifier for the entity resource.
        /// </summary>
        [JsonPropertyName("@id")]
        public string ODataId { get; set; }

        /// <summary>
        /// Gets or sets the URL that can be used to edit the entity using OData v4.01 simplified notation.
        /// This property provides the endpoint for performing update operations on the entity.
        /// </summary>
        [JsonPropertyName("editLink")]
        public string ODataEditLink { get; set; }

    }

}
