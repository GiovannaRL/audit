using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Models;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.GeneralInfoDomain)]
    public class TreeViewsController : AudaxWareController
    {
        // If the id (domain_id) is not null get all the projects of that domain
        [ActionName("All")]
        public HttpResponseMessage GetProjects(int id1)
        {
            var roleRepository = new AspNetUserRoleRepository();
            var role = roleRepository.GetId((short)id1, UserName);

            using (ITreeViewRepository repository = new TreeViewRepository())
            {
                var trees = repository.MountTree(id1, AudaxWareIdentity, UserId);                
                var status = HttpStatusCode.OK;

                if (trees != null)
                    return Request.CreateResponse(status, trees);

                status = HttpStatusCode.NotFound;
                return Request.CreateResponse(status);
            }
        }

        [ActionName("DepartmentsTable")]
        public HttpResponseMessage GetDepartmentsTable(int id1)
        {
            using (ITreeViewRepository repository = new TreeViewRepository())
            {
                var tableData = repository.GetDepartmentAsTable(id1, AudaxWareIdentity).Select(d => new
                {
                    domain_id = d.domain_id,
                    project_id = d.project_phase.project_id,
                    project_desc =
                    d.project_phase.project.project_description,
                    phase_id = d.phase_id,
                    phase_desc = d.project_phase.description,
                    department_id = d.department_id,
                    department_desc = d.description
                }).ToArray();

                var status = HttpStatusCode.OK;

                if (!tableData.Any())
                    status = HttpStatusCode.NotFound;

                return Request.CreateResponse(status, tableData);
            }
        }

        [ActionName("PhasesTable")]
        public HttpResponseMessage GetPhaseAsTableWithEmptyDepartments(int id1)
        {
            using (ITreeViewRepository repository = new TreeViewRepository())
            {
                var tableData = repository.GetPhaseAsTableWithEmptyDepartments(id1, AudaxWareIdentity);
                var status = HttpStatusCode.OK;

                if (!tableData.Any())
                    status = HttpStatusCode.NotFound;

                return Request.CreateResponse(status, tableData);
            }
        }
        
        [ActionName("LocationsTable")]
        public HttpResponseMessage GetRoomsTable(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (ITreeViewRepository repository = new TreeViewRepository())
            {
                var tableData = repository.GetRoomsAsTable(AudaxWareIdentity, id1, id2, id3, id4, id5);

                if (id4 != null)
                    return Request.CreateResponse(HttpStatusCode.OK, tableData.Select(r => new
                    {
                        domain_id = id1,
                        project_id = id2,
                        phase_id = id3,
                        department_id = id4,
                        room_id = r.room_id,
                        room_desc = r.drawing_room_name + (string.IsNullOrEmpty(r.drawing_room_number) ? "" : " - " + r.drawing_room_number) + (r.room_quantity > 1 ? " (" + r.room_quantity + ")" : "")
                    }));


                if (id3 != null)
                    return Request.CreateResponse(HttpStatusCode.OK, tableData.Select(r => new
                    {
                        domain_id = id1,
                        project_id = id2,
                        phase_id = id3,
                        department_id = r.department_id,
                        department_desc = r.project_department.description,
                        room_id = r.room_id,
                        room_desc = r.drawing_room_name + (string.IsNullOrEmpty(r.drawing_room_number) ? "" : " - " + r.drawing_room_number) + (r.room_quantity > 1 ? " (" + r.room_quantity + ")" : "")
                    }));

                if (id2 != null)
                    return Request.CreateResponse(HttpStatusCode.OK, tableData.Select(r => new
                    {
                        domain_id = id1,
                        project_id = id2,
                        phase_id = r.project_department.phase_id,
                        phase_desc = r.project_department.project_phase.description,
                        department_id = r.department_id,
                        department_desc = r.project_department.description,
                        room_id = r.room_id,
                        room_desc = r.drawing_room_name + (string.IsNullOrEmpty(r.drawing_room_number) ? "" : " - " + r.drawing_room_number) + (r.room_quantity > 1 ? " (" + r.room_quantity + ")" : "")
                    }));

                return Request.CreateResponse(HttpStatusCode.OK, tableData.Select(r => new
                {
                    domain_id = id1,
                    project_id = r.project_id,
                    project_desc = r.project_department.project_phase.project.project_description,
                    phase_id = r.phase_id,
                    phase_Desc = r.project_department.project_phase.description,
                    department_id = r.department_id,
                    department_desc = r.project_department.description,
                    room_id = r.room_id,
                    room_desc = r.drawing_room_name + (string.IsNullOrEmpty(r.drawing_room_number) ? "" : " - " + r.drawing_room_number) + (r.room_quantity > 1 ? " (" + r.room_quantity + ")" : "")
                }));

            }
        }
        
        [ActionName("NotLinkedRooms")]
        public HttpResponseMessage GetNotLinked(int id1, int id2, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (ITreeViewRepository repository = new TreeViewRepository())
            {
                var tableData = repository.GetRoomsAsTable(AudaxWareIdentity, id1, id2, id3, id4, id5).Where(r => r.linked_template != true);

                return Request.CreateResponse(HttpStatusCode.OK, tableData.Select(r => new
                {
                    domain_id = id1,
                    project_id = id2,
                    phase_id = r.phase_id,
                    phase_desc = r.project_department.project_phase.description,
                    department_id = r.department_id,
                    department_desc = r.project_department.description,
                    room_id = r.room_id,
                    room_desc = r.drawing_room_name + (string.IsNullOrEmpty(r.drawing_room_number) ? "" : " - " + r.drawing_room_number) + (r.room_quantity > 1 ? " (" + r.room_quantity + ")" : "")
                }));
            }
        }
    }
}