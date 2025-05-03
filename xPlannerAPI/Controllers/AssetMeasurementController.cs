using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfo)]
    public class AssetMeasurementController : TableGenericController<assets_measurement>
    {
        public AssetMeasurementController() : base(new [] { "eq_unit_measure_id" }, new string[] {}) { }
    }
}
