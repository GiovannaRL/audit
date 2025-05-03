using OfflineXPlanner.Facade.Domain;
using OfflineXPlanner.Security;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using OfflineXPlanner.Extensions;

namespace OfflineXPlanner.Facade
{
    public class LoginFacade
    {
        static HttpClient client = new HttpClient();

        public static LoginResponse LoginAsync(LoginRequest requestObj)
        {
            // Fields to be send on header
            var dict = new Dictionary<string, string>();
            dict.Add("username", requestObj.username);
            dict.Add("password", requestObj.password);
            dict.Add("client_id", "AudaxWare");
            dict.Add("grant_type", "password");

            // Call server
            var req = new HttpRequestMessage(HttpMethod.Post, 
                AudaxwareRestApiInfo.loginUrl) {
                Content = new FormUrlEncodedContent(dict)
            };
            var res = client.Send(req);

            // If success is returned, store token
            if (res.IsSuccessStatusCode)
            {
                LoginResponse response = res.Content.ReadAs<LoginResponse>();
                return response;
            }

            return null;
        }
    }
}
