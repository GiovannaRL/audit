using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class InventoryPoQtyVTotals
    {
        public decimal? total_budget_amt { get; set; }
        public decimal? total_po_amt { get; set; }
        public decimal? buyout_delta { get; set; }
        public decimal? projected_cost { get; set; }
    }
}