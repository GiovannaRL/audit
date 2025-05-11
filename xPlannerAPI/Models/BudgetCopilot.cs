using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using xPlannerCommon.Models;

namespace xPlannerAPI.Models
{
    public class BudgetCopilot
    {
        public List<get_budget_data_from_inventories_Result> assets { get; set; }
        public List<BudgetAggregate> budgets { get; set; }





    }
}