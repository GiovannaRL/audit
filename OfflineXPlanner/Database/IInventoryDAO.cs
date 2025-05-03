using OfflineXPlanner.Domain;
using System.Collections.Generic;
using System.Data;

namespace OfflineXPlanner.Database
{
    public enum PhotoType { Asset, Tag, Photo }
    public interface IInventoryDAO
    {
        bool InsertInventory(int project_id, int department_id, int room_id, Inventory inv);
        bool UpdateInventory(int project_id, int department_id, int room_id, Inventory inv);
        bool InsertIfNotExists(int project_id, int department_id, int room_id, Inventory inv);
        bool UpdateInventoryID(int project_id, int department_id, int room_id, Inventory inv);
        DataTable GetInventories(int project_id);
        DataTable GetInventory(int id);
        DataTable GetInventoryPhases(int project_id);
        DataTable GetInventoryDepartments(int project_id);
        DataTable GetInventoryRooms(int project_id);
        int GetNextInventoryId(int project_id);
        List<Inventory> GetInventoriesAsList(int project_id);

        //int DeleteInventorys(List<Inventory> Inventorys);

        //int DeleteInventorys(List<int> ids);

        //int DeleteInventory(Inventory Inventory);

        bool DeleteInventory(int inventoryId);

        List<Inventory> DuplicateItem(int itemID, int qty);

        List<Inventory> GetRoomInventories(int project_id, int department_id, int room_id);
        Inventory GetInventoryItem(int project_id, int id);
        bool SetPhoto(int inventoryId, PhotoType type, string photoName);
        bool UpdatePhotoFields(int inventoryId, PhotoType type, string photoName);
    }
}
