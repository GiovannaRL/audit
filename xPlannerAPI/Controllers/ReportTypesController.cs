using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfo)]
    public class ReportTypesController : TableGenericController<report_type>
    {
        public ReportTypesController() : base(new [] { "id" }, new string[] { }) { }
    }
}
