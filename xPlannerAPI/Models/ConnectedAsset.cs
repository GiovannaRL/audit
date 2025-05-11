using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class ConnectedAsset
    {
        public string phase { get; set; }
        public string department { get; set; }
        public string room_number { get; set; }
        public string room_name { get; set; }
        public int? box_number { get; set; }
        public string box_name { get; set; }
        public string jack_code { get; set; }
        public string jack_name { get; set; }
        public string jack_number { get; set; }
        public string asset_description { get; set; }
    }
}