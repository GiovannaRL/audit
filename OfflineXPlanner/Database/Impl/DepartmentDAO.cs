using OfflineXPlanner.Domain;
using OfflineXPlanner.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace OfflineXPlanner.Database.Impl
{
    public class DepartmentDAO : IDepartmentDAO
    {
        public List<Department> GetDepartments(int project_id)
        {
            List<Department> departments = new List<Department>();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM department WHERE project_id = ?";
            cmd.Parameters.Add(new OleDbParameter("ProjectID", project_id));

            var departmentsDatabase = cmd.ExecuteReader();

            while (departmentsDatabase.Read())
            {
                departments.Add(new Department(
                    Convert.ToInt32(departmentsDatabase["department_id"]),
                    departmentsDatabase["description"].ToString(),
                    departmentsDatabase["type"].ToString(),
                    Convert.ToInt32(departmentsDatabase["project_id"])));
            }

            DatabaseUtil.CloseConnection(conn, cmd, departmentsDatabase);
            return departments;
        }

        public Department GetDeparment(int project_id, int department_id)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM department WHERE project_id = ? and department_id = ?";
            cmd.Parameters.Add(new OleDbParameter("ProjectID", project_id));
            cmd.Parameters.Add(new OleDbParameter("DepartmentID", department_id));

            var departmentsDatabase = cmd.ExecuteReader();
            if (departmentsDatabase.Read())
            {
                return new Department(
                    Convert.ToInt32(departmentsDatabase["department_id"]),
                    departmentsDatabase["description"].ToString(),
                    departmentsDatabase["type"].ToString(),
                    Convert.ToInt32(departmentsDatabase["project_id"]));
            }
            else
                return null;

        }

        public DataTable GetDepartmentsAsDataTable(int project_id)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM department WHERE project_id = ?";
            cmd.Parameters.Add(new OleDbParameter("ProjectID", project_id));

            var departmentsDatabase = cmd.ExecuteReader();

            DataTable dt = new DataTable();
            dt.Load(departmentsDatabase);

            DatabaseUtil.CloseConnection(conn, cmd, departmentsDatabase);

            return dt;
        }

        public bool InsertDepartment(Department dpt)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "INSERT INTO department(project_id, department_id, description, type) VALUES(?, ?, ?, ?)";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", dpt.project_id),
                new OleDbParameter("DepartmentID", dpt.department_id),
                new OleDbParameter("Description", dpt.description),
                new OleDbParameter("Type", dpt.type)
            });

            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }

        public int GetMaxDepartmentID(int project_id)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT MAX(department_id) AS max_dpt_id FROM department WHERE project_id = ?";
            cmd.Parameters.Add(new OleDbParameter("ProjectID", project_id));

            var departmentsDatabase = cmd.ExecuteReader();

            int maxDepartmentID = 0;
            if (departmentsDatabase.Read() && !departmentsDatabase.IsDBNull(0))
            {
                maxDepartmentID = Convert.ToInt32(departmentsDatabase["max_dpt_id"]);
            }

            DatabaseUtil.CloseConnection(conn, cmd, departmentsDatabase);

            return maxDepartmentID;
        }

        public bool UpdateDepartment(Department dpt)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "UPDATE department SET description = ?, type = ? WHERE project_id = ? AND department_id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("Description", dpt.description),
                new OleDbParameter("Type", dpt.type),
                new OleDbParameter("ProjectID", dpt.project_id),
                new OleDbParameter("DepartmentID", dpt.department_id)
            });

            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected > 0;
        }

        public bool InsertIfNotExists(Department dpt)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM department WHERE project_id = ? AND department_id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", dpt.project_id),
                new OleDbParameter("DepartmentID", dpt.department_id)
            });

            var result = cmd.ExecuteReader();

            if (!result.Read())
            {

                DatabaseUtil.CloseConnection(conn, cmd, result);
                return InsertDepartment(dpt);
            }

            DatabaseUtil.CloseConnection(conn, cmd, result);
            return false;
        }

        public bool DeleteDepartment(int projectId, int departmentId)
        {

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = "select * FROM room WHERE project_id = ? AND department_id = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", projectId),
                new OleDbParameter("DepartmentID", departmentId)
            });

            var result = cmd.ExecuteReader();

            try
            {
                while (result.Read())
                {
                    IRoomDAO room = new RoomDAO();
                    room.DeleteRoom(projectId, departmentId, (int)result["room_id"]);
                }
                result.Close();

                cmd.CommandText = "delete FROM department WHERE project_id = ? AND department_id = ?";
                cmd.ExecuteNonQuery();

                DatabaseUtil.CloseConnection(conn, cmd, null);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
