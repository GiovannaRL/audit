using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class BudgetSummaryProjectedBudget
    {
        public Int32 phase_id { get; set; }
        public string description { get; set; }
        public decimal projected_budget { get; set; }
    }
}
