using BenchmarkDotNet.Running;

namespace CloudNimble.EasyAF.Benchmarks
{

    /// <summary>
    /// Runs the benchmarks in this assembly. Shared by Benchmarks.Core, Benchmarks.EF6, and Benchmarks.EFCore. Pass BenchmarkDotNet
    /// arguments, for example <c>dotnet run -c Release -f net10.0 -- --filter *Claims*</c>.
    /// </summary>
    public static class Program
    {

        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">BenchmarkDotNet command-line arguments.</param>
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }

    }

}
