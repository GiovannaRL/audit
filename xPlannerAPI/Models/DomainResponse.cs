using System;
using System.Collections.Generic;
using xPlannerCommon.Models;

namespace xPlannerAPI.Models
{
    public class DomainResponse
    {
        public DateTime? created_at { get; set; }
        public int role_id { get; set; }
        public bool accept_user_license { get; set; }
        public string user_id { get; set; }
        public IEnumerable<manufacturer> manufacturers { get; set; }
        public short domain_id { get; set; }
        public string name { get; set; }
        public bool show_audax_info { get; set; }
        public string type { get; set; }
        public bool enabled { get; set; }
        public int user_count { get; set; }
        public int user_last_month { get; set; }
        public int user_last_year { get; set; }
    }
}