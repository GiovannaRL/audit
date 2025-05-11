using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class OptionModifiedStructure
    {
        public short domain_id { get; set; }
        public int asset_option_id { get; set; }
        public int quantity { get; set; }
        public decimal? unit_price { get; set; }
    }
}