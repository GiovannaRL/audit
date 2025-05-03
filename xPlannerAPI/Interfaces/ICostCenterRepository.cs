using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xPlannerAPI.Interfaces
{
    interface ICostCenterRepository : IDisposable
    {
        bool updateAllAssets(int domain_id, int project_id, int cost_center_id);
    }
}
