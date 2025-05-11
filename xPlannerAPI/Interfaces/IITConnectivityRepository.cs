using System;
using System.Collections.Generic;
using xPlannerAPI.Models;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    interface IITConnectivityRepository : IDisposable
    {
        List<AssetITConnectivity> GetAllConnections(int domainId, int projectId);
        List<asset_inventory> GetAssetsOut(int domainId, int projectId, int ITConnectivityId, int inventoryIdIn);
    }
}
