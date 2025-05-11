using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class ArtworkItem
    {
        public short project_domain_id { get; set; }
        public string blob_file_name { get; set; }
        public string label { get; set; }
    }
}
