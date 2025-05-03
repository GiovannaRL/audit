using System.Net;
using System.Net.Http;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDetails)]
    public class ProjectValuesController : AudaxWareController
    {
        /**
         * id1 = domain_id
         * id2 = project_id
         * id3 = phase_id
         * id4 = department_id
         * id5 = room_id
         */
        public HttpResponseMessage Get(int id1, int id2, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (IProjectValueRepository detailRepository = new ProjectValueRepository())
            {
                object data;
                var status = HttpStatusCode.OK;

                if (id3 == null && id4 == null && id5 == null)
                    data = detailRepository.Get(id1, id2);
                else
                    data = detailRepository.Get(id1, id2, id3, id4, id5);

                if (data == null)
                    status = HttpStatusCode.NotFound;

                return Request.CreateResponse(status, data);
            }
        }
    }
}
