using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerData.Models
{
    public class TreeViewItem2
    {
        public string id { get; set; }
        public string text { get; set; }
        public string text2 { get; set; }
        public int? total { get; set; }
        public List<TreeViewItem2> items { get; set; }

        public TreeViewItem2()
        {
            this.items = new List<TreeViewItem2>();
            this.text2 = "";
        }
    }
}