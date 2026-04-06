using System;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// An interface that specifes an implementing Entity changes State as part of the SimpleStateMachine.
    /// </summary>
    /// <typeparam name="T">The type implementing <see cref="IDbStateEnum"/> that represents States for this Entity.</typeparam>
    public interface IHasState<T> : IIdentifiable<Guid> where T : class, IDbStateEnum
    {

        /// <summary>
        /// The populated instance of <see cref="IDbStateEnum"/>.
        /// </summary>
        T StateType { get; set; }

        /// <summary>
        /// The unique identifier for the SimpleStateMachine <see cref="IDbStateEnum"/>.
        /// </summary>
        Guid StateTypeId { get; set; }

    }

}