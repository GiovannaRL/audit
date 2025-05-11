using System;
using System.Collections.Generic;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    interface IDepartmentRepository : IDisposable
    {
        List<department_inventory_po_Result> GetWithInventoryPO(int domain_id, int project_id, int phase_id);
        bool Delete(int domain_id, int? project_id, int? phase_id, int? department_id);
    }
}
