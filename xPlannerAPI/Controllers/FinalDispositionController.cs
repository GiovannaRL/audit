
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class FinalDispositionController : TableGenericController<final_disposition>
    {
        public FinalDispositionController() : base(new[] { "domain_id", "project_id", "id"}, new[] { "domain_id", "project_id"}) { }
    }   
  
}