using OfflineXPlanner.Domain;
using System.Collections.Generic;
using System.Data;

namespace OfflineXPlanner.Database
{
    public interface IDepartmentDAO
    {
        List<Department> GetDepartments(int project_id);
        Department GetDeparment(int project_id, int department_id);
        DataTable GetDepartmentsAsDataTable(int project_id);
        bool InsertDepartment(Department dpt);
        bool InsertIfNotExists(Department dpt);
        int GetMaxDepartmentID(int project_id);
        bool UpdateDepartment(Department dpt);
        bool DeleteDepartment(int projectId, int departmentId);
    }
}
