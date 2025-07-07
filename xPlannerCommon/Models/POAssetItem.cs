using System;

namespace xPlannerCommon.Models
{
    class POAssetItem
    {
        public string asset_code { get; set; }
        public string jsn_code { get; set; }
        public string asset_description { get; set; }
        public int po_qty { get; set; }
        public Decimal total_po_amt { get; set; }
        public Decimal po_unit_amt { get; set; }
        public string manufacturer { get; set; }
        public string model_number { get; set; }
        public string model_name { get; set; }
        public string vendor_model { get; set; }
        public string UOM { get; set; }
    }
}