using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Http.OData
{

    /// <summary>
    /// Represents the base class for OData v4.0 single entity responses containing entity-specific metadata.
    /// Provides properties for entity type information, identification, and edit links.
    /// </summary>
    public class ODataV4SingleEntityResponseBase : ODataV4ResponseBase
    {

        /// <summary>
        /// Gets or sets the type annotation specifying the entity type.
        /// This property provides runtime type information for the entity.
        /// </summary>
        [JsonPropertyName("@odata.type")]
        public string ODataType { get; set; }

        /// <summary>
        /// Gets or sets the canonical URL that identifies the entity.
        /// This property provides a unique identifier for the entity resource.
        /// </summary>
        [JsonPropertyName("@odata.id")]
        public string ODataId { get; set; }

        /// <summary>
        /// Gets or sets the type annotation for the entity's Id property.
        /// This property specifies the data type of the entity identifier.
        /// </summary>
        [JsonPropertyName("Id@odata.type")]
        public string ODataIdType { get; set; }

        /// <summary>
        /// Gets or sets the URL that can be used to edit the entity.
        /// This property provides the endpoint for performing update operations on the entity.
        /// </summary>
        [JsonPropertyName("@odata.editLink")]
        public string ODataEditLink { get; set; }

    }

}
