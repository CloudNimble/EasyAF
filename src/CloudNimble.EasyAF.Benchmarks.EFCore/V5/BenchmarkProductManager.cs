namespace CloudNimble.EasyAF.Benchmarks.V5
{

    /// <summary>
    /// The EasyAF 5.0 <see cref="ProductManager"/>, with the status type database query skipped so the benchmarks measure the manager
    /// itself rather than the database.
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
