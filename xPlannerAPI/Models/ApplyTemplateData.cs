using xPlannerCommon.Models;

namespace xPlannerAPI.Models
{
    public class ApplyTemplateData
    {
        public int template_id { get; set; }
        public project_room mis { get; set; }
        public string cost_field { get; set; }
        public bool delete_assets { get; set; }
        public bool link_template { get; set; }
    }
}