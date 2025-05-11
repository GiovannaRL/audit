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
    [AudaxAuthorize(AreaModule = AreaModule.AssetsCatalogVendors)]
    public class AssetsVendorController : TableGenericController<assets_vendor>
    {
        public AssetsVendorController() : base(new [] { "asset_domain_id", "asset_id", "vendor_domain_id", "vendor_id" }, new [] { "asset_domain_id", "asset_id" }, new [] { "vendor" }) { }

        [ActionName("Item")]
        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<purchase_order>())
            {
                if (!repository.GetAll(new [] { "vendor_domain_id", "vendor_id" }, new [] { id3.GetValueOrDefault(), id4.GetValueOrDefault() }, new [] { "inventory_purchase_order" })
                    .Any(po => po.inventory_purchase_order.Any(ipo => ipo.asset_domain_id == id1 && ipo.asset_id == id2)))
                {
                    return base.Delete(id1, id2, id3, id4, id5);
                }
            }

            return Request.CreateResponse(HttpStatusCode.Conflict, "The vendor could not be deleted. Purchase orders have been issued.");
        }

        [ActionName("Item")]
        public override assets_vendor Put(assets_vendor item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<purchase_order>())
            {
                /* PO verification removed as requested in defect 2140 */

                //if (!repository.GetAll(new [] { "vendor_domain_id", "vendor_id" }, new [] { id3.GetValueOrDefault(), id4.GetValueOrDefault() }, new [] { "inventory_purchase_order" })
                //    .Any(po => po.inventory_purchase_order.Any(ipo => ipo.asset_domain_id == id1 && ipo.asset_id == id2)))
                //{
                    return base.Put(item, id1, id2, id3, id4, id5);
                //}

                //throw new HttpResponseException(HttpStatusCode.Conflict);
            }
        }
    }
}
