using System;
using System.Collections.Generic;
using xPlannerAPI.Models;
using xPlannerCommon.Models;

namespace xPlannerAPI.Interfaces
{
    interface IBudgetCopilotRepository : IDisposable
    {
        BudgetCopilot GetBudgetCopilot(int id1, int? id2, string id3, int? id4);
    }
}