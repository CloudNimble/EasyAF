using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Http.OData
{

    /// <summary>
    /// A container that allows you to capture metadata from an OData V4 response.
    /// </summary>
    /// <typeparam name="T">The type that will be deserialized from the OData V4 "value" property.</typeparam>
    public class ODataV401PrimitiveResult<T> : ODataV401ResponseBase
    {

        /// <summary>
        /// Gets or sets the primitive value returned by the OData v4.01 service.
        /// This property contains the actual data payload for primitive type responses.
        /// </summary>
        [JsonPropertyName("value")]
        public T Value { get; set; }

    }

}
