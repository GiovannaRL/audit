using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using xPlannerCommon.Models;

namespace xPlannerAPI.Models
{
    public class AuditLog
    {
        //public audit_log log { get; set; }

        public int audit_log_id { get; set; }
        public short domain_id { get; set; }
        public string username { get; set; }
        public string operation { get; set; }
        public string table_name { get; set; }
        public string table_pk { get; set; }
        public string original { get; set; }
        public string modified { get; set; }
        public string header { get; set; }
        public System.DateTime modified_date { get; set; }
        public Nullable<int> project_id { get; set; }
        public Nullable<int> asset_id { get; set; }
        public Nullable<short> asset_domain_id { get; set; }
        public string comment { get; set; }
        public string project_description { get; set; }
        public string asset_code { get; set; }
    }
}