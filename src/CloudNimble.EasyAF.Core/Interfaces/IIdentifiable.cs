namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// An interface that guarantees a particular Entity contains an "Id" property with a type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type for the identifier.</typeparam>
    public interface IIdentifiable<T> where T: struct
    {

        /// <summary>
        /// The unique identifier for this particular Entity.
        /// </summary>
        T Id { get; set; }

    }

}
