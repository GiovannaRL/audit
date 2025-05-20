using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class CopyRoom
    {
        public int source_project_id { get; set; }
        public int source_phase_id { get; set; }
        public int source_department_id { get; set; }
        public int source_room_id { get; set; }
        public int phase_id { get; set; }
        public string phase_description { get; set; }
        public int department_id { get; set; }
        public string department_description { get; set; }
        public string room_name { get; set; }
        public string room_number { get; set; }
        public string added_by { get; set; }
    }
}