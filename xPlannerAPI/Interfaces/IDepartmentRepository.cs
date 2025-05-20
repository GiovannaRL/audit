using System;
using System.Collections.Generic;
using System.Security.Claims;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    interface IDepartmentRepository : IDisposable
    {
        List<department_inventory_po_Result> GetWithFinancials(int domain_id, int project_id, int phase_id);
        bool Delete(int domain_id, int? project_id, int? phase_id, int? department_id);
        IEnumerable<project_department> GetDepartmentAsTable(int domainId, ClaimsIdentity identity);
    }
}
