using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Models;
using xPlannerCommon.App_Data;
using xPlannerCommon.Enumerators;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.AssetsCatalogManufacturers)]
    public class ManufacturerController : TableGenericController<manufacturer>
    {
        public ManufacturerController() : base(new [] { "domain_id", "manufacturer_id" }, new [] { "domain_id" }, new [] { "domain" }, true, true) { }

        protected override bool UpdateReferences(manufacturer item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            item.domain = null;
            return true;
        }

        protected override manufacturer DuplicateItem(manufacturer item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var oldManufacturerId = item.manufacturer_id;
            var manufacturer = Add(item, id1, id2, id3, id4, id5);

            using (var repository = new DuplicateRepository())
            {
                repository.DuplicateManufacturerItems(1, oldManufacturerId, manufacturer.domain_id, manufacturer.manufacturer_id, UserName);
            }

            return manufacturer;
        }

        [ActionName("All")]
        public override IEnumerable<manufacturer> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var manufacturers = base.GetAll(id1, id2, id3, id4, id5);

            if (isLoggedAsManufacturer())
            {
                manufacturers = manufacturers.Where(m => HasManufacturerAccess(m.domain_id, m.manufacturer_id));
            }

            return manufacturers.Select(m => {
                m.domain = new domain
                {
                    name = Helper.GetEnterpriseName(m.domain.name),
                    domain_id = m.domain.domain_id
                };
                return m;
            });
        }

        [ActionName("AllSorted")]
        public IEnumerable<manufacturer> GetAllSorted(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            return GetAll(id1, id2, id3, id4, id5).OrderBy(m => m.manufacturer_description);
        }

        [ActionName("Item")]
        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<asset>())
            {
                if (!repository.GetAll(new [] { "manufacturer_domain_id", "manufacturer_id" }, GetIds(id1, id2), null).Any())
                {
                    return base.Delete(id1, id2);
                }

                return Request.CreateResponse(HttpStatusCode.Conflict, "Manufacturer cannot be deleted. It has assigned asset!");
            }
        }

        [ActionName("Vendors")]
        public List<vendor> Get(short id1, int id2)
        {
            using (var repository = new ManufacturerVendorsRepository())
            {
                return repository.GetAllVendors(id1, id2);
            }
        }
    }
}
