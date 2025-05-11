using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class AssetBook
    {
        public string resp { get; set; }
        public string asset_code { get; set; }
        public string asset_description { get; set; }
        public int budget_qty { get; set; }
        public int lease_qty { get; set; }
        public int dnp_qty { get; set; }
        public int po_qty { get; set; }
        public string tag { get; set; }
    }
}