using System;

namespace xPlannerAPI.Interfaces
{
    interface IProjectMineRepository : IDisposable
    {
        bool Remove(int domainId, int projectId, string userId);
    }
}
