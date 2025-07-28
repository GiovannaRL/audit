using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace xPlannerAPI.Models
{
    public class AuditData
    {
        public string column { get; set; }
        public string original { get; set; }
        public string modified { get; set; }
    }

    public class AuditWithChanges
    {
        public int audit_log_id { get; set; }
        public string username { get; set; }
        public string operation { get; set; }
        public string table_name { get; set; }
        public string table_pk { get; set; }
        public string comment { get; set; }
        public DateTime modified_date { get; set; }
        public string project_description { get; set; }
        public string asset_code { get; set; }
        public string changed_fields { get; set; }
        public string original_fields { get; set; }
    }
}