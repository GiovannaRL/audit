using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerData.Models
{
    public class LocationOptions
    {
        public string typeName { get; set; }
        public List<LocationStructure> locationData { get; set; }

        public LocationOptions(string type, List<LocationStructure> location)
        {
            this.typeName = type;
            this.locationData = location;
        }
    }
}