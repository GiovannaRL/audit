using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerData.Models
{
    public class ProjectDataReturn
    {
        public object details { get; set; }
        public object values { get; set; }

        public ProjectDataReturn(object _details, object _values)
        {
            this.details = _details;
            this.values = _values;
        }
    }
}