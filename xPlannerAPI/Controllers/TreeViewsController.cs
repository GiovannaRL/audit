using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Models;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class TreeViewsController : AudaxWareController
    {
        // If the id (domain_id) is not null get all the projects of that domain
        [ActionName("All")]
        public HttpResponseMessage GetProjects(int id1)
        {
            var roleRepository = new AspNetUserRoleRepository();
            var role = roleRepository.GetId((short)id1, UserName);

            using (ITreeViewRepository repository = new TreeViewRepository())
            {
                var trees = repository.MountTree(id1, AudaxWareIdentity, UserId);                
                var status = HttpStatusCode.OK;

                if (trees != null)
                    return Request.CreateResponse(status, trees);

                status = HttpStatusCode.NotFound;
                return Request.CreateResponse(status);
            }
        }
        
    }
}