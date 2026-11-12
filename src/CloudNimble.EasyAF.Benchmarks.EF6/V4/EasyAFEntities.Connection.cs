using System.Data.Common;

namespace CloudNimble.EasyAF.Benchmarks.V4
{

    public partial class EasyAFEntities
    {

        /// <summary>
        /// Creates the context over an existing connection, such as an in-memory connection from Effort.
        /// </summary>
        /// <param name="existingConnection">An EntityConnection that already contains the EDMX metadata.</param>
        /// <param name="contextOwnsConnection">Whether disposing the context also disposes the connection.</param>
        /// <remarks>
        /// RWM: Hand-written, matching the constructor added to the V5 context in Tests.Shared.
        /// </remarks>
        public EasyAFEntities(DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection)
        {
        }

    }

}
