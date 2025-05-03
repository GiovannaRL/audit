using OfflineXPlanner.Database;
using OfflineXPlanner.Database.Impl;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Security;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;

namespace OfflineXPlanner.Utils
{
    public static class DatabaseUtil
    {
        public const string _defaultJSNCode = "TBD";
        public const string _defaultJSNNomenclature = "To Be Defined";

        public static int GenerateNewDepartmentID(int project_id)
        {
            IDepartmentDAO dptDAO = new DepartmentDAO();

            return dptDAO.GetMaxDepartmentID(project_id) + 1;
        }

        public static OleDbConnection CreateConnection()
        {
            return new OleDbConnection(string.Format(ConnectionsInfo.databaseConnection,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
                    "Audaxware")));
        }

        public static OleDbConnection CreateConnectionOnRootDiectoryDatabase() {
            return new OleDbConnection(string.Format(ConnectionsInfo.databaseConnection, IOUtil.GetRootDirectory()));
        }

        /**
         * If the JSN doesn't exist insert on database and return an update list
         *  with the new JSN 
         */
        public static List<JSN> CheckAndAddJSN(List<JSN> databaseJSNs, Inventory item)
        {
            ICatalogDAO catalogDAO = new CatalogDAO();
            if (databaseJSNs == null)
            {
                databaseJSNs = new List<JSN>();
            }

            if (item.JSN != null && item.JSNNomenclature != null)
            {
                JSN jsn = new JSN
                {
                    JSNCode = item.JSN,
                    JSNNomenclature = item.JSNNomenclature,
                    U1 = item.U1,
                    U2 = item.U2,
                    U3 = item.U3,
                    U4 = item.U4,
                    U5 = item.U5,
                    U6 = item.U6
                };

                if (!databaseJSNs.Any(dj => dj.Equals(jsn)))
                {
                    if (catalogDAO.InsertJSN(jsn))
                    {
                        databaseJSNs.Add(jsn);
                    }
                }
            }

            return databaseJSNs;
        }

        public static JSN GetDefaultJSN()
        {
            return new JSN() {
                ID = -1,
                JSNCode = _defaultJSNCode,
                JSNNomenclature = _defaultJSNNomenclature
            };
        }

        public static bool isDefaultJSNCode(string code) {
            return code != null && code.Equals(_defaultJSNCode);
        }

        public static void CloseConnection(OleDbConnection conn, OleDbCommand command, OleDbDataReader result)
        {
            if (result != null)
            {
                result.Close();
            }

            if (command != null)
            {
                command.Dispose();
            }

            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }

        public static string ToNotNull(string value)
        {
            return value ?? "";
        }
    }
}
