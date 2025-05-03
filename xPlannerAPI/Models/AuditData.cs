using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class AuditData
    {
        public string column { get; set; }
        public string original { get; set; }
        public string modified { get; set; }
    }
}