using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class ProjectQty
    {
        public int ProjectId {get; set;}
        public string ProjectDescription { get; set; }
        public int Quantity { get; set; }
    }
}