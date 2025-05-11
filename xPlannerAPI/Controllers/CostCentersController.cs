using System.Web.Http;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDetails)]
    public class CostCentersController : TableGenericController<cost_center>
    {
        public CostCentersController() : base(new [] { "domain_id", "project_id", "id" }, new [] { "domain_id", "project_id", "id" }) { }

        public override cost_center Add([FromBody] cost_center item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var returnData = base.Add(item, id1, id2, id3, id4, id5);

            if (id3 == 1)
            {
                using (ICostCenterRepository repository = new CostCenterRepository())
                {
                    repository.updateAllAssets(id1, id2.GetValueOrDefault(), returnData.id);
                }
            }

            return returnData;
        }
    }
}
