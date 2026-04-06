using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Http.OData
{

    /// <summary>
    /// Represents an OData v4.01 collection response containing a list of entities with optional pagination metadata.
    /// Uses simplified OData v4.01 notation for context and metadata properties.
    /// </summary>
    /// <typeparam name="T">The type of entities in the collection.</typeparam>
    public class ODataV401List<T> : ODataV401ResponseBase
    {

        /// <summary>
        /// Gets or sets the total number of entities in the collection using OData v4.01 simplified count notation.
        /// This property is only populated when the $count query option is used.
        /// </summary>
        [JsonPropertyName("@count")]
        public long ODataCount { get; set; }

        /// <summary>
        /// Gets or sets the URL for retrieving the next page of results using OData v4.01 simplified notation.
        /// This property is null if there are no more pages available.
        /// </summary>
        [JsonPropertyName("@nextLink")]
        public string ODataNextLink { get; set; }

        /// <summary>
        /// Gets or sets the collection of entities returned by the OData v4.01 service.
        /// This property contains the actual data payload of the response.
        /// </summary>
        [JsonPropertyName("value")]
#pragma warning disable CA2227 // Collection properties should be read only
        public List<T> Items { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

    }

}
