using System;
using System.Collections.Generic;
using xPlannerCommon.Models;
using xPlannerAPI.Models;

namespace xPlannerAPI.Interfaces
{
    interface IRoomRepository : IDisposable
    {
        List<project_room> GetMIS(int domain_id, int project_id, int? phase_id);
        List<room_inventory_po_Result> GetWithInventoryPO(int domain_id, int project_id, int phase_id, int department_id);

        void SplitRoom(IEnumerable<SplitRoomData> data, short domain_id, int project_id, int phase_id, int department_id, int room_id, bool is_linked_template, string added_by, string template_name = "");
        List<project_room> AddMultiRoom(IEnumerable<SplitRoomData> data, short domain_id, int project_id, int phase_id, int department_id, bool is_linked_template, string added_by, int? template_id);

        void MoveAsset(IEnumerable<int> data, short domain_id, int project_id, int phase_id, int department_id, int room_id);
        project_room GetRoomByNames(int domain_id, int project_id, string phase, string department, string room_number, string room_name);
        void UploadRoomPictures(project_room room, List<FileData> pictures);
        project_room InsertRoomByNames(int domain_id, int project_id, string phase, string department, string room_number, string room_name, string user);
    }
}
