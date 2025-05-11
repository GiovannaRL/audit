using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class AssetByRoomAsset
    {
        public short asset_domain_id { get; set; }
        public int asset_id { get; set; }

        public string asset_code { get; set; }
        public string asset_desc { get; set; }
        public string serial_number { get; set; }
        public string asset_comment { get; set; }
        public string eq_unit_desc { get; set; }
        public string hwd { get; set; }
        public string hwd_cm { get; set; }
        public string weight { get; set; }
        public string weight_kg { get; set; }
        public string electrical_option { get; set; }
        public string data_option { get; set; }
        public string water_option { get; set; }
        public string plumbing_option { get; set; }
        public string medgas_option { get; set; }
        public string blocking_option { get; set; }
        public string supports_option { get; set; }
    }
}
