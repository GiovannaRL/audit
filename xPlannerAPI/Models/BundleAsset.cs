using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class BundleAsset
    {
        public short domain_id { get; set; }
        public int asset_id { get; set; }
        public string asset_code { get; set; }
        public string asset_description { get; set; }
        public string model_number { get; set; }
        public string model_name { get; set; }
        public string manufacturer { get; set; }
    }
}