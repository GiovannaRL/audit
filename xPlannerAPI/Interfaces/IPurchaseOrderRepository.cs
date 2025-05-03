using System;
using System.Collections.Generic;
using xPlannerAPI.Models;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    interface IPurchaseOrderRepository : IDisposable
    {
        List<get_purchase_orders_Result> GetAll(int domainId, int projectId, int? phaseId = null, int? departmentId = null, int? roomId = null);
        IEnumerable<Get_PO_assigned_assets_Result> GetAssignedAssets(short domainId, int projectId, int poId);
        IEnumerable<get_available_assets_to_PO_Result> GetAssetsToAssign(int domainId, int projectId, int? phaseId, int? departmentId, int? roomId, bool? showUnapproved);
        bool AddAssets(int projectDomainId, int projectId, int poId, List<asset_inventory> ipo, string addedBy);
        void UpdateAssetCost(short poDomainId, int poId, int projectId, short assetDomainId, int assetId, int inventoryId, decimal? poUnitAmt);
        IEnumerable<get_expirated_pos_Result> GetExpirated(short domainId, string userId);
        bool UpdateInventoryData(int domainId, int inventoryId, string assetStatus, DateTime? deliveredDate, DateTime? receivedDate);
    }
}