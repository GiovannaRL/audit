using System.Collections.Generic;
using System.Web.Http;
using xPlannerAPI.Models;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Services;
using xPlannerCommon.Models;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using System.Net.Http;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsITConnectivity)]
    public class ITConnectivityController : TableGenericController<asset_it_connectivity>
    {
        public ITConnectivityController() : base(new [] { "domain_id", "project_id", "connectivity_id" }, new [] { "domain_id", "project_id"}, true) { }

        [ActionName("AllConnections")]
        public List<AssetITConnectivity> GetAllConnections(int id1, int id2)
        {
            using (IITConnectivityRepository repository = new ITConnectivityRepository())
            {
                return repository.GetAllConnections(id1, id2);
            }
        }

        [ActionName("AssetsOut")]
        public List<asset_inventory> GetAssetsOut(int id1, int id2, int id3, int id4)
        {
            using (IITConnectivityRepository repository = new ITConnectivityRepository())
            {
                return repository.GetAssetsOut(id1, id2, id3, id4);
            }
        } 

    }
}
