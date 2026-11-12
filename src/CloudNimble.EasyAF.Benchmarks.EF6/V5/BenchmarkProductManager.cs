using EasyAFModel.Managers;

namespace CloudNimble.EasyAF.Benchmarks.V5
{

    /// <summary>
    /// The generated <see cref="ProductManager"/> from Tests.Shared, with the status type database query skipped so the benchmarks
    /// measure the manager itself rather than SQL. Only status transitions need the loaded status types.
    /// </summary>
    public class BenchmarkProductManager : ProductManager
    {

        /// <summary>
        /// Creates a manager without a database or message publisher.
        /// </summary>
        public BenchmarkProductManager() : base(null, null)
        {
        }

        /// <inheritdoc />
        public override void Initialize()
        {
        }

    }

}
