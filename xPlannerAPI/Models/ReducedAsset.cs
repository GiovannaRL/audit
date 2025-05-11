using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class ReducedAsset
    {
        public int asset_id { get; set; }
        public string asset_code { get; set; }
        public short domain_id { get; set; }
    }
}