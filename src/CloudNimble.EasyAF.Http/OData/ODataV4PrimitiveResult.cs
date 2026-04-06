using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Http.OData
{

    /// <summary>
    /// A container that allows you to capture metadata from an OData V4 response.
    /// </summary>
    /// <typeparam name="T">The type that will be deserialized from the OData V4 "value" property.</typeparam>
    public class ODataV4PrimitiveResult<T> : ODataV4ResponseBase
    {

        ///// <summary>
        ///// 
        ///// </summary>
        //[JsonPropertyName("Id@odata.type")]
        //public string ODataIdType { get; set; }

        /// <summary>
        /// Gets or sets the primitive value returned by the OData service.
        /// This property contains the actual data payload for primitive type responses.
        /// </summary>
        [JsonPropertyName("value")]
        public T Value { get; set; }

    }

}
