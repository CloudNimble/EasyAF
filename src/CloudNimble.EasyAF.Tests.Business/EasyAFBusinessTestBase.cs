using CloudNimble.Breakdance.Assemblies;
using CloudNimble.EasyAF.Data;
using EasyAFModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Entity;
using System.Security.Claims;

namespace CloudNimble.BurnRate.Tests.Business
{

    /// <summary>
    /// Base class for setting up unit tests for message dispatchers.
    /// </summary>
    public class EasyAFBusinessTestBase : BreakdanceTestBase
    {

        #region Properties

        /// <summary>
        /// A reference to the current <see cref="TestContext"/>.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// A Guid used to authentication with managers as an evaluated role.
        /// </summary>
        internal static readonly Guid AdminUserId = new("731c7991-8714-4a6a-a98f-311f6e79f742"); // Robert's GUID

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs the test environment to simulate webjobs host.
        /// </summary>
        public EasyAFBusinessTestBase() : base()
        {
            // these services need to be configured to support authentication and the Microsoft.Data.SqlClient in the API
            EasyAF_ClaimsPrincipalExtensions.Initialize();

            // configure services needed by the test host
            TestHostBuilder.ConfigureServices((builder, services) =>
            {
                TestContext.WriteLine($"ConnectionString: {builder.Configuration["ConnectionStrings:EasyAFEntities"]}");
                services.AddScoped(_ => new EasyAFEntities(builder.Configuration["ConnectionStrings:EasyAFEntities"]));
            })
            .UseAzureStorageQueueMessagePublisher();

            DbConfiguration.SetConfiguration(new EasyAFSqlAzureConfiguration());
        }

        #endregion

    }

}
