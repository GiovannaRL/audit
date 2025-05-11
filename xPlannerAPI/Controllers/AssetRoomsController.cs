using System.Collections.Generic;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsAssets)]
    public class AssetRoomsController : AudaxWareController 
    {
        public List<get_asset_rooms_Result> Get(int id1, int id2, int id3, int id4, int id5, string id6)
        {
            using (IAssetRoomRepository repository = new AssetRoomRepository())
            {
                var data = repository.Get(id1, id2, id3, id4, id5, id6);

                return data;
            }
        }
    }
}
