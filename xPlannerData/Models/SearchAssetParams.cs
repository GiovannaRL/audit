using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerData.Models
{
    public class SearchAssetParams
    {
        public string search { get; set; }
        public int? subcategory_id { get; set; }
        public int? manufacturer_id { get; set; }
        public int? manufacturer_domain_id { get; set; }
        public int? vendor_id { get; set; }
        public int? vendor_domain_id { get; set; }

        public SearchAssetParams()
        {
            search = null;
        }
    }
}