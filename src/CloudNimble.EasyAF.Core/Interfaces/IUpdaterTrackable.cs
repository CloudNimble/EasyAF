using System;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// An interface that implements the CloudNimble common pattern for tracking who updated an Entity.
    /// </summary>
    /// <typeparam name="T">The type for the identifier.</typeparam>
    public interface IUpdaterTrackable<T> where T : struct
    {

        /// <summary>
        /// The unique identifier for the User that updated this particular Entity.
        /// </summary>
        T? UpdatedById { get; set; }

    }

}
