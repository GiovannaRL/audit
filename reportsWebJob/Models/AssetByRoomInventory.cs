using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    internal class AssetByRoomInventory
    {
        public short asset_domain_id { get; set; }
        public string asset_code { get; set; }
        public string manufacturer_description { get; set; }
        public string asset_description { get; set; }
        public string serial_name { get; set; }
        public string serial_number { get; set; }
        public string comment { get; set; }
        public string eq_unit_desc { get; set; }
        public string height { get; set; }
        public string width { get; set; }
        public string depth { get; set; }
        public string weight { get; set; }
        public int? electrical_option { get; set; }
        public int? data_option { get; set; }
        public int? water_option { get; set; }
        public int? plumbing_option { get; set; }
        public int? medgas_option { get; set; }
        public int? blocking_option { get; set; }
        public int? supports_option { get; set; }
        public int asset_id { get; set; }
    }
}
