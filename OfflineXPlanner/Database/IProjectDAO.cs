using OfflineXPlanner.Domain;
using System.Collections.Generic;
using System.Data;

namespace OfflineXPlanner.Database
{
    public interface IProjectDAO
    {
        bool InsertProject(Project project);
        bool InsertIfNotExists(Project project);
        List<Project> GetProjects();
        int DeleteProjects(List<Project> projects);
        int DeleteProjects(List<int> project_ids);
        int DeleteProject(Project project);
        int DeleteProject(int id);
        void SetHasDataToUploadToTrue(int project_id);
        void SetHasDataToUploadToFalse(int project_id);
        bool InsertCostCenter(CostCenter costCenter);

        /// <summary>
        ///  Delete all projects that project_id is not in the project_ids list
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        int DeleteAllUnless(List<int> project_ids);
        DataTable GetDepartments(int project_id);
        DataTable GetRooms(int project_id, int? department_id = null);
    }
}
