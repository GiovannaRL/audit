using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfo)]
    public class AspNetRolesController : TableGenericController<AspNetRole>
    {
        public AspNetRolesController() : base(new [] { "Id" }, new string[] {}) { }
    }
}