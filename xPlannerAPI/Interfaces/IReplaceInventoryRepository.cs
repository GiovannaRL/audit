using System;
using xPlannerAPI.Models;

namespace xPlannerAPI.Interfaces
{
    interface IReplaceInventoryRepository : IDisposable
    {
        bool Put(int domainId, int projectId, ReplaceInventory data);
    }
}
