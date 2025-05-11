using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerData.Models
{
    public class TreeData
    {
        public int id { get; set; }
        public string text { get; set; }

        public TreeData(int id, string text)
        {
            this.id = id;
            this.text = text;
        }
    }
}