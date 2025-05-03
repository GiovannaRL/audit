using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerData.Models
{
    public class AssetOption
    {
        public int id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public bool selected { get; set; }
        public int total { get; set; }

        public AssetOption()
        {
            selected = false;
            total = 0;
        }
    }
}