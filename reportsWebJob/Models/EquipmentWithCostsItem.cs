using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class EquipmentWithCostsItem
    {
        public string resp { get; set; }
        public string asset_code { get; set; }
        public string asset_description { get; set; }
        public string manufacturer_description { get; set; }
        public string model_name { get; set; }
        public string model_number { get; set; }
        public int project_id { get; set; }
        public int budget_qty { get; set; }
        public Decimal unit_budget { get; set; }
        public Decimal total_budget { get; set; }
    }
}

