using OfflineXPlanner.Facade.Domain;
using System.Collections.Generic;

namespace OfflineXPlanner.Security
{
    public static class SecurityUtil
    {
        private static List<DomainsRequestResponse> availableDomains;

        public static bool IsLogged()
        {
            return AudaxwareRestApiInfo.accessToken != null;
        }

        public static List<DomainsRequestResponse> GetDomains() {
            if (AudaxwareRestApiInfo.accessToken == null) {
                return null;
            }

            return availableDomains;
        }

        public static void SetAvailableDomains(List<DomainsRequestResponse> domains) {
            availableDomains = domains;
        }
    }
}
