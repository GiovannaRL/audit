using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace xPlannerCommon.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder AddWebJobConfiguration(this IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(appConfig => {
                appConfig.AddEnvironmentVariables();
                appConfig.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            }).
                ConfigureWebJobs(webJobs =>
            {
                webJobs.AddAzureStorageBlobs()
                .AddAzureStorageQueues();
            });

            return builder;
        }
    }
}
