using CloudNimble.EasyAF.Core;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json
{

    /// <summary>
    /// Methods to extend <see cref="JsonSerializerOptions"/> in useful ways.
    /// </summary>
    public static class EasyAF_JsonSerializerOptionsExtensions
    {

        /// <summary>
        /// Stops <see cref="ICreatedAuditable.DateCreated"/>, <see cref="ICreatorTrackable{T}.CreatedById"/>, <see cref="IUpdatedAuditable.DateUpdated"/>,
        /// and <see cref="IUpdaterTrackable{T}.UpdatedById"/> from being written when serializing any <see cref="DbObservableObject"/>.
        /// </summary>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to configure. Must not have been used for serialization yet.</param>
        /// <returns>The same <see cref="JsonSerializerOptions"/> instance, so the call can be chained onto an object initializer.</returns>
        /// <remarks>
        /// <para>
        /// The audit properties are removed from each type's serialization contract the first time that type is serialized, so there is no per-call cost.
        /// Every other System.Text.Json setting, including naming policies, <see cref="Serialization.JsonPropertyNameAttribute"/>, and
        /// <see cref="JsonSerializerOptions.DefaultIgnoreCondition"/>, applies as usual. Audit properties are still read during deserialization.
        /// </para>
        /// <para>
        /// If <see cref="JsonSerializerOptions.TypeInfoResolver"/> is already set (for example, to a source-generated
        /// <see cref="Serialization.JsonSerializerContext"/>), the modifier is added to it, so this works with Native AOT.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="options"/> has already been used and is read-only.</exception>
        /// <example>
        /// <code>
        /// var options = new JsonSerializerOptions
        /// {
        ///     DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        /// }.IgnoreAuditFields();
        /// </code>
        /// </example>
        public static JsonSerializerOptions IgnoreAuditFields(this JsonSerializerOptions options)
        {
            Ensure.ArgumentNotNull(options, nameof(options));

            //RWM: JsonSerializerOptions.Default supplies the reflection resolver when reflection is enabled, and an empty one under Native AOT.
            options.TypeInfoResolver = (options.TypeInfoResolver ?? JsonSerializerOptions.Default.TypeInfoResolver).WithAddedModifier(IgnoreAuditFieldsModifier);
            return options;
        }

        /// <summary>
        /// Turns off serialization for the audit properties on <see cref="DbObservableObject"/> contracts.
        /// </summary>
        /// <param name="typeInfo">The contract System.Text.Json is building.</param>
        internal static void IgnoreAuditFieldsModifier(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object || !typeof(DbObservableObject).IsAssignableFrom(typeInfo.Type)) return;

            foreach (var property in typeInfo.Properties)
            {
                //RWM: property.Name has the naming policy and [JsonPropertyName] applied, so match on the CLR member name instead.
                switch ((property.AttributeProvider as MemberInfo)?.Name ?? property.Name)
                {
                    case nameof(ICreatedAuditable.DateCreated):
                    case nameof(ICreatorTrackable<>.CreatedById):
                    case nameof(IUpdatedAuditable.DateUpdated):
                    case nameof(IUpdaterTrackable<>.UpdatedById):
                        property.ShouldSerialize = static (_, _) => false;
                        break;
                }
            }
        }

    }

}
