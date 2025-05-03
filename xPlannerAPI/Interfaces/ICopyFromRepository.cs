using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xPlannerAPI.Interfaces
{
    interface ICopyFromRepository : IDisposable
    {
        string Copy(short domain_id, int project_id, int phase_id, int department_id, int room_id, short cp_domain_id, int cp_project_id,
             int cp_phase_id, int cp_department_id, int cp_room_id, string addedBy, bool cp_opt_col);

        string CopyProject(short domainId, int projectId, string projectDescription, bool copyUser, string addedBy);
    }
}
