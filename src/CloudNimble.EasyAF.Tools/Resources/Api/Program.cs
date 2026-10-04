using CloudNimble.SimpleMessageBus.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// TODO: Uncomment these after `dotnet easyaf database generate` and `dotnet easyaf code generate` create the DbContext and Api.
//using {{Namespace}}.Api;
//using {{Namespace}}.Data;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Restier.AspNetCore;

namespace {{Namespace}}.Api
{

    /// <summary>
    /// The entry point for the {{Product}} API.
    /// </summary>
    public static class Program
    {

        /// <summary>
        /// Builds and runs the web host.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Local development publishes to a folder on disk; every deployed environment publishes to Azure Queue Storage.
            builder.Services.Configure<FileSystemOptions>(builder.Configuration.GetSection(nameof(FileSystemOptions)));
            builder.Services.Configure<AzureStorageQueueOptions>(builder.Configuration.GetSection(nameof(AzureStorageQueueOptions)));
            if (builder.Environment.IsDevelopment())
            {
                builder.Host.UseFileSystemMessagePublisher();
            }
            else
            {
                builder.Host.UseAzureStorageQueueMessagePublisher();
            }

            builder.Services.AddAuthorization();

            // TODO: Uncomment after `dotnet easyaf code generate` creates {{Product}}ContextApi.
            //builder.Services.AddRestier(apiBuilder =>
            //{
            //    apiBuilder.AddRestierApi<{{Product}}ContextApi>(restierServices =>
            //    {
            //        restierServices.AddEFCoreProviderServices<{{Product}}Context>((serviceProvider, options) =>
            //            options.UseSqlServer(builder.Configuration.GetConnectionString("{{Product}}")));
            //    });
            //}, useEndpointRouting: true);

            // Exposes every Restier route as an MCP server at {prefix}/mcp (for example /v1/mcp). Must be registered before the app is built.
            //builder.Services.AddODataMcp();

            var app = builder.Build();

            //app.UseRestierBatching();
            app.UseRouting();
            app.UseAuthorization();

            //app.MapRestier(routeBuilder =>
            //{
            //    routeBuilder.MapApiRoute<{{Product}}ContextApi>("{{Product}}", "v1", true);
            //});

            // Must come after the Restier routes are mapped. Do not call the MCP SDK's MapMcp yourself.
            //app.UseODataMcp();

            app.Run();
        }

    }

}
