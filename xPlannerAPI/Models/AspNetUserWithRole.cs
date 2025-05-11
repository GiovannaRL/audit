using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using xPlannerCommon.Models;

namespace xPlannerAPI.Models
{
    public partial class AspNetUserWithRole
    {
        public AspNetUser aspNetUser { get; set; }
        public string role { get; set; }
        public Boolean? lockoutEnabled { get; set; }

    }
}