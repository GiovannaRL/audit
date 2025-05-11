using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class InventoryOption
    {
        public short? domain_id { get; set; }
        public string data_type { get; set; }
        public string display_code { get; set; }
        public string description { get; set; }
        public string code { get; set; }
        public string settings { get; set; }
        public string blob_file_name { get; set; }
        
    }
}
