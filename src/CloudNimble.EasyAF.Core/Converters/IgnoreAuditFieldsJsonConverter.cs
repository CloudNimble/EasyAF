using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Core.Converters
{

    /// <summary>
    /// A <see cref="JsonConverter{T}"/> that ignores certain properties on a <see cref="DbObservableObject"/>.
    /// </summary>
    /// <remarks>
    /// This converter also honors <see cref="JsonIgnoreAttribute" /> decorations on properties.
    /// </remarks>
    public class IgnoreAuditFieldsJsonConverter<T> : JsonConverter<T> where T : DbObservableObject
    {

        #region Private Members

        private readonly JsonSerializerOptions _options;

        private static readonly List<string> _propertiesToIgnore =
        [
            nameof(ICreatedAuditable.DateCreated),
            nameof(ICreatorTrackable<Guid>.CreatedById),
            nameof(IUpdatedAuditable.DateUpdated),
            nameof(IUpdaterTrackable<Guid>.UpdatedById),
        ];

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public override bool HandleNull => false;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public IgnoreAuditFieldsJsonConverter(JsonSerializerOptions options)
        {
            _options = new JsonSerializerOptions(options);

            var thisConverter = _options.Converters.Where(c => c.GetType() == typeof(IgnoreAuditFieldsJsonConverterFactory)).FirstOrDefault();
            if (thisConverter is not null)
            {
                _options.Converters.Remove(thisConverter);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(ref reader, _options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value is not null)
            {
                writer.WriteStartObject();

                foreach (var property in value.GetType().GetProperties()
                    // RWM: Implementing "Write" this way bypasses the built-in [JsonIgnore] handling, so make sure we don't forget
                    //      to do it ourselves
                    .Where(c => c.CustomAttributes.All(c => c.AttributeType != typeof(JsonIgnoreAttribute)) &&
                    !_propertiesToIgnore.Contains(c.Name)))
                {
                    var propValue = property.GetValue(value);
                    switch (true)
                    {
                        case true when propValue is not null:
                        case true when propValue is null && options.DefaultIgnoreCondition == JsonIgnoreCondition.Never:
                            writer.WritePropertyName(property.Name);
                            JsonSerializer.Serialize(writer, propValue, _options);
                            break;
                    }
                }

                writer.WriteEndObject();
            }
        }

        #endregion

    }

}
