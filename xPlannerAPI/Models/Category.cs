using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class Category
    {
        public int category_id { get; set; }
        public string category { get; set; }
        public int subcategory_id { get; set; }
        public string subcategory { get; set; }
    }
}