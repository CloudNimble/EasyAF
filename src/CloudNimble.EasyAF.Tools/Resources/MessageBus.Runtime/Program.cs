using CloudNimble.SimpleMessageBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
// TODO: Uncomment these after `dotnet easyaf database generate` and `dotnet easyaf code generate` create the DbContext.
//using {{Namespace}}.Data;
//using Microsoft.EntityFrameworkCore;

namespace {{Namespace}}.MessageBus.Runtime
{

    /// <summary>
    /// The entry point for the {{Product}} SimpleMessageBus runtime, which processes queued messages with the handlers in
    /// {{Namespace}}.MessageBus.Dispatch. It can run as a console app, a Windows Service, or an Azure WebJob.
    /// </summary>
    public static class Program
    {

        /// <summary>
        /// Builds and runs the message processing host.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            // Local development processes a folder on disk; every deployed environment processes Azure Queue Storage.
            var isDevelopment = string.Equals(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"), Environments.Development, StringComparison.OrdinalIgnoreCase);

            var builder = Host.CreateDefaultBuilder(args)
                // Configure the services before calling Use____QueueProcessor so the handler assemblies are loaded before SimpleMessageBus scans them.
                .ConfigureServices((context, services) =>
                {
                    services.Configure<FileSystemOptions>(context.Configuration.GetSection(nameof(FileSystemOptions)));
                    services.Configure<AzureStorageQueueOptions>(context.Configuration.GetSection(nameof(AzureStorageQueueOptions)));

                    // TODO: Uncomment after `dotnet easyaf database generate` and `dotnet easyaf code generate` create {{Product}}Context.
                    //services.AddDbContext<{{Product}}Context>(options => options.UseSqlServer(context.Configuration.GetConnectionString("{{Product}}")));

                    // TODO: Uncomment after `dotnet easyaf code generate` creates the business managers.
                    //services.Add{{Product}}BusinessDependencies();

                    // TODO: Register your IMessageHandler implementations from {{Namespace}}.MessageBus.Dispatch here.
                });

            if (isDevelopment)
            {
                builder.UseFileSystemQueueProcessor();
            }
            else
            {
                builder.UseAzureStorageQueueProcessor();
            }

            builder
                .UseOrderedMessageDispatcher()
                .UseSimpleMessageBusLifetime();

            using var host = builder.Build();
            host.Run();
        }

    }

}
