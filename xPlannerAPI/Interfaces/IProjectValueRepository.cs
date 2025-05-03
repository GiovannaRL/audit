using System;

namespace xPlannerAPI.Interfaces
{
    interface IProjectValueRepository : IDisposable
    {
        object Get(int domainId, int projectId);
        object Get(int domainId, int projectId, int? phaseId, int? departmentId, int? roomId);
    }
}
