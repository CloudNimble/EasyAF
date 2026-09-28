using CloudNimble.Breakdance.Assemblies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// TODO: Uncomment these after `dotnet easyaf database generate` and `dotnet easyaf code generate` create the DbContext.
//using {{Namespace}}.Data;
//using Microsoft.EntityFrameworkCore;

namespace {{Namespace}}.Tests.Business
{

    /// <summary>
    /// Common base class for {{Product}} business manager tests. Builds a Breakdance test host with the business layer registered.
    /// </summary>
    public class BusinessTestBase : BreakdanceTestBase
    {

        #region Properties

        /// <summary>
        /// A reference to the current <see cref="TestContext"/>.
        /// </summary>
        public TestContext TestContext { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Configures the services needed by the test host.
        /// </summary>
        public BusinessTestBase() : base()
        {
            TestHostBuilder.ConfigureServices((context, services) =>
            {
                // TODO: Uncomment after `dotnet easyaf code generate` creates {{Product}}Context. Add the
                // Microsoft.EntityFrameworkCore.InMemory package to this project to use the in-memory provider.
                //services.AddDbContext<{{Product}}Context>(options => options.UseInMemoryDatabase("{{Product}}Tests"));

                // TODO: EasyAF managers require an IMessagePublisher. Register one (or a test double) here.

                services.Add{{Product}}Business();
            });
        }

        #endregion

    }

}
