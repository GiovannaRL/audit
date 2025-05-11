using System.Web.Http;
using xPlannerAPI.Models;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using System.Net.Http;
using System.Net;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDetails)]
    public class CopyRoomController : AudaxWareController
    {
        [ActionName("Item")]
        public HttpResponseMessage Post([FromBody] CopyRoom data, int id1, int id2)
        {
            using (ICopyRoomRepository repository = new CopyRoomRepository())
            {
                data.added_by = UserName;
                return repository.Add(id1, id2, data);
                
            }
        }
    }
}
