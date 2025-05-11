using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class FacilitiesController : TableGenericController<facility>
    {
        public FacilitiesController() : base(new [] { "domain_id", "id" }, new [] { "domain_id" }) { }
    }
}
