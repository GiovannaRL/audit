using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xPlannerCommon.Models;

namespace reportsWebJob.Models
{
    class IllustrationSheetItem
    {
        public int project_id { get; set; }
        public int phase_id { get; set; }
        public int department_id { get; set; }
        public int room_id { get; set; }
        public short domain_id { get; set; }
        public int asset_id { get; set; }
        public short asset_domain_id { get; set; }
        public int inventory_id { get; set; }
        public string asset_description { get; set; }
        public string placement { get; set; }
        public string height { get; set; }
        public string width { get; set; }
        public string depth { get; set; }
        public string weight_limit { get; set; }
        public string jsn_code { get; set; }
        public string weight { get; set; }
        public string manufacturer_description { get; set; }
        public string department_description { get; set; }
        public string project_description { get; set; }
        public string asset_comment { get; set; }
        public string facility { get; set; }
        public string client { get; set; }
        public string locked_comment { get; set; }
        public string locked_date { get; set; }
        public string prefix_description { get; set; }
        public string photo { get; set; }
        public string model_number { get; set; }
        public string model_name { get; set; }
        public Nullable<decimal> unit_budget_total { get; set; }
        public string blob_file_name { get; set; }
        public List<project_room> rooms { get; set; }
        public List<InventoryOption> options { get; set; }

    }
}
