using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Analyzers.EF6
{

    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProjectType
    {

        /// <summary>
        /// 
        /// </summary>
        Api = 1,

        /// <summary>
        /// 
        /// </summary>
        Business = 2,

        /// <summary>
        /// 
        /// </summary>
        Core = 3,

        /// <summary>
        /// 
        /// </summary>
        Data = 4,

        /// <summary>
        /// 
        /// </summary>
        SimpleMessageBus = 5,

        /// <summary>
        /// 
        /// </summary>
        Unknown = 0

    }

}
