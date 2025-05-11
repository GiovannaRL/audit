using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class SplitRoomData
    {
        public int project_id { get; set; }
        public int phase_id { get; set; }
        public int department_id { get; set; }
        public string room_name { get; set; }
        public string room_number { get; set; }
    }
}