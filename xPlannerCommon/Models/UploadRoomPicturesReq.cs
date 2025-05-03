using System.Collections.Generic;

namespace xPlannerCommon.Models
{
    public class UploadRoomPicturesReq
    {
        public string phaseName { get; set; }
        public string departmentName { get; set; }
        public string roomNumber { get; set; }
        public string roomName { get; set; }
        public List<FileData> pictures { get; set; }
    }
}
