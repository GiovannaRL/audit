using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class LinkedRooms
    {
        public string project { get; set; }
        public string phase { get; set; }
        public string department { get; set; }
        public string room_number { get; set; }
        public string room_name { get; set; }
    }
}