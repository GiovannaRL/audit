using OfflineXPlanner.Domain;
using OfflineXPlanner.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace OfflineXPlanner.Database
{
    public class ProjectDAO : IProjectDAO
    {
        private bool ProjectExists(Project project)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM projects WHERE project_id = ?";
            cmd.Parameters.Add(new OleDbParameter("ProjectID", project.project_id));

            var result = cmd.ExecuteReader();

            bool exists = result.Read();

            DatabaseUtil.CloseConnection(conn, cmd, result);

            return exists;
        }

        public bool InsertProject(Project project)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            if (ProjectExists(project))
            {
                cmd.CommandText = "UPDATE projects SET project_name = ? WHERE project_id = ?";
            }
            else
            {
                cmd.CommandText = "INSERT INTO projects(project_name, project_id) VALUES(?, ?);";
            }
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("Description", project.description),
                new OleDbParameter("ProjectID", project.project_id)
            });

            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }

        public bool InsertIfNotExists(Project project)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            int rowsAffected = 0;
            if (!ProjectExists(project))
            {
                cmd.CommandText = "INSERT INTO projects(project_name, project_id) VALUES(?, ?);";
                cmd.Parameters.AddRange(new OleDbParameter[] {
                    new OleDbParameter("Description", project.description),
                    new OleDbParameter("ProjectID", project.project_id)
                });

                rowsAffected = cmd.ExecuteNonQuery();
            }

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }

        public List<Project> GetProjects()
        {
            List<Project> projects = new List<Project>();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "select * from projects";
            var projectsDatabase = cmd.ExecuteReader();

            while (projectsDatabase.Read())
            {
                projects.Add(new Project(Convert.ToInt32(projectsDatabase["id"]),
                    Convert.ToInt32(projectsDatabase["project_id"]),
                    projectsDatabase["project_name"].ToString()));
            }

            DatabaseUtil.CloseConnection(conn, cmd, projectsDatabase);

            return projects;
        }

        public int DeleteProjects(List<Project> projects)
        {
            if (projects == null)
            {
                return 0;
            }

            return this.DeleteProjects(projects.Select(p => p.project_id).ToList());
        }

        public int DeleteProjects(List<int> project_ids)
        {
            if (project_ids == null)
            {
                return 0;
            }

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "DELETE FROM projects WHERE project_id IN (?)";
            cmd.Parameters.Add(new OleDbParameter("ProjectsID", String.Join(",", project_ids)));

            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected;
        }

        public int DeleteProject(Project project)
        {
            if (project == null)
            {
                return 0;
            }

            return this.DeleteProject(project.project_id);
        }

        public int DeleteProject(int project_id)
        {
            return this.DeleteProjects(new List<int>() { project_id });
        }

        public int DeleteAllUnless(List<int> project_ids)
        {
            if (project_ids == null && project_ids.Count() == 0)
            {
                return 0;
            }

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "DELETE FROM projects WHERE project_id NOT IN (?)";
            cmd.Parameters.Add(new OleDbParameter("ProjectsID", String.Join(",", project_ids)));

            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected;
        }

        public void SetHasDataToUploadToTrue(int project_id)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "UPDATE projects SET has_data_to_upload = true WHERE project_id = ?";
            cmd.Parameters.Add(new OleDbParameter("ProjectID", project_id));

            cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);
        }
        public void SetHasDataToUploadToFalse(int project_id)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "UPDATE projects SET has_data_to_upload = false WHERE project_id = ?";
            cmd.Parameters.Add(new OleDbParameter("ProjectID", project_id));

            cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);
        }

        private bool CostCenterExists(CostCenter costCenter)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM cost_center WHERE project_id = ? AND LCase(code) = ?";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", costCenter.project_id),
                new OleDbParameter("Code", costCenter.code.ToLower())
            });

            var result = cmd.ExecuteReader();

            bool exists = result.Read();

            DatabaseUtil.CloseConnection(conn, cmd, result);

            return exists;
        }

        public bool InsertCostCenter(CostCenter costCenter)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            int rowsAffected = 0;
            if (!CostCenterExists(costCenter))
            {
                cmd.CommandText = "INSERT INTO cost_center(code, project_id, description, is_default) VALUES(?, ?, ?, ?);";
                cmd.Parameters.AddRange(new OleDbParameter[] {
                    new OleDbParameter("Code", costCenter.code),
                    new OleDbParameter("ProjectID", costCenter.project_id),
                    new OleDbParameter("Description", costCenter.description ?? ""),
                    new OleDbParameter("IsDefault", costCenter.is_default)
                });

                rowsAffected = cmd.ExecuteNonQuery();
            }

            DatabaseUtil.CloseConnection(conn, cmd, null);

            return rowsAffected == 1;
        }

        public DataTable GetDepartments(int project_id)
        {
            DataTable dt = new DataTable();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "select 0 as department_id, '' as description from department union select department_id, description from department where project_id = ? order by description";
            cmd.Parameters.AddRange(new OleDbParameter[] {
                new OleDbParameter("ProjectID", project_id),
                new OleDbParameter("ProjectID1", project_id)
            });

            var departments = cmd.ExecuteReader();
            dt.Load(departments);

            DatabaseUtil.CloseConnection(conn, cmd, departments);
            return dt;
        }

        public DataTable GetRooms(int project_id, int? department_id = null)
        {
            DataTable dt = new DataTable();

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.Parameters.Add(new OleDbParameter("ProjectID", project_id));
            if (department_id == null)
            {
                cmd.CommandText = "select 0 as room_id, '' as room from room union select room_id, (room_name + ' -- ' + room_number) as room from room where project_id = ? order by room";
            } else
            {
                cmd.CommandText = "select 0 as room_id, '' as room from room union select room_id, (room_name + ' -- ' + room_number) as room from room where project_id = ? and department_id = ? order by room";
                cmd.Parameters.Add(new OleDbParameter("DepartmentID", department_id.GetValueOrDefault()));
            }

            var rooms = cmd.ExecuteReader();
            dt.Load(rooms);

            DatabaseUtil.CloseConnection(conn, cmd, rooms);
            return dt;
        }
    }
}
