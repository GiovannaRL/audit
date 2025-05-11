using OfflineXPlanner.Utils;
using System;
using System.Data.OleDb;

namespace OfflineXPlanner.Database.Impl
{
    public class VersionDAO: IVersionDAO
    {

        public int GetCurrentInstalledVersion() {

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM version";

            var result = cmd.ExecuteReader();
            int version = 0;

            if (result.Read())
            {
                version = Convert.ToInt32(result["Number"]);
            }

            DatabaseUtil.CloseConnection(conn, cmd, result);
            return version;
        }

        public int GetNewVersion()
        {
            var conn = DatabaseUtil.CreateConnectionOnRootDiectoryDatabase();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM version";

            var result = cmd.ExecuteReader();
            int version = 0;

            if (result.Read())
            {
                version = Convert.ToInt32(result["Number"]);
            }

            DatabaseUtil.CloseConnection(conn, cmd, result);
            return version;
        }

        public bool UpdateVersion(int newVersion)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();
            
            cmd.CommandText = "UPDATE version SET Number = ?";
           
            cmd.Parameters.Add(new OleDbParameter("Number", newVersion));

            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }
    }
}
