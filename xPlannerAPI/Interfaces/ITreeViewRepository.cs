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
        List<ProjectTreeItems> MountTree(int id1, ClaimsIdentity identity, string UserId);
    }
}
