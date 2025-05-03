using System;
using System.Collections.Generic;
using System.Security.Claims;
using xPlannerCommon.Models;
using xPlannerAPI.Models;

namespace xPlannerAPI.Interfaces
{
    interface ITreeViewRepository : IDisposable
    {
        List<get_project_treeview_Result> GetAll(int domainId, ClaimsIdentity identity);
        IEnumerable<project_department> GetDepartmentAsTable(int domainId, ClaimsIdentity identity);
        IEnumerable<DepartmentTreeView> GetPhaseAsTableWithEmptyDepartments(int domainId, ClaimsIdentity identity);
        IEnumerable<project_room> GetRoomsAsTable(ClaimsIdentity identity, int domainId, int? projectId, int? phaseId = null,
            int? departmentId = null, int? roomId = null);
        List<ProjectTreeItems> MountTree(int id1, ClaimsIdentity identity, string UserId);
    }
}
