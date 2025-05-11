using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class InventoryPurchaseOrder
    {
        public short domain_id { get; set; }
        public int project_id { get; set; }
        public short asset_domain_id { get; set; }
        public int asset_id { get; set; }
        public int po_id { get; set; }
        public string inventory_ids { get; set; }
        public int po_qty { get; set; }
        public decimal? po_unit_amt { get; set; }
        public string added_by { get; set; }
        public decimal? total_po_amt { get; set; }
        public DateTime? delivered_date { get; set; }
        public DateTime? received_date { get; set; }
        public string jsn_code { get; set; }
        public string current_location { get; set; }
    }
}