using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xPlannerCommon.Models
{
    public static class  report_locationExtensions
    {
        public static project_room GetRoom(this report_location loc, audaxwareEntities db)
        {
            var room = db.project_room.Include("project_department.project_phase").Where(x => x.domain_id == loc.project_domain_id && x.project_id == loc.project_id && x.phase_id == loc.phase_id && x.room_id == loc.room_id).FirstOrDefault();
            return room;
        }
    }
}
