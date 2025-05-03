using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Models;
using xPlannerAPI.Services;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsAssets)]
    public class ReplaceInventoryController : AudaxWareController
    {
        [ActionName("items")]
        public HttpResponseMessage Put([FromBody] ReplaceInventory data, int id1, int id2)
        {
            using (IReplaceInventoryRepository repository = new ReplaceInventoryRepository())
            {
                if (repository.Put(id1, id2, data))
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }

                return Request.CreateResponse(HttpStatusCode.BadRequest, string.Format("{0} could not be replaced: purchase orders have been issued!", (data.inventories_id.Count() == 1 ? "The inventory" : "Some, or all, of the inventories")));
            }
        }
    }
}
