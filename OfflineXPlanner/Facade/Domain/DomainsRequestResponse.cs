using System;

namespace OfflineXPlanner.Facade.Domain
{
    public class DomainsRequestResponse
    {
        public short domain_id { get; set; }
        public string name { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public bool show_audax_info { get; set; }
        public int role_id { get; set; }
    }
}
