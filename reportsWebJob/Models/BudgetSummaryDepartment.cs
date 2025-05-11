using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class BudgetSummaryDepartment
    {
        public short domain_id { get; set; }
        public int project_id { get; set; }
        public int phase_id { get; set; }
        public int department_id { get; set; }
        public string description { get; set; }
        public Decimal? total_budget_amt { get; set; }
        public Decimal? total_po_amt { get; set; }
        public Decimal? buyout_delta { get; set; }
        public Decimal? projected_budget { get; set; }
        public Decimal? pct_committed { get; set; }
    }
}
