using System;
using System.Collections.Generic;
using System.Security.Claims;
using xPlannerAPI.Models;
namespace xPlannerAPI.Interfaces
{
    interface IPhaseRepository : IDisposable
    {
        bool Delete(int domain_id, int? project_id, int? phase_id);
        IEnumerable<DepartmentTreeView> GetPhaseAsTable(int domainId, ClaimsIdentity identity);
    }
}
