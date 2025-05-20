using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using xPlannerAPI.Interfaces;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDetails)]
    public class PhasesController : TableGenericController<project_phase>
    {
        public PhasesController() : base(new[] { "domain_id", "project_id", "phase_id" },
            new[] { "domain_id", "project_id" }, new[] { "project" })
        { }

        protected override bool UpdateReferences(project_phase item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (id3 == null || item.phase_id <= 0)
            {
                item.copy_link = Guid.NewGuid();
            }

            return base.UpdateReferences(item, id1, id2, id3, id4, id5);
        }


        [ActionName("Item")]
        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<project_room_inventory>())
            {
                var inventories = repository.GetAll(new[] { "domain_id", "project_id", "phase_id" }, GetIds(id1, id2, id3),
                        new[] { "inventory_purchase_order" });
                if (inventories.Any(pri => pri.inventory_purchase_order.Any()))
                    return Request.CreateResponse(HttpStatusCode.Conflict, "The phase could not be deleted because purchase orders have been issued.");

                var files = new Dictionary<string, List<string>>
                {
                    /* Get report files name */
                    {"reports", ProjectFilesRepository.GetReportsName(id1, id2.GetValueOrDefault(), id3)}
                };

                /* Get quote and po files name */
                var quotesPOs = ProjectFilesRepository.GetQuotePONames(id1, id2.GetValueOrDefault(), id3);
                files.Add("quotes", quotesPOs["quotes"]);
                files.Add("pos", quotesPOs["pos"]);

                /* Get documents phases files name */
                files.Add("phases", ProjectFilesRepository.GetPhaseDocumentsName(id1, id2.GetValueOrDefault(), id3));

                HttpResponseMessage response;

                using (var phaseRepository = new PhaseRepository())
                {
                    response = phaseRepository.Delete(id1, id2, id3)
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

                    foreach (var fileName in files["phases"])
                        fileRepository.DeleteBlob($"document{id1}", fileName);
                }

                return response;
            }
        }

        [ActionName("PhasesTable")]
        public HttpResponseMessage GetPhaseAsTable(int id1, int id2)
        {
            using (IPhaseRepository repository = new PhaseRepository())
            {
                var tableData = repository.GetPhaseAsTable(id1, AudaxWareIdentity);
                var status = HttpStatusCode.OK;

                if (!tableData.Any())
                    status = HttpStatusCode.NotFound;

                return Request.CreateResponse(status, tableData);
            }
        }
    }
}
