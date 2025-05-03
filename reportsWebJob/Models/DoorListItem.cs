using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class DoorListItem
    {
        public int room_id { get; set; }
        public string room_name { get; set; }
        public string room_number { get; set; }
        public string department_description { get; set; }
        public string phase_description { get; set; }
        public string project_name { get; set; }
        public string jsn_code { get; set; }
        public string asset_description { get; set; }
        public string resp { get; set; }
        public string cost_center { get; set; }
        public int? budget_qty { get; set; }
        public string ecn { get; set; }
    }
}
