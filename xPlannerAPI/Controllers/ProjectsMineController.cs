using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDetails)]
    public class ProjectsMineController : TableGenericController<user_project_mine>
    {
        public ProjectsMineController() : base(new[] { "domain_id", "project_id", "userId" }, new[] { "domain_id", "project_id" }) { }

        protected override bool UpdateReferences(user_project_mine item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            item.userId = UserId;

            return true;
        }

        public override user_project_mine Add([FromBody] user_project_mine item, int id1, int? id2 = null, int? id3 = null, int? id4 = null,
            int? id5 = null)
        {
            item = new user_project_mine();
            return base.Add(item, id1, id2, id3, id4, id5);
        }

        [ActionName("Item")]
        public HttpResponseMessage Delete(int id1, int id2)
        {
            using (IProjectMineRepository repository = new ProjectMineRepository())
            {
                var removed = repository.Remove(id1, id2, UserId);
                return Request.CreateResponse(removed ? HttpStatusCode.OK : HttpStatusCode.NotFound, "Project not found in the mine projects");
            }
        }
    }
}