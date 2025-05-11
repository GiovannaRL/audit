using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class AssetStatusItem
    {
        public string type { get; set; }
        public string resp { get; set; }
        public short asset_domain_id { get; set; }
        public int asset_id { get; set; }
        public string asset_code { get; set; }
        public string asset_description { get; set; }
        public int budget_qty { get; set; }
        public int budget_qty_sf { get; set; }
        public int lease_qty { get; set; }
        public int lease_qty_sf { get; set; }
        public int dnp_qty { get; set; }
        public int dnp_qty_sf { get; set; }
        public int po_qty { get; set; }
        public int po_qty_sf { get; set; }
        public Decimal total_budget_amt { get; set; }
        public Decimal total_po_amt { get; set; }
        public Decimal buyout_delta { get; set; }
        public string current_location { get; set; }
        public string po_status { get; set; }
        public string po_num { get; set; }
        public string po_comment { get; set; }
        public string vendor { get; set; }
        public string manufacturer { get; set; }
        public string model_no { get; set; }
        public string model_name { get; set; }
        public string cad_id { get; set; }
        public string tag { get; set; }
        public string cost_center { get; set; }
    }
}
