using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.Configuration
{

    /// <summary>
    /// Provides extension methods for binding configuration sections to objects using JSON property names.
    /// Enables configuration binding that respects <see cref="JsonPropertyNameAttribute"/> when mapping
    /// configuration keys to object properties.
    /// </summary>
    public static class IConfigurationExtensions
    {

        /// <summary>
        /// Binds the configuration values to the specified instance using JSON property names for key mapping.
        /// This method respects <see cref="JsonPropertyNameAttribute"/> when determining configuration keys,
        /// allowing for JSON-style configuration binding with different property naming conventions.
        /// </summary>
        /// <typeparam name="T">The type of the instance to bind the configuration values to.</typeparam>
        /// <param name="configuration">The configuration instance to bind from.</param>
        /// <param name="instance">The instance to bind the configuration values to.</param>
        /// <remarks>
        /// This method supports automatic type conversion for common types including DateTime, DateTimeOffset,
        /// and all types supported by <see cref="Convert.ChangeType(object, Type)"/>. If a property has a
        /// <see cref="JsonPropertyNameAttribute"/>, the attribute's Name value is used as the configuration key;
        /// otherwise, the property name is used directly.
        /// </remarks>
        /// <example>
        /// <code>
        /// public class MyConfig
        /// {
        ///     [JsonPropertyName("api_endpoint")]
        ///     public string ApiEndpoint { get; set; }
        ///     
        ///     public int Port { get; set; }
        /// }
        /// 
        /// var config = new MyConfig();
        /// configuration.BindWithJsonNames(config);
        /// // Looks for "api_endpoint" and "Port" in configuration
        /// </code>
        /// </example>
        public static void BindWithJsonNames<T>(this IConfiguration configuration, T instance)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                try
                {
                    var jsonPropertyNameAttribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();
                    var configKey = jsonPropertyNameAttribute?.Name ?? property.Name;

                    var configValue = configuration[configKey];
                    if (configValue is not null)
                    {
                        object convertedValue = null;

                        if (property.PropertyType == typeof(DateTime))
                        {
                            if (DateTime.TryParse(configValue, out var dateTimeValue))
                            {
                                convertedValue = dateTimeValue;
                            }
                        }
                        else if (property.PropertyType == typeof(DateTimeOffset))
                        {
                            if (DateTimeOffset.TryParse(configValue, out var dateTimeOffsetValue))
                            {
                                convertedValue = dateTimeOffsetValue;
                            }
                        }
                        else
                        {
                            convertedValue = Convert.ChangeType(configValue, property.PropertyType);
                        }

                        if (convertedValue is null)
                        {
                            Console.WriteLine($"Error converting value '{configValue}' to type '{property.PropertyType.Name}' for property '{property.Name}'");
                            continue;
                        }

                        property.SetValue(instance, convertedValue);
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    Console.WriteLine($"Error binding property '{property.Name}': {ex.Message}");
                }
            }
        }

    }

}
