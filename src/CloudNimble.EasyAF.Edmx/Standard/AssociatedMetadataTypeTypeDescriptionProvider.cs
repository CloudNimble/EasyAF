#if NETSTANDARD
namespace System.ComponentModel.DataAnnotations
{

    /// <summary>
    /// 
    /// </summary>
    public class AssociatedMetadataTypeTypeDescriptionProvider : TypeDescriptionProvider
    {
        private Type _associatedMetadataType;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public AssociatedMetadataTypeTypeDescriptionProvider(Type type)
            : base(TypeDescriptor.GetProvider(type))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="associatedMetadataType"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AssociatedMetadataTypeTypeDescriptionProvider(Type type, Type associatedMetadataType)
            : this(type)
        {
            if (associatedMetadataType is null)
            {
                throw new ArgumentNullException("associatedMetadataType");
            }

            _associatedMetadataType = associatedMetadataType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            ICustomTypeDescriptor baseDescriptor = base.GetTypeDescriptor(objectType, instance);
            return new AssociatedMetadataTypeTypeDescriptor(baseDescriptor, objectType, _associatedMetadataType);
        }

    }

}
#endif
