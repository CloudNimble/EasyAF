using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Core.Converters
{
    /// <summary>
    /// 
    /// </summary>
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
    public class IgnoreAuditFieldsJsonConverterFactory : JsonConverterFactory
    {

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
    }

}
