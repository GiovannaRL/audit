using System;
using System.Collections.Generic;
using System.Web.Http;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Models;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsAssets)]
    public class AssetsInventoryConsolidatedController : AudaxWareController
    {
        [ActionName("Item")]
        public void Post(int id1, int id2, string id3, [FromBody] project_room_inventory assetData)
        {
            using (IAssetInventoryConsolidatedRepository repository = new AssetInventoryConsolidatedRepository())
            {
                assetData.copy_link = Guid.NewGuid();

                assetData.domain_id = (short)id1;
                assetData.project_id = id2;
                assetData.added_by = UserName;
                repository.Add(id3, assetData);
            }
        }

        [ActionName("All")]
        public IEnumerable<asset_inventory> GetAll(int id1, int id2, int? id3, int? id4 = null, int? id5 = null, [FromUri] bool? filterPoQty = false, [FromUri] bool? showOnlyApprovedAssets = false, [FromUri] string groupBy = "")
        {
            using (IAssetInventoryConsolidatedRepository repository = new AssetInventoryConsolidatedRepository())
            {
                string[] groupByArray = string.IsNullOrWhiteSpace(groupBy)
                                        ? Array.Empty<string>()
                                        : groupBy.Split(',');
                return repository.GetAll(id1, id2, id3, id4, id5, groupByArray, filterPoQty, showOnlyApprovedAssets);
            }
        }
    }
}