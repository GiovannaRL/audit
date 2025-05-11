using System;

namespace reportsWebJob.Models
{
    class JSNRollupItem
    {
        public string jsn { get; set; }
        public string jsn_description { get; set; }
        public Nullable<int> planned_qty { get; set; }
        public Nullable<decimal> unit_cost { get; set; }
        public Nullable<decimal> tax_cost { get; set; }
        public Nullable<decimal> extended_cost { get; set; }
        public Nullable<decimal> adjusted_unit_cost { get; set; }
        public string resp { get; set; }
        public string cost_center { get; set; }
        public Nullable<decimal> total_cost { get; set; }
    }
}
