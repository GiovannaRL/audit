using System.Configuration;
using OfflineXPlanner.Domain;

namespace OfflineXPlanner.Security
{
    public static class AudaxwareRestApiInfo
    {
        private static string restApiBaseUrl = ConfigurationManager.ConnectionStrings["audaxware_offlineRestApiAddress"].ConnectionString;  

        public static readonly string loginUrl = $"{restApiBaseUrl}/Token";
        public static readonly string baseUrl = $"{restApiBaseUrl}/api/";

        public static readonly string tokenType = "Bearer";
        public static string accessToken;
        public static DomainInfo loggedDomain;

        public static void ClearToken()
        {
            accessToken = null;
            loggedDomain = null;
        }
    }
}
