using System.Net;
using System.Net.Http;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsDetails)]
    public class AddressController : TableGenericController<project_addresses>
    {
        public AddressController() : base(new[] { "domain_id", "project_id", "id" }, new[] { "domain_id", "project_id" }) { }

        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (base.GetItem(id1, id2, id3, id4, id5) == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            using (var repository = new TableRepository<purchase_order>())
            {
                if (repository.GetAll(new[] { "domain_id", "project_id", "ship_to" }, GetIds(id1, id2, id3), null).Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "This address is assigned to one or more purchase orders and cannot be deleted");
                }

                base.Delete(id1, id2, id3, id4, id5);
                return Request.CreateResponse(HttpStatusCode.OK);
            }

        }
    }
}
