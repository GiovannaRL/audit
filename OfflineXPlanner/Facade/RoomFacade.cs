using Newtonsoft.Json;
using OfflineXPlanner.Domain.Enums;
using OfflineXPlanner.Security;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using xPlannerCommon.Models;
using OfflineXPlanner.Extensions;

namespace OfflineXPlanner.Facade
{
    public class RoomFacade
    {
        private static HttpClient client = new HttpClient();
        private static readonly string endpoint =
            $"{AudaxwareRestApiInfo.baseUrl}{Endpoints.rooms}/";

        public static bool UploadPictures(int project_id, UploadRoomPicturesReq request)
        {
            // Verify if there is a logged token
            if (!SecurityUtil.IsLogged())
            {
                //TODO: throw exception
                return false;
            }

            var data = JsonConvert.SerializeObject(request);
            var buffer = Encoding.UTF8.GetBytes(data);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AudaxwareRestApiInfo.tokenType, AudaxwareRestApiInfo.accessToken);
            var res = client.Post($"{endpoint}{RestAPIActions.Pictures}/{AudaxwareRestApiInfo.loggedDomain.domain_id}/{project_id}", byteContent);
            return res.IsSuccessStatusCode;
        }
    }
}
