using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.AssetsCatalogManufacturers)]
    public class ManufacturerContactsController : TableGenericController<manufacturer_contact>
    {
        public ManufacturerContactsController() : base(new [] { "manufacturer_domain_id", "manufacturer_id", "name" }, new [] { "manufacturer_domain_id", "manufacturer_id" }) { }

        [ActionName("AllWithID")]
        public IEnumerable<manufacturer_contact_all> GetAll(int id1, int id2)
        {
            using (var repository = new TableRepository<manufacturer_contact_all>())
            {
                return repository.GetAll(new [] { "manufacturer_domain_id", "manufacturer_id" }, GetIds(id1, id2), null);
            }
        }

        [ActionName("Item")]
        public manufacturer_contact Put(manufacturer_contact item, int id1, int id2, string id3)
        {
            using (var repository = new TableRepository<manufacturer_contact>())
            {
                if (!id3.Equals(item.name))
                {
                    var oldTime = item.date_added;

                    Delete(id1, id2, id3);
                    base.Add(item, id1, id2);

                    item.date_added = oldTime;
                }
                
                return repository.Update(item) ? item : null;
            }
        }

        [ActionName("Item")]
        public manufacturer_contact Get(int id1, int id2, string id3)
        {
            id3 = id3.ToLower();
            return base.GetAll(id1, id2).FirstOrDefault(mc => mc.name.ToLower().Equals(id3));
        }

        [ActionName("Item")]
        public HttpResponseMessage Delete(int id1, int id2, string id3)
        {
            using (var repository = new TableRepository<manufacturer_contact>())
            {
                repository.Delete(Get(id1, id2, id3));
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}
