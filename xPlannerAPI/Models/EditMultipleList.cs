using System.Collections.Generic;
using xPlannerCommon.Models;

namespace xPlannerAPI.Models
{
    public class EditMultipleData
    {
        public project_room_inventory edited_data { get; set; }
        public List<int> inventories { get; set; }
        public List<inventory_options> optionsModified { get; set; }
        public bool updatedOptions { get; set; }
    }
}