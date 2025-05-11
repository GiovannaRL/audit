using Newtonsoft.Json.Converters;
using System;
using xPlannerCommon.Enumerators;

namespace xPlannerAPI.Models
{
    public class BudgetAggregate
    {
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public BudgetValueTypes aggregate_type { get; set; }
        public Nullable<decimal> unit_budget { get; set; }
        public Nullable<decimal> unit_freight { get; set; }
        public Nullable<decimal> unit_install { get; set; }
        public Nullable<decimal> unit_tax { get; set; }
        public Nullable<decimal> unit_markup { get; set; }
        public Nullable<decimal> unit_escalation { get; set; }


    }
}