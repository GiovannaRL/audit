using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class BudgetSummaryPhasesTotal
    {
        public Decimal? total_budget_amt { get; set; }
        public Decimal? total_po_amt { get; set; }
        public Decimal? buyout_delta { get; set; }
        public Decimal? projected_budget { get; set; }
        public Decimal? pct_committed { get; set; }
    }
}
