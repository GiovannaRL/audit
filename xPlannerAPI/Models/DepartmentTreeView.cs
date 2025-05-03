using xPlannerCommon.Models;

namespace xPlannerAPI.Models
{
    public class DepartmentTreeView
    {
        public int domain_id { get; set; }
        public int project_id { get; set; }
        public string project_desc { get; set; }
        public int phase_id { get; set; }
        public string phase_desc { get; set; }
        public int department_id { get; set; }
        public string department_desc { get; set; }

    }
}