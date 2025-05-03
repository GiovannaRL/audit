using System.Collections.Generic;
using System.Data;
using OfflineXPlanner.Domain;

namespace OfflineXPlanner.Database
{
    public interface IRoomDAO
    {
        List<Room> GetAllFromProject(int projectId);
        List<Room> GetAllFromProjectWithDepartment(int projectId);
        DataTable Get(int projectId, int departmentId);
        Room Get(int projectId, int departmentId, int roomId);

        /**
         * Returns the added room with the generated ID
         */
        Room Insert(Room room);
        bool InsertIfNotExists(Room room);
        bool Update(Room room);
        bool DeleteRoom(int projectId, int departmentId, int roomId);
        Room DuplicateItem(int project_id, int department_id, int room_id, Room newRoomInfo);
        bool SetPhoto(int project_id, int department_id, int room_id, string photoFile);
    }
}
