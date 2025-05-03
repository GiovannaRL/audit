using System;
using System.Collections.Generic;
using xPlannerAPI.Models;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    interface IAssetInventoryRepository : IDisposable
    {
        string EditMultiple(EditMultipleData data);
        bool EditSingle(project_room_inventory item);

        bool LockCost(int domain_id, int project_id, int? phase_id, int? department_id, int? room_id);

        List<copy_single_item_inventory_Result> CopyMultipleInventory(List<asset_inventory> copyFrom, int domain_id, int project_id, int phase_id, int department_id, int room_id, int action, string addedBy, bool withBudget = false);

        CalculateAssetBudget CalculateBudget(CalculateAssetBudget inventory);
        List<string> GetPictures(int domain_id, int project_id, int inventory_id);
        void UploadInventoryPictures(int domain_id, int project_id, int inventory_id, List<FileData> pictures);
        void UpdateOptions(int domain_id, int project_id, InventoriesOptions options, string addedBy);
        bool Synchronize(project_room_inventory item);
        int GetSynchronizedCount(int domainId, int projectId, int inventoryId, string jsnCode, string resp);

    }
}