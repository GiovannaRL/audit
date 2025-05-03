using OfflineXPlanner.Domain.Enums;
using OfflineXPlanner.Facade.Domain;
using OfflineXPlanner.Security;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using OfflineXPlanner.Extensions;

namespace OfflineXPlanner.Facade
{
    public class DomainFacade
    {
        private static HttpClient client = new HttpClient();
        private static readonly string endpoint =
            $"{AudaxwareRestApiInfo.baseUrl}{Endpoints.domains}/";

        public static List<DomainsRequestResponse> GetDomains()
        {
            // Verify if there is a logged token
            if (AudaxwareRestApiInfo.accessToken == null)
            {
                //TODO: throw exception
                return null;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AudaxwareRestApiInfo.tokenType, AudaxwareRestApiInfo.accessToken);
            var res = client.Get($"{endpoint}{RestAPIActions.Available.ToString()}");
            if (res.IsSuccessStatusCode)
            {
                var domains = res.Content.ReadAs<List<DomainsRequestResponse>>();
                return domains;
            }

            //TODO: throw exception
            return null;
        }

        public static bool AddLoggedDomain(DomainsRequestResponse requestObj)
        {
            // Verify if there is a logged token
            if (AudaxwareRestApiInfo.accessToken == null)
            {
                //TODO: throw exception
                return false;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AudaxwareRestApiInfo.tokenType, AudaxwareRestApiInfo.accessToken);
            var res = client.PostAsJson($"{AudaxwareRestApiInfo.baseUrl}{Endpoints.account}/{RestAPIActions.AddLoggedDomain}", requestObj);

            // TODO: throw exception if it's false
            return res.IsSuccessStatusCode;
        }
    }
}
