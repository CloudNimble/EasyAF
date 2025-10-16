using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using JsonSerialization = System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.NewtonsoftJson.Compatibility
{

    /// <summary>
    /// Provides support for <see cref="JsonSerialization.JsonIgnoreAttribute" />, <see cref="JsonSerialization.JsonExtensionDataAttribute" />, and <see cref="JsonPropertyNameAttribute"/>
    /// in System.Text.Json scenarios.
    /// </summary>
    /// <remarks>
    /// Influenced by https://github.com/RicoSuter/NJsonSchema/blob/master/src/NJsonSchema/Generation/SystemTextJsonUtilities.cs
    /// </remarks>
    public class SystemTextJsonContractResolver : DefaultContractResolver
    {

        #region Private Static Members

        private static readonly JsonSerializerSettings _settings = new()
        {
            ContractResolver = new SystemTextJsonContractResolver(),
        };

        #endregion

        #region Internal Static Members

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static JsonSerializerSettings GetJsonSerializerSettings() => _settings;

        #endregion

        #region Public Static Members

        /// <summary>
        /// 
        /// </summary>
        public static readonly SystemTextJsonContractResolver Instance = new();

        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            //RWM: Do whatever the DefaultContractResolver does to get the property information (including DataContract attribute processing).
            var property = base.CreateProperty(member, memberSerialization);
            var attributes = member.GetCustomAttributes(true);

            //RWM: Step 1 - Check if the property should be ignored.
            var propertyIgnored = false;

            var jsonIgnoreAttribute = attributes.OfType<JsonSerialization.JsonIgnoreAttribute>().FirstOrDefault();
            if (jsonIgnoreAttribute is not null && jsonIgnoreAttribute.Condition == JsonIgnoreCondition.Always)
            {
                propertyIgnored = true;
            }

            property.Ignored = propertyIgnored || attributes.OfType<JsonSerialization.JsonExtensionDataAttribute>().FirstOrDefault() is not null;

            // RWM: If the property is ignored then we don't need to do anything else.
            if (property.Ignored) return property;

            // RWM: Step 2 - Process the Name.
            var jsonPropertyNameAttribute = attributes.OfType<JsonPropertyNameAttribute>().FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(jsonPropertyNameAttribute?.Name))
            {
                property.PropertyName = jsonPropertyNameAttribute.Name;
            }

            return property;
        }

        #endregion

    }

}
