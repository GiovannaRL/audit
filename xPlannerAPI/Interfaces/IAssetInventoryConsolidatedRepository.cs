using System;
using System.Collections.Generic;
using xPlannerAPI.Models;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    interface IAssetInventoryConsolidatedRepository : IDisposable
    {
        List<asset_inventory> GetAll(int domainId, int projectId, int? phaseId, int? departmentId, int? roomId, string[] groupBy, bool? FilterPoQty = false, bool? showOnlyApprovedAssets = false);
        void Add(string cost_field, project_room_inventory assetData);
        bool Delete(string inventoryIds, int domainId, int projectId, int phaseId, int departmentId, int roomId);
    }
}
