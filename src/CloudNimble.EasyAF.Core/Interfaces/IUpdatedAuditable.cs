using System;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// An interface that implements the CloudNimble common pattern for tracking who created an Entity.
    /// </summary>
    public interface IUpdatedAuditable
    {

        /// <summary>
        /// The unique identifier for the User that created this particular Entity.
        /// </summary>
        DateTimeOffset? DateUpdated { get; set; }

    }

}
