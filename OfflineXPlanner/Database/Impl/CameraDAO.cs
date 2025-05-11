using OfflineXPlanner.Utils;
using System.Data.OleDb;

namespace OfflineXPlanner.Database.Impl
{
    public class CameraDAO : ICameraDAO
    {
        public string GetLastSelectedCamera()
        {
            string selectedCamera = null;

            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();

            cmd.CommandText = "SELECT * FROM selected_camera";

            var camera = cmd.ExecuteReader();

            if (camera.Read())
            {
                selectedCamera = camera["camera_name"].ToString();
            }

            DatabaseUtil.CloseConnection(conn, cmd, camera);
            return selectedCamera;
        }

        public bool UpdateLastSelectedCamera(string camera)
        {
            var conn = DatabaseUtil.CreateConnection();
            var cmd = conn.CreateCommand();
            conn.Open();
            
            if (this.GetLastSelectedCamera() != null)
            {
                cmd.CommandText = "UPDATE selected_camera SET camera_name = ?";
            } else
            {
                cmd.CommandText = "INSERT INTO selected_camera VALUES(?)";
            }
            cmd.Parameters.Add(new OleDbParameter("CameraName", camera));
            int rowsAffected = cmd.ExecuteNonQuery();

            DatabaseUtil.CloseConnection(conn, cmd, null);
            return rowsAffected == 1;
        }
    }
}
