using System;

namespace reportsWebJob.Models
{
    class RoomByRoomGovernmentItem
    {
        public string submittal { get; set; }
        public int department_id { get; set; }
        public int room_id { get; set; }
        public string department_description { get; set; }
        public string room_number { get; set; }
        public string room_name { get; set; }
        public DateTime date_added { get; set; }
        public string functional_area { get; set; }
        public string comment { get; set; }
        public string room_code { get; set; }
        public string staff { get; set; }
        public string blueprint { get; set; }
        public string jsn_code { get; set; }
        public string asset_description { get; set; }
        public string resp { get; set; }
        public string cost_center { get; set; }
        public string ECN { get; set; }
        public string measurement { get; set; }
        public string U1 { get; set; }
        public string U2 { get; set; }
        public string U3 { get; set; }
        public string U4 { get; set; }
        public string U5 { get; set; }
        public string U6 { get; set; }
        public string asset_comment { get; set; }
        public int planned_qty { get; set; }
        public string source_location { get; set; }
        public decimal unit_cost_adjusted { get; set; }
        public decimal total_cost { get; set; }
        public decimal extended_cost { get; set; }
        public int? network_option { get; set; }

    }
}
