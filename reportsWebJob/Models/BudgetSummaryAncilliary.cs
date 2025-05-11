using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class BudgetSummaryAncilliary
    {
        public Decimal freight_budget { get; set; }
        public Decimal freight_charges { get; set; }
        public Decimal freight_projected { get; set; }
        public Decimal warehouse_budget { get; set; }
        public Decimal warehouse_charges { get; set; }
        public Decimal warehouse_projected { get; set; }
        public Decimal tax_budget { get; set; }
        public Decimal tax_charges { get; set; }
        public Decimal tax_projected { get; set; }
        public Decimal misc_budget { get; set; }
        public Decimal misc_charges { get; set; }
        public Decimal misc_projected { get; set; }
        public Decimal warranty_projected { get; set; }
        public Decimal total_projected { get; set; }

    }
}
