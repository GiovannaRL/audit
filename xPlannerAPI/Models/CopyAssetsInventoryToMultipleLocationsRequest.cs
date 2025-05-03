using System;
using System.Collections.Generic;
using System.Linq;
using xPlannerCommon.Models;

namespace xPlannerAPI.Models
{
    public class CopyAssetsInventoryToMultipleLocationsRequest
    {
        public List<asset_inventory> items;
        public List<Location> locations;
    }
}