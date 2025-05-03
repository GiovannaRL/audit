using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerData.Models
{
    public class TreeViewItem
    {
        public int id { get; set; }
        public string text { get; set; }
        public int? total { get; set; }
        public List<TreeViewItem> items { get; set; }

        public TreeViewItem()
        {
            this.items = new List<TreeViewItem>();
        }

        public TreeViewItem(int id, string text, List<TreeViewItem> children)
        {
            this.id = id;
            this.text = text;
            this.items = children != null ? children : new List<TreeViewItem>();
        }
    }
}