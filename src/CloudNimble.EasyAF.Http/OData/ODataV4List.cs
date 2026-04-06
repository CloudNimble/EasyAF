using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Http.OData
{

    /// <summary>
    /// Represents an OData v4.0 collection response containing a list of entities with optional pagination metadata.
    /// Provides strongly-typed access to collection data with count and next link information.
    /// </summary>
    /// <typeparam name="T">The type of entities in the collection.</typeparam>
    public class ODataV4List<T> : ODataV4ResponseBase
    {

        /// <summary>
        /// Gets or sets the total number of entities in the collection, regardless of pagination.
        /// This property is only populated when the $count query option is used.
        /// </summary>
        [JsonPropertyName("@odata.count")]
        public long ODataCount { get; set; }

        /// <summary>
        /// Gets or sets the URL for retrieving the next page of results when server-side paging is enabled.
        /// This property is null if there are no more pages available.
        /// </summary>
        [JsonPropertyName("@odata.nextLink")]
        public string ODataNextLink { get; set; }

        /// <summary>
        /// Gets or sets the collection of entities returned by the OData service.
        /// This property contains the actual data payload of the response.
        /// </summary>
        [JsonPropertyName("value")]
#pragma warning disable CA2227 // Collection properties should be read only
        public List<T> Items { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

    }

}
