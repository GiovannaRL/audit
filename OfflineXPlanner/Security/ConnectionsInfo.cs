using System.Configuration;

namespace OfflineXPlanner.Security
{
    public static class ConnectionsInfo
    {
        public static string databaseConnection = ConfigurationManager.ConnectionStrings["audaxware_offlineConnectionString"].ConnectionString;
    }
}
