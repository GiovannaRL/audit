using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class CopyRoom
    {
        public int from_domain_id { get; set; }
        public int from_project_id { get; set; }
        public int from_phase_id { get; set; }
        public int from_department_id { get; set; }
        public int from_room_id { get; set; }
        public List<ProjectIds> to { get; set; }
        public List<Tuple<string, string>> to_room_number_name { get; set; }
        public bool copy_options_colors { get; set; }
        public bool move { get; set; }
        public string added_by { get; set; }
    }
}