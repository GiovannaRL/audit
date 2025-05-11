using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;
using xPlannerCommon.Services;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDocuments)]
    public class DocumentsAssociationsController : TableGenericController<documents_associations>
    {

        public DocumentsAssociationsController() : base(new [] { "project_domain_id", "project_id", "document_id" }, new [] { "project_domain_id", "project_id", "id" }) { }

        [ActionName("AllNames")]
        public IEnumerable<get_doc_associations_names_Result> GetAll(short id1, int id2, int id3)
        {
            using (var rep = new ProceduresRepository())
            {
                var items = rep.GetDocAssociations(id1, id2, id3);
                return items;
            }
        }
    }
}
