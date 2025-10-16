using CloudNimble.EasyAF.EFCoreToEdmx.Extensions;
using CloudNimble.EasyAF.Tools.Commands.Root;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tools
{

    class Program
    {

        public static Task<int> Main(string[] args) =>
            Host.CreateDefaultBuilder()
                // RWM: If this is not set, it won't find appsettings.json.
                //      https://github.com/dotnet/sdk/issues/9730#issuecomment-433724425
                .UseContentRoot(Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName)
                .ConfigureServices((context, services) =>
                {
                    services.AddEFCoreToEdmxServices();
                })
                .RunCommandLineApplicationAsync<EasyAFRootCommand>(args);

    }

}
