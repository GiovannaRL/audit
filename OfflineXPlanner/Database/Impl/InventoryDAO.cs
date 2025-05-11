using System.Data;
using System.Data.OleDb;
using OfflineXPlanner.Domain;
using OfflineXPlanner.Utils;
using System.Collections.Generic;
using System;

namespace OfflineXPlanner.Database
{
    public class InventoryDAO : IInventoryDAO
    {
        public bool InsertInventory(int project_id, int department_id, int room_id, Inventory inv)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "INSERT INTO inventories(inventory_id, Code, Manufacturer, Description, ModelNumber, ModelName, JSN, JSNNomenclature, ";
            cmd.CommandText += "PlannedQty, Class, Clin, UnitBudget, Phase, Department, RoomNumber, RoomName, ";
            cmd.CommandText += "Resp, U1, U2, U3, U4, U5, U6, UnitMarkup, UnitEscalation, UnitTax, UnitInstallNet, ";
            cmd.CommandText += "UnitInstallMarkup, UnitFreightNet, UnitFreightMarkup, UnitOfMeasure, project_id, department_id, room_id, ECN, Height, Depth, Width, MountingHeight, InstallMethod, Comments, CADID, DateAdded) ";
            cmd.CommandText += "VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("InventoryID", inv.inventory_id),
                new OleDbParameter("Code", inv.Code ?? ""),
                new OleDbParameter("Manufacturer", inv.Manufacturer ?? ""),
                new OleDbParameter("Description", inv.Description ?? ""),
                new OleDbParameter("ModelNumber", inv.ModelNumber ?? ""),
                new OleDbParameter("ModelName", inv.ModelName ?? ""),
                new OleDbParameter("JSN", inv.JSN ?? ""),
                new OleDbParameter("JSNNomenclature", inv.JSNNomenclature ?? ""),
                new OleDbParameter("PlanedQty", inv.PlannedQty > 0 ? inv.PlannedQty : 1),
                new OleDbParameter("Class", inv.Class ?? ""),
                new OleDbParameter("Clin", inv.Clin ?? ""),
                new OleDbParameter("UnitBudget", ConvertUtil.ToUSFormat(inv.UnitBudget)),
                new OleDbParameter("Phase", inv.Phase ?? ""),
                new OleDbParameter("Department", inv.Department ?? ""),
                new OleDbParameter("RoomNumber", inv.RoomNumber ?? ""),
                new OleDbParameter("RoomName", inv.RoomName ?? ""),
                new OleDbParameter("Resp", inv.Resp ?? ""),
                new OleDbParameter("U1", inv.U1 ?? ""),
                new OleDbParameter("U2", inv.U2 ?? ""),
                new OleDbParameter("U3", inv.U3 ?? ""),
                new OleDbParameter("U4", inv.U4 ?? ""),
                new OleDbParameter("U5", inv.U5 ?? ""),
                new OleDbParameter("U6", inv.U6 ?? ""),
                new OleDbParameter("UnitMarkup", inv.UnitMarkup.GetValueOrDefault()),
                new OleDbParameter("UnitEscalation", inv.UnitEscalation.GetValueOrDefault()),
                new OleDbParameter("UnitTax", inv.UnitTax.GetValueOrDefault()),
                new OleDbParameter("UnitInstallNet", inv.UnitInstallNet.GetValueOrDefault()),
                new OleDbParameter("UnitInstallMarkup", inv.UnitInstallMarkup.GetValueOrDefault()),
                new OleDbParameter("UnitFreightNet", inv.UnitFreightNet.GetValueOrDefault()),
                new OleDbParameter("UnitFreightMarkup", inv.UnitFreightMarkup.GetValueOrDefault()),
                new OleDbParameter("UnitOfMeasure", inv.UnitOfMeasure ?? ""),
                new OleDbParameter("ProjectID", project_id),
                new OleDbParameter("DepartmentID", department_id),
                new OleDbParameter("RoomID", room_id),
                new OleDbParameter("ECN", inv.ECN ?? ""),
                new OleDbParameter("Height", inv.Height ?? ""),
                new OleDbParameter("Depth", inv.Depth ?? ""),
                new OleDbParameter("Width", inv.Width ?? ""),
                new OleDbParameter("MountingHeight", inv.MountingHeight ?? ""),
                new OleDbParameter("InstallMethod", inv.Placement ?? ""),
                new OleDbParameter("Comments", inv.Comment ?? ""),
                new OleDbParameter("CADID", inv.CADID ?? ""),
                new OleDbParameter("DateAdded", OleDbType.Date) { Value = inv.DateAdded }
            });

            int rowsAffected = cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT @@IDENTITY"; 
            cmd.Parameters.Clear(); 

            inv.Id = Convert.ToInt32(cmd.ExecuteScalar());

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }

        public bool UpdateInventoryID(int project_id, int department_id, int room_id, Inventory inv)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "UPDATE inventories SET inventory_id = ? WHERE Id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("InventoryID", inv.inventory_id),
                new OleDbParameter("ID", inv.Id)
            });

            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }

        public bool UpdateInventory(int project_id, int department_id, int room_id, Inventory inv)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "UPDATE inventories SET Code = ?, Manufacturer = ?, Description = ?, ModelNumber = ?, ModelName = ?, JSN = ?, JSNNomenclature = ?, ";
            cmd.CommandText += " U1 = ?, U2 = ?, U3 = ?, U4 = ?, U5 = ?, U6 = ?, ECN = ?, Height = ?, Depth = ?, Width = ?, MountingHeight = ?, InstallMethod = ?, Comments = ?, CADID = ?, RoomName = ?, RoomNumber = ?, DateAdded = ? WHERE Id = ?";
            //cmd.CommandText += "PlannedQty = , Class = , Clin = , UnitBudget = , Phase = , Department, RoomNumber, RoomName, ";
            //cmd.CommandText += "Resp, U1, U2, U3, U4, U5, U6, UnitMarkup, UnitEscalation, UnitTax, UnitInstallNet, ";
            //cmd.CommandText += "UnitInstallMarkup, UnitFreightNet, UnitFreightMarkup, UnitOfMeasure, project_id, department_id, room_id) ";

            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("Code", inv.Code ?? ""),
                new OleDbParameter("Manufacturer", inv.Manufacturer ?? ""),
                new OleDbParameter("Description", inv.Description ?? ""),
                new OleDbParameter("ModelNumber", inv.ModelNumber ?? ""),
                new OleDbParameter("ModelName", inv.ModelName ?? ""),
                new OleDbParameter("JSN", inv.JSN ?? ""),
                new OleDbParameter("JSNNomenclature", inv.JSNNomenclature ?? ""),
                new OleDbParameter("U1", inv.U1 ?? ""),
                new OleDbParameter("U2", inv.U2 ?? ""),
                new OleDbParameter("U3", inv.U3 ?? ""),
                new OleDbParameter("U4", inv.U4 ?? ""),
                new OleDbParameter("U5", inv.U5 ?? ""),
                new OleDbParameter("U6", inv.U6 ?? ""),
                new OleDbParameter("ECN", inv.ECN ?? ""),
                new OleDbParameter("Height", inv.Height ?? ""),
                new OleDbParameter("Depth", inv.Depth ?? ""),
                new OleDbParameter("Width", inv.Width ?? ""),
                new OleDbParameter("MountingHeight", inv.MountingHeight ?? ""),
                new OleDbParameter("InstallMethod", inv.Placement ?? ""),
                new OleDbParameter("Comments", inv.Comment ?? ""),
                new OleDbParameter("CADID", inv.CADID ?? ""),
                new OleDbParameter("RoomName", inv.RoomName ?? ""),
                new OleDbParameter("RoomNumber", inv.RoomNumber ?? ""),
                new OleDbParameter("DateAdded", OleDbType.Date) { Value = inv.DateAdded },
                new OleDbParameter("ID", inv.inventory_id)
            });

            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }

        public bool InsertIfNotExists(int project_id, int department_id, int room_id, Inventory inv)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM inventories WHERE inventory_id = ?";
            cmd.Parameters.Add(new OleDbParameter("InventoryID", inv.inventory_id));

            var result = cmd.ExecuteReader();
            
            if (!result.Read())
            {
                DatabaseUtil.CloseConnection(conn, cmd, result);
                return InsertInventory(project_id, department_id, room_id, inv);
            }

            DatabaseUtil.CloseConnection(conn, cmd, result);
            return false;
        }

        public List<Inventory> GetInventoriesAsList(int project_id)
        {
            List<Inventory> data = new List<Inventory>();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM inventories WHERE project_id = ?";
            cmd.Parameters.Add(new OleDbParameter("ProjectID", project_id));

            var inventories = cmd.ExecuteReader();

            while (inventories.Read())
            {
                data.Add(new Inventory(inventories));
            }

            DatabaseUtil.CloseConnection(conn, cmd, inventories);

            return data;
        }

        public List<Inventory> GetRoomInventories(int project_id, int department_id, int room_id)
        {
            List<Inventory> data = new List<Inventory>();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM inventories WHERE project_id = ? AND department_id = ? AND room_id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", project_id),
                new OleDbParameter("DepartmentID", department_id),
                new OleDbParameter("RoomID", room_id)
            });

            var inventories = cmd.ExecuteReader();

            while (inventories.Read())
            {
                data.Add(new Inventory(inventories));
            }

            DatabaseUtil.CloseConnection(conn, cmd, inventories);

            return data;
        }

        public Inventory GetInventoryItem(int project_id, int id)
        {
            Inventory inventoryItem = null;

            using (var conn = DatabaseUtil.CreateConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();

                    cmd.CommandText = "SELECT * FROM inventories WHERE project_id = ? AND id = ?";
                    cmd.Parameters.AddRange(new OleDbParameter[]
                    {
                new OleDbParameter("ProjectID", project_id),
                new OleDbParameter("ID", id)
                    });

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            inventoryItem = new Inventory(reader);
                        }
                    }
                }
            }

            return inventoryItem;
        }






        public DataTable GetInventories(int project_id)
        {
            DataTable dt = new DataTable();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT Phase, Department, RoomNumber, RoomName, JSN, Description, Manufacturer, CADID, ModelNumber, ModelName, U1, U2, U3, U4, U5, U6, Resp, Id, inventory_id, Code, JSNNomenclature, PlannedQty, Class, Clin, UnitBudget, UnitMarkup, UnitEscalation, UnitTax, UnitInstallNet, UnitInstallMarkup, UnitOfMeasure, UnitFreightMarkup, project_id, department_id, room_id, ECN, Height, Width, Depth, MountingHeight, InstallMethod, Comments, PhotoFile, TagPhotoFile, DateAdded FROM inventories WHERE project_id = ?";
            cmd.Parameters.Add(new OleDbParameter("ProjectID", project_id));

            var inventories = cmd.ExecuteReader();
            dt.Load(inventories);

            DatabaseUtil.CloseConnection(conn, cmd, inventories);

            return dt;
        }

        public DataTable GetInventory(int id)
        {
            DataTable dt = new DataTable();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM inventories WHERE Id = ?";
            cmd.Parameters.Add(new OleDbParameter("Id", id));

            var inventories = cmd.ExecuteReader();
            dt.Load(inventories);

            DatabaseUtil.CloseConnection(conn, cmd, inventories);
            return dt;
        }

        public int GetNextInventoryId(int project_id)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT (max(inventory_id)) as next_id FROM inventories";

            var inventories = cmd.ExecuteReader();
            var inventory_id = 1;
            if (inventories.Read())
            {
                inventory_id = ((int)inventories["next_id"] + 1);
            }

            DatabaseUtil.CloseConnection(conn, cmd, inventories);
            return inventory_id;
        }

        public DataTable GetInventoryPhases(int project_id)
        {
            DataTable dt = new DataTable();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "select '' as phase from inventories where project_id = ? union select Phase from inventories where project_id = ? order by Phase";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", project_id),
                new OleDbParameter("ProjectID1", project_id)
            });

            var inventories = cmd.ExecuteReader();
            dt.Load(inventories);

            DatabaseUtil.CloseConnection(conn, cmd, inventories);
            return dt;
        }

        public DataTable GetInventoryDepartments(int project_id)
        {
            DataTable dt = new DataTable();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "select 0 as department_id, '' as department from inventories where project_id = ? union select department_id, Department from inventories where project_id = ? order by Department";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", project_id),
                new OleDbParameter("ProjectID1", project_id)
            });

            var inventories = cmd.ExecuteReader();
            dt.Load(inventories);

            DatabaseUtil.CloseConnection(conn, cmd, inventories);
            return dt;
        }

        public DataTable GetInventoryRooms(int project_id)
        {
            DataTable dt = new DataTable();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "select 0 as room_id, '' as room from inventories where project_id = ? union select room_id, (RoomName + ' - ' + RoomNumber) as room from inventories where project_id = ? order by room";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", project_id),
                new OleDbParameter("ProjectID1", project_id)
            });


            var inventories = cmd.ExecuteReader();
            dt.Load(inventories);

            DatabaseUtil.CloseConnection(conn, cmd, inventories);
            return dt;
        }

        public bool DeleteInventory(int inventoryId)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = $"DELETE from inventories WHERE Id = {inventoryId}";
            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }

        public bool SetPhoto(int inventoryId, PhotoType type, string photoName)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = $"UPDATE inventories SET [{(type == PhotoType.Asset ? "PhotoFile" : "TagPhotoFile")}] = ? WHERE Id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("PhotoFile", photoName),
                new OleDbParameter("ID", inventoryId)
            });

            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }

        public bool UpdatePhotoFields(int inventoryId, PhotoType type, string photoName)
        {
            using (var conn = DatabaseUtil.CreateConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();

                    string columnToUpdate = type == PhotoType.Asset ? "PhotoFile" : "TagPhotoFile";

                    cmd.CommandText = $"UPDATE inventories SET {columnToUpdate} = NULL WHERE Id = ? AND {columnToUpdate} = ?";
                    cmd.Parameters.Add(new OleDbParameter("ID", inventoryId));
                    cmd.Parameters.Add(new OleDbParameter("PhotoName", photoName));

                    int rowsAffected = cmd.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
        }

        //public int DeleteInventorys(List<int> ids)
        //{
        //    if (ids == null)
        //    {
        //        return 0;
        //    }

        //    var conn = new OleDbConnection(connectionString);
        //    var cmd = conn.CreateCommand();
        //    conn.Open();

        //    cmd.CommandText = $"DELETE FROM Inventorys WHERE ID IN ({String.Join(", ", ids)})";
        //    int rowsAffected = cmd.ExecuteNonQuery();

        //    cmd.Dispose();
        //    conn.Close();
        //    conn.Dispose();
        //    return rowsAffected;
        //}

        //public int DeleteInventory(Inventory Inventory)
        //{
        //    if (Inventory == null)
        //    {
        //        return 0;
        //    }

        //    return this.DeleteInventory(Inventory.id);
        //}

        //public int DeleteInventory(int id)
        //{
        //    return this.DeleteInventorys(new List<int>() { id });
        //}

        public List<Inventory> DuplicateItem(int itemID, int qty)
        {
            List<Inventory> result = new List<Inventory>();

            if (qty < 1) {
                return result;
            }

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM inventories WHERE Id = ?";
            cmd.Parameters.Add(new OleDbParameter("ID", itemID));

            var inventories = cmd.ExecuteReader();
            if (inventories.Read())
            {
                Inventory newItem = new Inventory(inventories);
                newItem.Id = null;
                newItem.inventory_id = 0;
                newItem.PlannedQty = 1;
                newItem.ECN = "";
                newItem.DateAdded = DateTime.Now;


                for (int i = 0; i < qty; ++i)
                {
                    var itemResult = InsertInventory(newItem.project_id, newItem.department_id, newItem.room_id, newItem);
                    result.Add(newItem);
                    if (!itemResult)
                    {
                        cmd.Transaction.Rollback();                        
                        return null;
                    }
                }
            }

            DatabaseUtil.CloseConnection(conn, cmd, inventories);
            return result;
        }
    }
}
