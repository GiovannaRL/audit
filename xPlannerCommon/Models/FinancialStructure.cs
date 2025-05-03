using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xPlannerCommon.Models
{
    public class FinancialStructure : FinancialValues
    {
        public FinancialValues warehouse { get; set; }
        public FinancialValues tax { get; set; }
        public FinancialValues contingency { get; set; }
        public FinancialValues freight { get; set; }
        public FinancialValues warranty { get; set; }
        public FinancialValues markup { get; set; }
        public FinancialValues escalation { get; set; }
        public FinancialValues install { get; set; }

        public int budget_qty { get; set; }
        public int dnp_qty { get; set; }
        public int po_qty { get; set; }
    }
}
