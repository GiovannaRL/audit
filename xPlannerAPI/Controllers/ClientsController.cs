using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class ClientsController : TableGenericController<client>
    {
        public ClientsController() : base(new [] { "domain_id", "id" }, new [] { "domain_id" }) { }
    }
}