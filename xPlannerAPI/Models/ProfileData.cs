using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using xPlannerCommon.Models;

namespace xPlannerAPI.Models
{
    public class ProfileData
    {
        public string oldProfile { get; set; }
        public IEnumerable<inventory_options> options { get; set; }
        public bool new_detailed_budget { get; set; }
        public bool old_detailed_budget { get; set; }
    }
}