using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    interface IFinancialsRepository : IDisposable
    {
        FinancialStructure GetAll(int domainId, int projectId, int? phaseId, int? departmentId, int? roomId);
    }
}
