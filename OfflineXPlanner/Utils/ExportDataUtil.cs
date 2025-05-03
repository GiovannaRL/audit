using OfflineXPlanner.Business;
using System.Threading.Tasks;

namespace OfflineXPlanner.Utils
{
    public static class ExportDataUtil
    {
        public static bool ExportData(int project_id)
        {
            // Export data
            if (InventoryBusiness.Export(project_id))
            {
                RoomBusiness.ExportRoomPictures(project_id);
                return true;
            }
            return false;
        }
    }
}
