using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerData.Models
{
    public class LocationStructure
    {
        public string phase { get; set; }
        public string department { get; set; }
        public string room { get; set; }
        public int? planned_qty { get; set; }
        public string resp { get; set; }
    }
}