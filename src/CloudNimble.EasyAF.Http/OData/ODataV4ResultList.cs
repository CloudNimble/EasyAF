using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Http.OData
{

    /// <summary>
    /// A container for deserializing an OData v4 result and its associated metadata.
    /// </summary>
    /// <typeparam name="T">The type of Items in the OData payload.</typeparam>
    public class ODataV4ResultList<T>
    {

        /// <summary>
        /// Maps to the "@odata.context" property, and specifies which item in the model metadata is being returned.
        /// </summary>
        [JsonPropertyName("@odata.context")]
        public string MetadataReferenceLink { get; set; }

        /// <summary>
        /// Maps to the "@odata.nextLink" property, and specifies the URL to call to get the next page of results.
        /// </summary>
        [JsonPropertyName("@odata.nextLink")]
        public string NextPageLink { get; set; }

        /// <summary>
        /// Maps to the "odata.count" property.
        /// </summary>
        /// <remarks>
        /// A mismatch between <see cref="ExpectedItemCount"/> and <see cref="Items"/>.Count can indicate an issue with deserialization.
        /// </remarks>
        [JsonPropertyName("@odata.count")]
        public string ExpectedItemCount { get; set; }

        /// <summary>
        /// A <see cref="List{T}"/> containing the items returned from the service.
        /// </summary>
        [JsonPropertyName("value")]
        public List<T> Items { get; set; }

    }

}
