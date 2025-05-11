using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerData.Models
{
    public class OptionStructure
    {
        public List<TreeViewItem2> tree { get; set; }
        public List<AssetOption> options { get; set; }
        public bool none_selected { get; set; }
        public int none_qty { get; set; }
        public List<LocationOptions> locations { get; set; }
    }
}