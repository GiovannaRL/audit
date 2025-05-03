using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class ProjectIds
    {
        public short domain_id { get; set; }
        public int project_id { get; set; }
        public int phase_id { get; set; }
        public int department_id { get; set; }
    }
}