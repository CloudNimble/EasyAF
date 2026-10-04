using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Core.Converters
{

    /// <summary>
    /// <para>
    /// The converter we create needs to know the exact type we're converting, otherwise you would only get base object properties every
    /// time. Therefore it has to be generic. So the Factory creates the right Converter instance type for the object and sends it on its' way.
    /// </para>
    /// <para>
    /// For more details,
    /// <see href="https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-6-0">see Microsoft's converter documentation.</see>
    /// </para>
    /// </summary>
    [Obsolete("Call JsonSerializerOptions.IgnoreAuditFields() instead. This converter will be removed in a future release.")]
    public class IgnoreAuditFieldsJsonConverterFactory : JsonConverterFactory
    {

        #region Private Members

        //RWM: One inner options instance per outer options, shared by every entity type, so they all share one metadata cache.
        private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> _innerOptions = new();

        #endregion

        #region Public Methods

        /// <summary>
        ///
        /// </summary>
        /// <param name="typeToConvert"></param>
        /// <returns></returns>
        public override bool CanConvert(Type typeToConvert) =>
#if NET5_0_OR_GREATER
            typeToConvert.IsAssignableTo(typeof(DbObservableObject));
#else
            typeof(DbObservableObject).IsAssignableFrom(typeToConvert);
#endif

        /// <summary>
        ///
        /// </summary>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
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

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets a copy of <paramref name="options"/> without this factory and with the audit field contract modifier applied.
        /// </summary>
        /// <param name="options">The options the caller serializes with.</param>
        /// <returns>The cached inner options for <paramref name="options"/>.</returns>
        internal static JsonSerializerOptions GetInnerOptions(JsonSerializerOptions options) =>
            _innerOptions.GetValue(options, static outer =>
            {
                var inner = new JsonSerializerOptions(outer);
                foreach (var factory in inner.Converters.OfType<IgnoreAuditFieldsJsonConverterFactory>().ToList())
                {
                    inner.Converters.Remove(factory);
                }
                return inner.IgnoreAuditFields();
            });

        #endregion

    }

}
