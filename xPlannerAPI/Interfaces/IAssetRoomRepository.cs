using System;
using System.Collections.Generic;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    interface IAssetRoomRepository : IDisposable
    {
        List<get_asset_rooms_Result> Get(int domain_id, int projectId, int phaseId, int departmentId, int roomId,
            string assetIds);
    }
}
