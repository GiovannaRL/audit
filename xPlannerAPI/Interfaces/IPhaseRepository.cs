using System;
namespace xPlannerAPI.Interfaces
{
    interface IPhaseRepository : IDisposable
    {
        bool Delete(int domain_id, int? project_id, int? phase_id);
    }
}
