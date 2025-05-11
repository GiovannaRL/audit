using OfflineXPlanner.Domain;
using System.Collections.Generic;
using xPlannerCommon.Models;

namespace OfflineXPlanner.Facade.Domain
{
    public class ImportProjectDataResponse
    {
        public Project project { get; set; }
        public List<project_department> departments { get; set; }
        public List<project_room> rooms { get; set; }
        public List<asset_inventory> assets { get; set; }
        public List<cost_center> costCenters { get; set; }
    }
}
