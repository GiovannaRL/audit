using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class ProjectTreeData
    {
        public int phase_id { get; set; }
        public string phase_name { get; set; }
        public int department_id { get; set; }
        public string department_name { get; set; }
        public int room_id { get; set; }
        public string drawing_room_number { get; set; }
        public string drawing_room_name { get; set; }
    }
}