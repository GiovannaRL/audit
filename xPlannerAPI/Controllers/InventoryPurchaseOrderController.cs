using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Models;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Attributes;
using xPlannerAPI.Services;
using xPlannerCommon.Enumerators;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsPurchaseOrders)]
    public class InventoryPurchaseOrderController : TableGenericController<inventory_purchase_order>
    {
        public InventoryPurchaseOrderController() : base(new [] { "po_domain_id", "project_id", "po_id", "inventory_id" }, new [] { "po_domain_id", "project_id", "po_id" }) { }

        [ActionName("All")]
        public IEnumerable<Get_PO_assigned_assets_Result> GetAll(short id1, int id2, int id3)
        {
            using (IPurchaseOrderRepository repository = new PurchaseOrderRepository())
            {
                return repository.GetAssignedAssets(id1, id2, id3);
            }
        }

        [ActionName("ToAssign")]
        public IEnumerable<get_available_assets_to_PO_Result> GetToAssign(short id1, int id2, int? id3 = null, int? id4 = null, int? id5 = null, bool? id6 = false)
        {
            using (IPurchaseOrderRepository repository = new PurchaseOrderRepository())
            {
                return repository.GetAssetsToAssign(id1, id2, id3, id4, id5, id6);
            }
        }

        [ActionName("Item")]
        public HttpResponseMessage Add(int id1, int id2, int id3, [FromBody] List<asset_inventory> items)
        {
            if (items == null || items.Count <= 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            using (IPurchaseOrderRepository repository = new PurchaseOrderRepository())
            {
                return Request.CreateResponse(repository.AddAssets(id1, id2, id3, items, UserName) ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
            }
        }

        [ActionName("Item")]
        public HttpResponseMessage Put([FromBody]List<InventoryPurchaseOrder> items, int id1, int id2, int id3)
        {
            foreach (InventoryPurchaseOrder item in items)
            {
                var ids = item.inventory_ids.Split(';');

                foreach (var id in ids)
                {
                    int idInt;

                    if (!int.TryParse(id, out idInt))
                        continue;

                    var dbItem = base.GetItem(id1, id2, id3, idInt);
                    dbItem.po_unit_amt = item.total_po_amt != null ? item.total_po_amt / item.po_qty : 0;
                    base.Put(dbItem, id1, id2, id3, idInt);

                    using (IPurchaseOrderRepository repository = new PurchaseOrderRepository())
                    {
                        repository.UpdateInventoryData(id1, idInt, item.current_location, item.delivered_date, item.received_date);
                    }

                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
