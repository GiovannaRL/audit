using OfflineXPlanner.Domain;
using OfflineXPlanner.Domain.Enums;
using OfflineXPlanner.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using xPlannerCommon.Models;

namespace OfflineXPlanner.Database.Impl
{
    class CatalogDAO : ICatalogDAO
    {
        public DataTable GetAllJSN()
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();
            
            cmd.CommandText = $"SELECT (JSNCode + ' - ' + JSNNomenclature) as jsn_full, (JSNNomenclature + ' - ' + JSNCode) as jsn_full_reverse, * FROM jsn order by JSNCode";

            var data = cmd.ExecuteReader();
            DataTable dt = new DataTable(); 
            dt.Load(data);

            JSN defaultJSN = DatabaseUtil.GetDefaultJSN();
            DataRow dr = dt.NewRow();
            dr["ID"] = -1;
            dr["JSNNomenclature"] = defaultJSN.JSNNomenclature;
            dr["JSNCode"] = defaultJSN.JSNCode;
            dr["jsn_full"] = string.Format("{0} - {1}", defaultJSN.JSNCode, defaultJSN.JSNNomenclature);
            dr["jsn_full_reverse"] = string.Format("{1} - {0}", defaultJSN.JSNCode, defaultJSN.JSNNomenclature);
            dr["U1"] = defaultJSN.U1 ?? "";
            dr["U2"] = defaultJSN.U2 ?? "";
            dr["U3"] = defaultJSN.U3 ?? "";
            dr["U4"] = defaultJSN.U4 ?? "";
            dr["U5"] = defaultJSN.U5 ?? "";
            dr["U6"] = defaultJSN.U6 ?? "";

            dt.Rows.Add(dr);

            DatabaseUtil.CloseConnection(conn, cmd, data);

            return dt;
        }

        public List<JSN> GetAllJSNAslist()
        {
            List<JSN> jsns = new List<JSN>();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM jsn";
            var data = cmd.ExecuteReader();
            
            while (data.Read())
            {
                jsns.Add(new JSN {
                    JSNCode = data["JSNCode"]?.ToString(),
                    JSNNomenclature = data["JSNNomenclature"]?.ToString(),
                    U1 = data["U1"]?.ToString(),
                    U2 = data["U2"]?.ToString(),
                    U3 = data["U3"]?.ToString(),
                    U4 = data["U4"]?.ToString(),
                    U5 = data["U5"]?.ToString(),
                    U6 = data["U6"]?.ToString()
                });
            }

            DatabaseUtil.CloseConnection(conn, cmd, data);
            return jsns;
        }

        public jsn GetJSN(string JSNCode)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM jsn where JSNCode = ?";
            cmd.Parameters.Add(new OleDbParameter("JSNCode", JSNCode));

            var data = cmd.ExecuteReader();
            var jsnData = new jsn();

            if (data.Read())
            {
                jsnData.jsn_code = data["JSNCode"].ToString();
                jsnData.nomenclature = data["JSNNomenclature"].ToString();
                jsnData.utility1 = data["U1"].ToString();
                jsnData.utility2 = data["U2"].ToString();
                jsnData.utility3 = data["U3"].ToString();
                jsnData.utility4 = data["U4"].ToString();
                jsnData.utility5 = data["U5"].ToString();
                jsnData.utility6 = data["U6"].ToString();
            } else if (JSNCode.Equals(DatabaseUtil.GetDefaultJSN().JSNCode))
            {
                return DatabaseUtil.GetDefaultJSN().toCommonModel();
            }

            DatabaseUtil.CloseConnection(conn, cmd, data);
            return jsnData;
        }

        public DataTable GetAllManufacturer()
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = $"(select M.ID, m.manufacturer_id, m.description from manufacturer as m union (SELECT 0, 0,  i.Manufacturer FROM inventories i LEFT JOIN manufacturer m2 on CStr(m2.description) = CStr(i.Manufacturer) WHERE(((m2.ID)Is Null))) order by 3)";
            var data = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(data);

            DatabaseUtil.CloseConnection(conn, cmd, data);
            return dt;
        }

        public bool InsertJSN(JSN jsn)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "INSERT INTO jsn(JSNCode, JSNNomenclature, U1, U2, U3, U4, U5, U6) VALUES(?, ?, ?, ?, ?, ?, ?, ?)";

            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("Code", jsn.JSNCode),
                new OleDbParameter("Nomenclature", jsn.JSNNomenclature),
                new OleDbParameter("U1", DatabaseUtil.ToNotNull(jsn.U1)),
                new OleDbParameter("U2", DatabaseUtil.ToNotNull(jsn.U2)),
                new OleDbParameter("U3", DatabaseUtil.ToNotNull(jsn.U3)),
                new OleDbParameter("U4", DatabaseUtil.ToNotNull(jsn.U4)),
                new OleDbParameter("U5", DatabaseUtil.ToNotNull(jsn.U5)),
                new OleDbParameter("U6", DatabaseUtil.ToNotNull(jsn.U6))
            });

            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }

        public bool InsertOrUpdateJSN(JSN jsn)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            if (JSNExists(jsn))
            {
                cmd.CommandText = "UPDATE jsn SET JSNNomenclature = ?, U1 = ?, U2 = ?, U3 = ?, U4 = ?, U5 = ?, U6 = ? WHERE JSNCode = ?";
            }
            else
            {
                cmd.CommandText = "INSERT INTO jsn(JSNNomenclature, U1, U2, U3, U4, U5, U6, JSNCode) VALUES(?, ?, ?, ?, ?, ?, ?, ?)";
            }
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("Nomenclature", jsn.JSNNomenclature),
                new OleDbParameter("U1", DatabaseUtil.ToNotNull(jsn.U1)),
                new OleDbParameter("U2", DatabaseUtil.ToNotNull(jsn.U2)),
                new OleDbParameter("U3", DatabaseUtil.ToNotNull(jsn.U3)),
                new OleDbParameter("U4", DatabaseUtil.ToNotNull(jsn.U4)),
                new OleDbParameter("U5", DatabaseUtil.ToNotNull(jsn.U5)),
                new OleDbParameter("U6", DatabaseUtil.ToNotNull(jsn.U6)),
                new OleDbParameter("Code", jsn.JSNCode),
            });

            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }

        public bool InsertManufacturer(Manufacturer manufacturer)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            ExistsEnum exists = ManufacturerExists(manufacturer);
            if (exists == ExistsEnum.Id || exists == ExistsEnum.None)
            {
                if (exists == ExistsEnum.Id)
                {
                    manufacturer.manufacturer_id = GetMaxManufacturerID() + 1;
                }

                cmd.CommandText = "INSERT INTO manufacturer(description, manufacturer_id) VALUES(?, ?)";
                cmd.Parameters.AddRange(new OleDbParameter[] {
                    new OleDbParameter("Description", manufacturer.description),
                    new OleDbParameter("Manufacturer_id", manufacturer.manufacturer_id)
                });

                int rowsAffected = cmd.ExecuteNonQuery();

                DatabaseUtil.CloseConnection(conn, cmd, null);

                return rowsAffected == 1;
            }

            return false;
        }

        public ExistsEnum ManufacturerExists(Manufacturer manufacturer)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM manufacturer WHERE description = ? and manufacturer_id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("Description", manufacturer.description),
                new OleDbParameter("Manufacturer_id", manufacturer.manufacturer_id)
            });

            var result = cmd.ExecuteReader();

            if (result.Read())
            {
                DatabaseUtil.CloseConnection(conn, cmd, result);
                return ExistsEnum.Both;
            }
            else
            {
                result.Close();
                cmd.CommandText = "SELECT * FROM manufacturer WHERE description = ?";
                result = cmd.ExecuteReader();

                if (result.Read())
                {
                    DatabaseUtil.CloseConnection(conn, cmd, result);
                    return ExistsEnum.Description;
                }
                else
                {
                    result.Close();
                    cmd.CommandText = "SELECT * FROM manufacturer WHERE manufacturer_id = ?";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new OleDbParameter("Manufacturer_id", manufacturer.manufacturer_id));

                    result = cmd.ExecuteReader();

                    if (result.Read())
                    {
                        DatabaseUtil.CloseConnection(conn, cmd, result);
                        return ExistsEnum.Id;
                    }

                    DatabaseUtil.CloseConnection(conn, cmd, result);
                    return ExistsEnum.None;
                }
            }
        }

        public int GetMaxManufacturerID()
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT MAX(manufacturer_id) AS max_mnf_id FROM manufacturer";
            var manufacturerDatabase = cmd.ExecuteReader();

            int maxDepartmentID = 0;
            if (manufacturerDatabase.Read() && !manufacturerDatabase.IsDBNull(0))
            {
                maxDepartmentID = Convert.ToInt32(manufacturerDatabase["max_mnf_id"]);
            }

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return maxDepartmentID;
        }

        #region PRIVATE METHODS
        private bool JSNExists(JSN jsn)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM jsn WHERE JSNCode = ? AND JSNNomenclature = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("Code", jsn.JSNCode),
                new OleDbParameter("JSNNomenclature", jsn.JSNNomenclature)
            });

            bool exists = cmd.ExecuteReader().Read();
            DatabaseUtil.CloseConnection(conn, cmd, null);

            return exists;
        }
        #endregion
    }
}
