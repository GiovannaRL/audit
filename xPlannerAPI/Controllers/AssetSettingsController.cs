using System.Collections.Generic;
using System.Web.Http;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;
using xPlannerCommon.Services;


namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.AssetsCatalogAssets)]
    public class AssetSettingsController : AudaxWareController
    {
        [ActionName("Settings")]
        public IEnumerable<AssetSettingsStructure> GetSettings(int id1, int id2)
        {
            using (var repository = new AssetSettingsRepository())
            {
                return repository.GetSettings(id1, id2);
            }
        } 

        
        [ActionName("InventorySettings")]
        public IEnumerable<AssetSettingsStructure> GetInventorySettings(int id1, int id2)
        {
            using(var repository = new AssetSettingsRepository())
            {
                return repository.GetInventorySettings(id1, id2); 
            }
        }
        
    }
}
