using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class TemplateListItem
    {
        public int? template_id { get; set; }
        public int domain_id { get; set; }
        public int project_id { get; set; }
        public int phase_id { get; set; }
        public int department_id { get; set; }
        public int room_id { get; set; }
        public string department_type { get; set; }
        public int? project_id_template { get; set; }
        public short? project_domain_id_template { get; set; }
        public string description { get; set; }
        public string project_name { get; set;}
        public DateTime? date_added { get; set; }
        public string comment { get; set; }
        public string owner { get; set; }
    }
}