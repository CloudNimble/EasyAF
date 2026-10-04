using CloudNimble.EasyAF.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Benchmarks.V4
{

    /// <summary>
    /// A copy of the EasyAF 4.x IgnoreAuditFieldsJsonConverterFactory, copied verbatim so the benchmarks can compare 5.0 against it.
    /// </summary>
    public class IgnoreAuditFieldsJsonConverterFactory : JsonConverterFactory
    {

        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsAssignableTo(typeof(DbObservableObject));

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return (JsonConverter)Activator.CreateInstance(
                typeof(IgnoreAuditFieldsJsonConverter<>)
                    .MakeGenericType([typeToConvert]),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: [options],
                culture: null)!;
        }

    }

    /// <summary>
    /// A copy of the EasyAF 4.x IgnoreAuditFieldsJsonConverter, which used reflection on every call.
    /// </summary>
    public class IgnoreAuditFieldsJsonConverter<T> : JsonConverter<T> where T : DbObservableObject
    {

        #region Private Members

        private readonly JsonSerializerOptions _options;

        private static readonly List<string> _propertiesToIgnore =
        [
            nameof(ICreatedAuditable.DateCreated),
            nameof(ICreatorTrackable<>.CreatedById),
            nameof(IUpdatedAuditable.DateUpdated),
            nameof(IUpdaterTrackable<>.UpdatedById),
        ];

        #endregion

        #region Properties

        public override bool HandleNull => false;

        #endregion

        #region Constructors

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

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(ref reader, _options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value is not null)
            {
                writer.WriteStartObject();

                foreach (var property in value.GetType().GetProperties()
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
