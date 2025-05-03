using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class Inventory
    {
        public int inventory_id { get; set; }
        public int domain_id { get; set; }
        public string asset_description { get; set; }
    }
}