using System.Data.Common;

namespace EasyAFModel
{

    public partial class EasyAFEntities
    {

        /// <summary>
        /// Creates the context over an existing connection, such as an in-memory connection from Effort.
        /// </summary>
        /// <param name="existingConnection">An EntityConnection that already contains the EDMX metadata.</param>
        /// <param name="contextOwnsConnection">Whether disposing the context also disposes the connection.</param>
        /// <remarks>
        /// RWM: Hand-written for the benchmarks until we decide whether the generator should emit this constructor.
        /// </remarks>
        public EasyAFEntities(DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection)
        {
        }

    }

}
