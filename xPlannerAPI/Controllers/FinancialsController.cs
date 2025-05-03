using System.Collections.Generic;
using System.Web.Http;
using xPlannerCommon.Models;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDetails)]
    public class FinancialsController : AudaxWareController
    {
        [ActionName("All")]
        public FinancialStructure GetAll(int id1, int id2, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (IFinancialsRepository repository = new FinancialsRepository())
            {
                return repository.GetAll(id1, id2, id3, id4, id5);
            }
           
        }
    }
}