using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Services;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using System;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDetails)]
    public class DepartmentsController : TableGenericController<project_department>
    {
        public DepartmentsController() : base(new [] { "domain_id", "project_id", "phase_id", "department_id" },
            new [] { "domain_id", "project_id", "phase_id" }, new [] { "department_type", "project_phase.project" })
        { }

        protected override bool UpdateReferences(project_department item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {

            if (item.department_type == null)
                return false;

            if (id4 == null || item.department_id <= 0)
            {
                item.copy_link = Guid.NewGuid();
            }

            item.department_type_id = item.department_type.department_type_id;
            item.department_type_domain_id = item.department_type.domain_id;
            item.department_type = null;

            return true;

        }

        [ActionName("AllWithFinancials")]
        public IEnumerable<department_inventory_po_Result> GetAllWithFinancials(int id1, int id2, int id3)
        {
            using (IDepartmentRepository repository = new DepartmentRepository())
            {
                return repository.GetWithFinancials(id1, id2, id3);
            }
        }

        [ActionName("Item")]
        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<project_room_inventory>())
            {
                if (repository.GetAll(new[] { "domain_id", "project_id", "phase_id", "department_id" },
                        GetIds(id1, id2, id3, id4), new[] { "inventory_purchase_order" }).Any(pri => pri.inventory_purchase_order.Any()))
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Purchase orders have been issued.");

                var files = new Dictionary<string, List<string>>
                {
                    /* Get report files name */
                    {"reports", ProjectFilesRepository.GetReportsName(id1, id2.GetValueOrDefault(), id3, id4)}
                };
                
                /* Get quote and po files name */
                var quotesPOs = ProjectFilesRepository.GetQuotePONames(id1, id2.GetValueOrDefault(), id3, id4);
                files.Add("quotes", quotesPOs["quotes"]);
                files.Add("pos", quotesPOs["pos"]);

                //CHECK IF THERE IS ASSETS INSIDE ROOMS AND SEND ERROR MESSAGE
                if (repository.GetAll(new[] { "domain_id", "project_id", "phase_id", "department_id" },
                        GetIds(id1, id2, id3, id4), null).Any())
                    return Request.CreateResponse(HttpStatusCode.Conflict, "The room has assets.");
                
                HttpResponseMessage response;

                using (var departmentsRepository = new DepartmentRepository())
                {
                    response = departmentsRepository.Delete(id1, id2, id3, id4) 
                                ? Request.CreateResponse(HttpStatusCode.OK) 
                                : Request.CreateResponse(HttpStatusCode.InternalServerError);
                }

                if (response.StatusCode != HttpStatusCode.OK)
                    return response;

                using (var fileRepository = new FileStreamRepository())
                {
                    foreach (var fileName in files["reports"])
                        fileRepository.DeleteBlob("reports", fileName);

                    foreach (var fileName in files["pos"])
                        fileRepository.DeleteBlob($"po{id1}", fileName);

                    foreach (var fileName in files["quotes"])
                        fileRepository.DeleteBlob($"quote{id1}", fileName);
                }

                return response;
            }
        }

        [ActionName("DepartmentsTable")]
        public HttpResponseMessage GetDepartmentsAsTable(int id1, int id2)
        {
            using (IDepartmentRepository repository = new DepartmentRepository())
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

    }
}