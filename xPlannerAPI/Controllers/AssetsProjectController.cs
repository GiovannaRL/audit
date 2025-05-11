using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsAssets)]
    public class AssetsProjectController : TableGenericController<assets_project>
    {
        public AssetsProjectController() : base(new [] { "asset_domain_id", "asset_id", "domain_id", "project_id" }, new [] { "asset_domain_id", "asset_id" }) { }
        
        [ActionName("Assign")]
        public HttpResponseMessage Post([FromBody] assets_project item, int id1, int id2, int id3, int id4)
        {
            using (var repository = new TableRepository<project_room_inventory>())
            {
                if (repository.GetAll(new [] { "asset_domain_id", "asset_id", "domain_id" }, GetIds(id1, id2, id3), null).Any(pri => pri.project_id != id4))
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "You cannot add this equipment to an specific project because this equipment is already related to other projects");
                }

                var projectAsset = base.GetAll(id1, id2, id3, id4).FirstOrDefault(ap => ap.domain_id == id3);

                if (projectAsset != null) base.Delete(id1, id2, id3, projectAsset.project_id);

                return Request.CreateResponse(HttpStatusCode.OK, base.Add(item, id1, id2, id3, id4));
            }
        }
    }
}
