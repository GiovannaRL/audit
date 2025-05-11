using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class ProjectComparisonList
    {

        public string drawing_room_number { get; set; }
        public string drawing_room_name { get; set; }
        public string jsn_code { get; set; }
        public string asset_description { get; set; }
        public string resp { get; set; }
        public string pfd_resp { get; set; }
        public int? budget_qty { get; set; }
        public int? pfd_budget_qty { get; set; }
        public string comment { get; set; }
    }
}
