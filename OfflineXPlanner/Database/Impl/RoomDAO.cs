using OfflineXPlanner.Domain;
using OfflineXPlanner.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;

namespace OfflineXPlanner.Database.Impl
{
    public class RoomDAO : IRoomDAO
    {
        public List<Room> GetAllFromProject(int projectId)
        {
            List<Room> rooms = new List<Room>();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM room where project_id = ?";
            cmd.Parameters.Add(new OleDbParameter("ProjectID", projectId));

            var roomsDatabase = cmd.ExecuteReader();
            while (roomsDatabase.Read())
            {
                rooms.Add(new Room {
                    ProjectId = projectId,
                    DepartmentId = Convert.ToInt32(roomsDatabase["department_id"]),
                    Id = Convert.ToInt32(roomsDatabase["room_id"]),
                    Number = roomsDatabase["room_number"] == DBNull.Value ? null : roomsDatabase["room_number"].ToString(),
                    Name = roomsDatabase["room_name"].ToString()
                });
            }

            DatabaseUtil.CloseConnection(conn, cmd, roomsDatabase);
            return  rooms;
        }

        public List<Room> GetAllFromProjectWithDepartment(int projectId)
        {
            List<Room> rooms = new List<Room>();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT r.*, d.description as dpto_description, d.type as dpto_type FROM room r, department d where r.project_id = ? AND d.project_id = r.project_id AND r.department_id = d.department_id";
            cmd.Parameters.Add(new OleDbParameter("ProjectID", projectId));

            var roomsDatabase = cmd.ExecuteReader();
            while (roomsDatabase.Read())
            {
                rooms.Add(new Room
                {
                    ProjectId = projectId,
                    DepartmentId = Convert.ToInt32(roomsDatabase["department_id"]),
                    Id = Convert.ToInt32(roomsDatabase["room_id"]),
                    Number = roomsDatabase["room_number"] == DBNull.Value ? null : roomsDatabase["room_number"].ToString(),
                    Name = roomsDatabase["room_name"].ToString(),
                    Dpto = new Department {
                        department_id = Convert.ToInt32(roomsDatabase["department_id"]),
                        description = roomsDatabase["dpto_description"].ToString(),
                        type = roomsDatabase["dpto_type"].ToString(),
                        project_id = projectId
                    }
                });
            }

            DatabaseUtil.CloseConnection(conn, cmd, roomsDatabase);
            return rooms;
        }

        public DataTable Get(int projectId, int departmentId)
        {
            List<Room> rooms = new List<Room>();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT (room_name + ' -- ' + room_number) as room, * FROM room where project_id = ? AND department_id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", projectId),
                new OleDbParameter("DepartmentID", departmentId)
            });


            var roomsDatabase = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(roomsDatabase);

            DatabaseUtil.CloseConnection(conn, cmd, roomsDatabase);
            return dt;
        }

        public Room Get(int projectId, int departmentId, int roomId)
        {
            Room room = null;

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM room where project_id = ? AND department_id = ? AND room_id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", projectId),
                new OleDbParameter("DepartmentID", departmentId),
                new OleDbParameter("RoomID", roomId)
            });

            var result = cmd.ExecuteReader();
            if (result.Read())
            {
                room = new Room
                {
                    ProjectId = projectId,
                    DepartmentId = Convert.ToInt32(result["department_id"]),
                    Id = Convert.ToInt32(result["room_id"]),
                    Number = result["room_number"] == DBNull.Value ? null : result["room_number"].ToString(),
                    Name = result["room_name"].ToString(),
                    PhotoFile = result["PhotoFile"] == DBNull.Value ? null : result["PhotoFile"].ToString()
                };
            }

            DatabaseUtil.CloseConnection(conn, cmd, result);
            return room;
        }
        
        public Room Insert(Room room)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            if (room.Id <= 0)
            {
                room.Id = GetInsertId();
            }

            cmd.CommandText = "INSERT INTO room(project_id, department_id, room_id, room_number, room_name) VALUES(?, ?, ?, ?, ?)";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", room.ProjectId),
                new OleDbParameter("DepartmentID", room.DepartmentId),
                new OleDbParameter("RoomID", room.Id),
                new OleDbParameter("Number", room.Number ?? ""),
                new OleDbParameter("Name", room.Name ?? "")
            });

            if (cmd.ExecuteNonQuery() != 1)
            {
                room = null;
            }

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return room;
        }

        public bool Update(Room room)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "UPDATE room SET room_number = ?, room_name = ?, PhotoFile = ? WHERE project_id = ? AND department_id = ? AND room_id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("Number", room.Number ?? ""),
                new OleDbParameter("Name", room.Name ?? ""),
                new OleDbParameter("PhotoFile", room.PhotoFile ?? ""),
                new OleDbParameter("ProjectID", room.ProjectId),
                new OleDbParameter("DepartmentID", room.DepartmentId),
                new OleDbParameter("RoomID", room.Id)
            });

            int rowsAffected = cmd.ExecuteNonQuery();

            //update invetories if they exist
            cmd.CommandText = "UPDATE inventories SET RoomNumber = ?, RoomName = ? WHERE project_id = ? and department_id = ? and room_id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("Number", room.Number),
                new OleDbParameter("Name", room.Name),
                new OleDbParameter("ProjectID", room.ProjectId),
                new OleDbParameter("DepartmentID", room.DepartmentId),
                new OleDbParameter("RoomID", room.Id)

            });

            cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }

        private int GetInsertId()
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = $"SELECT MAX(room_id) AS max_id FROM room";
            var roomsDatabase = cmd.ExecuteReader();

            int maxId = 0;
            if (roomsDatabase.Read() && !roomsDatabase.IsDBNull(0))
            {
                maxId = Convert.ToInt32(roomsDatabase["max_id"]);
            }

            DatabaseUtil.CloseConnection(conn, cmd, roomsDatabase);

            return maxId + 1;
        }

        public bool InsertIfNotExists(Room room)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM room WHERE project_id = ? AND department_id = ? AND room_id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", room.ProjectId),
                new OleDbParameter("DepartmentID", room.DepartmentId),
                new OleDbParameter("RoomID", room.Id)
            });

            var result = cmd.ExecuteReader();

            int rowsAffected = 0;
            if (!result.Read())
            {
                result.Close();

                cmd.CommandText = "INSERT INTO room(project_id, department_id, room_id, room_number, room_name) VALUES(?, ?, ?, ?, ?)";
                cmd.Parameters.AddRange(new OleDbParameter[] {
                    new OleDbParameter("Number", room.Number),
                    new OleDbParameter("Name", room.Name)
                });
                
                rowsAffected = cmd.ExecuteNonQuery();
            }

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }

        public bool DeleteRoom(int projectId, int departmentId, int roomId)
        {

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            
            conn.Open();

            cmd.CommandText = "DELETE FROM room WHERE project_id = ? AND department_id = ? AND room_id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", projectId),
                new OleDbParameter("DepartmentID", departmentId),
                new OleDbParameter("RoomID", roomId)
            });

            cmd.ExecuteNonQuery();

            //delete all assets
            IInventoryDAO inventory = new InventoryDAO();
            cmd.CommandText = "SELECT inventory_id FROM inventories WHERE project_id = ? AND department_id = ? AND room_id = ?";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                inventory.DeleteInventory((int)reader["inventory_id"]);
            }

            return true;
        }

        public Room DuplicateItem(int project_id, int department_id, int room_id, Room newRoomInfo)
        {
            Room room = Get(project_id, department_id, room_id);
            if (room == null)            
                return null;
            

            room.Id = 0;
            room.Name = newRoomInfo.Name;
            room.Number = newRoomInfo.Number;
            room.DepartmentId = newRoomInfo.DepartmentId;
            Room insertedRoom = Insert(room);

            if (insertedRoom != null)
            {
                IInventoryDAO inventoryDAO = new InventoryDAO();
                IDepartmentDAO departmentDAO = new DepartmentDAO();
                List<Inventory> inventories = inventoryDAO.GetRoomInventories(project_id, department_id, room_id);
                var department = departmentDAO.GetDeparment(project_id, newRoomInfo.DepartmentId);

                foreach (var item in inventories)
                {
                    item.Id = 0;
                    item.inventory_id = 0;
                    item.room_id = insertedRoom.Id;
                    item.RoomName = insertedRoom.Name;
                    item.RoomNumber = insertedRoom.Number;
                    item.Department = department.description;
                    item.department_id = department_id;
                    item.ECN = "";
                    item.DateAdded = DateTime.Now;
                    inventoryDAO.InsertInventory(room.ProjectId, room.DepartmentId, room.Id, item);
                }

                return insertedRoom;
            }

            return null;
        }

        public bool MoveRoom(int project_id, int old_department_id, int room_id, Room roomInfo)
        {
            IDepartmentDAO departmentDAO = new DepartmentDAO();
            var newDepartment = departmentDAO.GetDeparment(project_id, roomInfo.DepartmentId);
            var actualRoom = Get(project_id, old_department_id, room_id);

            if (newDepartment == null || actualRoom == null)
                return false;

            string oldPath = $"department_{old_department_id}";
            string newPath = $"department_{roomInfo.DepartmentId}";

            if (!string.IsNullOrEmpty(actualRoom.PhotoFile))
                roomInfo.PhotoFile = actualRoom.PhotoFile.Replace(oldPath, newPath);

            using (var conn = DatabaseUtil.CreateConnection())
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;

                    try
                    {
                        cmd.CommandText = @"
                            UPDATE room 
                            SET
                                room_number = ?,
                                room_name = ?,
                                department_id = ?,
                                PhotoFile = ?
                            WHERE
                                project_id = ? AND
                                department_id = ? AND
                                room_id = ?";

                        cmd.Parameters.AddRange(new OleDbParameter[]
                        {
                            new OleDbParameter("room_number", roomInfo.Number ?? ""),
                            new OleDbParameter("room_name", roomInfo.Name ?? ""),
                            new OleDbParameter("department_id", newDepartment.department_id),
                            new OleDbParameter("PhotoFile", roomInfo.PhotoFile ?? ""),
                            new OleDbParameter("project_id", project_id),
                            new OleDbParameter("old_department_id", old_department_id),
                            new OleDbParameter("room_id", room_id)
                        });

                        int updatedRoom = cmd.ExecuteNonQuery();
                        if (updatedRoom == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        cmd.CommandText = $@"
                            UPDATE inventories
                            SET
                                RoomNumber = ?,
                                RoomName = ?,
                                PhotoFile = IIF(PhotoFile IS NULL, '', REPLACE(PhotoFile, '{oldPath}', '{newPath}')),
                                TagPhotoFile = IIF(TagPhotoFile IS NULL, '', REPLACE(TagPhotoFile, '{oldPath}', '{newPath}')),
                                department_id = ?,
                                Department = ?
                            WHERE
                                room_id = ?";

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddRange(new OleDbParameter[]
                        {
                            new OleDbParameter("RoomNumber", roomInfo.Number ?? ""),
                            new OleDbParameter("RoomName", roomInfo.Name ?? ""),
                            new OleDbParameter("department_id", newDepartment.department_id),
                            new OleDbParameter("Department", newDepartment.description ?? ""),
                            new OleDbParameter("room_id", actualRoom.Id)
                        });

                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }


        public bool SetPhoto(int project_id, int department_id, int room_id, string photoFile)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "UPDATE room SET PhotoFile = ? WHERE project_id = ? and department_id = ? and room_id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("PhotoName", photoFile),
                new OleDbParameter("ProjectID", project_id),
                new OleDbParameter("DepartmentID", department_id),
                new OleDbParameter("RoomID", room_id)
            });

            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;

        }
    }
}
