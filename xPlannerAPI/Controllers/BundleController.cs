using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class BundleController : TableGenericController<bundle>
    {
        public BundleController() : base(new [] { "domain_id", "bundle_id" }, new [] { "domain_id" }, new [] { "domain", "project" }, true) { }
    }
}
