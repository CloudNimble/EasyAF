using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace CloudNimble.EasyAF.Core.Converters
{

    /// <summary>
    /// A <see cref="JsonConverter{T}"/> that ignores the audit properties on a <see cref="DbObservableObject"/>.
    /// </summary>
    /// <remarks>
    /// As of EasyAF 5.0, this converter delegates to the same contract modifier as
    /// <see cref="EasyAF_JsonSerializerOptionsExtensions.IgnoreAuditFields(JsonSerializerOptions)"/>, so every System.Text.Json setting is honored.
    /// </remarks>
    [Obsolete("Call JsonSerializerOptions.IgnoreAuditFields() instead. This converter will be removed in a future release.")]
    public class IgnoreAuditFieldsJsonConverter<T> : JsonConverter<T> where T : DbObservableObject
    {

        #region Private Members

        private readonly JsonTypeInfo<T> _typeInfo;

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
            //RWM: The contract is built once per type here, so Read and Write add no per-call overhead.
            _typeInfo = (JsonTypeInfo<T>)IgnoreAuditFieldsJsonConverterFactory.GetInnerOptions(options).GetTypeInfo(typeof(T));
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
            return JsonSerializer.Deserialize(ref reader, _typeInfo);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, _typeInfo);
        }

        #endregion

    }

}
