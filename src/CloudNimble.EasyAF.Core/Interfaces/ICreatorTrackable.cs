namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// An interface that implements the CloudNimble common pattern for tracking who created an Entity.
    /// </summary>
    /// <typeparam name="T">The type for the identifier.</typeparam>
    public interface ICreatorTrackable<T> where T : struct
    {

        /// <summary>
        /// The unique identifier for the User that created this particular Entity.
        /// </summary>
        T CreatedById { get; set; }

    }

}