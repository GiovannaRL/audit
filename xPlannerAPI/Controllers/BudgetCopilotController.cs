using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDetails)]
    public class BudgetCopilotController : AudaxWareController
    {
        [ActionName("All")]
        public HttpResponseMessage GetAll(int id1, int? id2, int? id3, string id4 = "")
        {
            try
            {
                using (IBudgetCopilotRepository repository = new BudgetCopilotRepository())
                {
                    return Request.CreateResponse(repository.GetBudgetCopilot(id1, id2, id4, id3));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

        }
    }
}