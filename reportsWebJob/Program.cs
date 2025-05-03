using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace reportsWebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var builder = new HostBuilder();
            builder.ConfigureAppConfiguration(appConfig => {
                appConfig.AddJsonFile("appsettings.json");
            }).
                ConfigureWebJobs(webJobs =>
                {
                    webJobs.AddAzureStorageCoreServices()
                    .AddAzureStorageBlobs()
                    .AddAzureStorageQueues();
                }).ConfigureLogging((context, b) =>
                {
                    b.AddConsole();
                    b.SetMinimumLevel(LogLevel.Debug);
                });


            var host = builder.Build();
            // The following code ensures that the WebJob will be running continuously
            host.Run();
        }
    }
}

//using Microsoft.Extensions.Hosting;
//using xPlannerCommon.Extensions;

//namespace reportsWebJob
//{
//    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
//    class Program
//    {
//        // Please set the following connection strings in app.config for this WebJob to run:
//        // AzureWebJobsDashboard and AzureWebJobsStorage
//        static void Main()
//        {
//            var builder = new HostBuilder().AddWebJobConfiguration();
//            var host = builder.Build();
//            // The following code ensures that the WebJob will be running continuously
//            host.Run();
//        }
//    }
//}
