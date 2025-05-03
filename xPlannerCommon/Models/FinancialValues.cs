using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xPlannerCommon.Models
{
    public class FinancialValues
    {
        public string budget { get; set; }
        public string planned { get; set; }
        public string actual { get; set; }
        public string projected { get; set; }
        public string delta { get; set; }
    }
}
