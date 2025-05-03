using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportsWebJob.Models
{
    class ShopDrawing
    {
        public int id { get; set; }
        public string filename { get; set; }
        public int type_id { get; set; }
        public System.DateTime date_added { get; set; }
        public short project_domain_id { get; set; }
        public int project_id { get; set; }
        public string blob_file_name { get; set; }
        public string version { get; set; }
        public string status { get; set; }
        public string file_extension { get; set; }
        public string asset_code { get; set; }
        public string asset_description { get; set; }
        public string department_description { get; set; }
        public string phase_description { get; set; }
        public string room_number { get; set; }
        public string room_name { get; set; }
        public string project_description { get; set; }
    }
}
