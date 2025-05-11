using OfflineXPlanner.Domain;
using OfflineXPlanner.Utils;
using System;
using System.Data.OleDb;

namespace OfflineXPlanner.Database.Impl
{
    public class LoggedDomainDAO: ILoggedDomainDAO
    {
        public DomainInfo GetChosenDomain()
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM logged_domain";

            var result = cmd.ExecuteReader();

            DomainInfo domainInfo = null;
            if (result.Read())
            {
                domainInfo = new DomainInfo(
                    Convert.ToInt32(result["domain_id"]),
                    result["domain_name"].ToString(),
                    Convert.ToDateTime(result["created_at"]),
                    Convert.ToBoolean(result["show_audax_info"]),
                    Convert.ToInt32(result["role_id"])
                );
            }

            DatabaseUtil.CloseConnection(conn, cmd, result);
            return domainInfo;
        }

        public void StoreChosenDomain(DomainInfo domain)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "INSERT INTO logged_domain(domain_id, domain_name, created_at, show_audax_info, role_id) VALUES(?, ?, ?, ?, ?)";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("DomainID", domain.domain_id),
                new OleDbParameter("DomainName", domain.domain_name),
                new OleDbParameter("CreatedAt", domain.created_at),
                new OleDbParameter("ShowAudaxInfo", domain.show_audax_info),
                new OleDbParameter("RoleID", domain.role_id)
            });

            cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);
        }
    }
}
