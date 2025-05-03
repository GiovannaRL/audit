using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class CalculateAssetBudget
    {
        public decimal unit_markup_calc { get; set; }
        public decimal unit_escalation_calc { get; set; }
        public decimal? unit_budget_adjusted { get; set; }
        public decimal? unit_tax_calc { get; set; }
        public decimal? unit_install { get; set; }
        public decimal? unit_freight { get; set; }
        public decimal? unit_budget_total { get; set; }

        public string type_resp { get; set; }
        public decimal? total_install_net { get; set; }
        public decimal? total_budget_adjusted { get; set; }
        public decimal? total_tax { get; set; }
        public decimal? total_install { get; set; }
        public decimal? total_freight_net { get; set; }
        public decimal? total_freight { get; set; }
        public decimal? total_budget { get; set; }

        public decimal? unit_markup { get; set; }
        public decimal? total_unit_budget { get; set; }

        public decimal? unit_escalation { get; set; }

        public decimal? unit_tax { get; set; }

        public decimal? unit_install_net { get; set; }
        public decimal? unit_install_markup { get; set; }
        public decimal? unit_freight_net { get; set; }
        public decimal? unit_freight_markup { get; set; }

        public decimal net_new { get; set; }

    }
}