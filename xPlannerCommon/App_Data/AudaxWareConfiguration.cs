using System;
using System.Collections.Generic;
using System.Configuration;

namespace xPlannerCommon.Extensions
{
    public static class AudaxWareConfiguration
    {
        private static string GetValueEnvironmentOrConfig(string settingName)
        {
            var setting = Environment.GetEnvironmentVariable(settingName);
            if (setting != null)
            {
                return setting;
            }
            return ConfigurationManager.ConnectionStrings[settingName].ToString();
        }

        public static string GetStorageConnectionString()
        {
            return GetValueEnvironmentOrConfig("StorageConnectionString");
        }
        public static string GetWebJobsStorage()
        {
            return GetValueEnvironmentOrConfig("AzureWebJobsStorage");
        }
    }
}
