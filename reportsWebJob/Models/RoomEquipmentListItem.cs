using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class RoomEquipmentListItem
    {
        public int room_id { get; set; }
        public string room_name { get; set; }
        public string room_number { get; set; }
        public string department_description { get; set; }
        public string contact_name { get; set; }
        public string project_name { get; set; }
        public string resp { get; set; }
        public string cost_center { get; set; }
        public string asset_code { get; set; }
        public string asset_description { get; set; }
        public string manufacturer_description { get; set; }
        public string model_name { get; set; }
        public string model_number { get; set; }
        public int budget_qty { get; set; }
        public string facility { get; set; }

    }
}
