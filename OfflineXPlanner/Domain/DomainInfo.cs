using OfflineXPlanner.Facade.Domain;
using System;

namespace OfflineXPlanner.Domain
{
    public class DomainInfo
    {
        public int domain_id { get; set; }
        public string domain_name { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public bool show_audax_info { get; set; }
        public int role_id { get; set; }

        public DomainInfo(int domain_id, string domain_name, Nullable<System.DateTime> created_at, bool show_audax_info, int role_id)
        {
            this.domain_id = domain_id;
            this.domain_name = domain_name;
            this.created_at = created_at;
            this.show_audax_info = show_audax_info;
            this.role_id = role_id;
        }

        public DomainInfo(DomainsRequestResponse domain)
        {
            this.domain_id = domain.domain_id;
            this.domain_name = domain.name;
            this.created_at = domain.created_at;
            this.show_audax_info = domain.show_audax_info;
            this.role_id = domain.role_id;
        }
    }
}
