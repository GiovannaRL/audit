using OfflineXPlanner.Business;
using OfflineXPlanner.Domain.Enums;
using OfflineXPlanner.Security;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using xPlannerCommon.Models;
using OfflineXPlanner.Extensions;

namespace OfflineXPlanner.Facade
{
    public class ManufacturerFacade
    {
        private static HttpClient client = new HttpClient();
        private static readonly string endpoint =
            $"{AudaxwareRestApiInfo.baseUrl}{Endpoints.manufacturer}/";

        public static List<manufacturer> Import()
        {
            // Verify if there is a logged token
            if (AudaxwareRestApiInfo.accessToken == null)
            {
                //TODO: throw exception
                return null;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AudaxwareRestApiInfo.tokenType, AudaxwareRestApiInfo.accessToken);
            var res = client.Get($"{endpoint}{RestAPIActions.All}/{AudaxwareRestApiInfo.loggedDomain.domain_id}");
            if (res.IsSuccessStatusCode)
            {
                return res.Content.ReadAs<List<manufacturer>>();
            }

            //TODO: throw exception
            return null;
        }
    }
}
