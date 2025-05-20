using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class AssetByRoomLocation
    {
        public string resp { get; set; }
        public string phase_description { get; set; }
        public string department_description { get; set; }
        public string drawing_room_number { get; set; }
        public string drawing_room_name { get; set; }
        public int total { get; set; }
        public int lease { get; set; }
        public int dnp { get; set; }
    }
}
