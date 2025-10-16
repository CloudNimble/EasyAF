using System;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// An interface that represents the CloudNimble database-driven enumeration pattern that lets you update the Enum as processes change
    /// without changing the meaning of Entities that are linked to the older enums.
    /// </summary>
    public interface IDbEnum : IIdentifiable<Guid>, IActiveTrackable, IHumanReadable, ISortable
    {
    }

}