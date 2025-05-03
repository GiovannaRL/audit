using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class DepartmentTypeController : TableGenericController<department_type>
    {
        public DepartmentTypeController() : base(new [] { "domain_id", "department_type_id" }, new [] { "domain_id" }, true) { }
    }
}