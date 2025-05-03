using System;
using xPlannerCommon.Models;

namespace xPlannerAPI.Models
{
    public class AssetITConnectivity
    {
        public string asset_in_description { get; set; }
        public string room_name_in { get; set; }
        public string room_number_in { get; set; }
        public string asset_out_description { get; set; }
        public string room_name_out { get; set; }
        public string room_number_out { get; set; }


        public asset_it_connectivity conn { get; set; }
        public project_room_inventory inventoryIn { get; set; }
        public project_room_inventory inventoryOut { get; set; }
    }
}