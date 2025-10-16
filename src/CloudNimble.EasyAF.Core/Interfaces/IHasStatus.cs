using System;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// An interface that specifes an implementing Entity contains a child Entity of T that implements <see cref="IDbStatusEnum"/> and
    /// represents the Entity's current status.
    /// </summary>
    /// <typeparam name="T">The type implementing <see cref="IDbStatusEnum"/>.</typeparam>
    public interface IHasStatus<T> : IIdentifiable<Guid> where T : class, IDbStatusEnum
    {

        /// <summary>
        /// The populated instance of <see cref="IDbStatusEnum"/>.
        /// </summary>
        T StatusType { get; set; }

        /// <summary>
        /// The unique identifier for the SimpleStateMachine <see cref="IDbStatusEnum"/>.
        /// </summary>
        Guid StatusTypeId { get; set; }

    }

}