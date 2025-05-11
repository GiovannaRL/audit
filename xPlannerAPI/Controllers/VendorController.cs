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
    [AudaxAuthorize(AreaModule = AreaModule.AssetsCatalogVendors)]
    public class VendorController : TableGenericController<vendor>
    {
        public VendorController() : base(new [] { "domain_id", "vendor_id" }, new [] { "domain_id" }, new [] { "domain" }, true, true) { }

        protected override bool UpdateReferences(vendor item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            item.domain = null;
            return true;
        }

        protected override vendor DuplicateItem(vendor item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var oldVendorId = item.vendor_id;
            var vendor = Add(item, id1, id2, id3, id4, id5);

            using (var repository = new DuplicateRepository())
            {
                repository.DuplicateVendorItems(1, oldVendorId, vendor.domain_id, vendor.vendor_id);
            }

            return vendor;
        }

        [ActionName("ToAssign")]
        public IEnumerable<vendor> GetToAssign(int id1, int id2, int id3)
        {
            using (var repository = new TableRepository<vendor>())
            {
                var vendors = repository.GetAll(new [] { "domain_id" }, GetIds(id1), new [] { "assets_vendor" }, true);
                return vendors.Where(v => !v.assets_vendor.Any(av => av.asset_id == id3 && av.asset_domain_id == id2)).OrderBy(v => v.name);
            }
        }

        [ActionName("ToAssignEdit")]
        public IEnumerable<vendor> GetToAssignEdit(int id1, int id2, int id3, int id4, int id5)
        {
            using (var repository = new TableRepository<vendor>())
            {
                var vendors = repository.GetAll(new [] { "domain_id" }, GetIds(id1), new [] { "assets_vendor" }, true);
                return vendors.Where(v => !v.assets_vendor.Any(av => av.asset_id == id3 && av.asset_domain_id == id2 && (av.vendor_domain_id != id4 || av.vendor_id != id5))).OrderBy(v => v.name);
            }
        }

        [ActionName("Item")]
        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<assets_vendor>())
            {
                if (!repository.GetAll(new [] { "vendor_domain_id", "vendor_id" }, GetIds(id1, id2), null).Any())
                {
                    using (var repository1 = new TableRepository<purchase_order>())
                    {
                        if (!repository1.GetAll(new [] { "vendor_domain_id", "vendor_id" }, GetIds(id1, id2), null).Any())
                        {
                            return base.Delete(id1, id2, id3, id4, id5);
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.Conflict, "The vendor could not be deleted. Purchase orders have been issued!");
            }
        }

        [ActionName("Manufacturers")]
        public List<manufacturer> Get(short id1, int id2)
        {
            using (var repository = new ManufacturerVendorsRepository())
            {
                return repository.GetAllManufacturers(id1, id2);
            }
        }

        [ActionName("All")]
        public override IEnumerable<vendor> GetAll(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var vendors = base.GetAll(id1, id2, id3, id4, id5).ToList();

            for (var i = 0; i < vendors.Count; i++)
            {
                var domain = new domain
                {
                    name = Helper.GetEnterpriseName(vendors.ElementAt(i).domain.name),
                    domain_id = vendors.ElementAt(i).domain.domain_id
                };
                vendors.ElementAt(i).domain = domain;
            }
            return vendors;
        }

    }
}
