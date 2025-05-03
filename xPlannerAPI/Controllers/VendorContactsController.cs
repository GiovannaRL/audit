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
    [AudaxAuthorize(AreaModule = AreaModule.AssetsCatalogVendors)]
    public class VendorContactsController : TableGenericController<vendor_contact>
    {
        public VendorContactsController() : base(new [] { "vendor_contact_id" }, new [] { "vendor_domain_id", "vendor_id", "domain_id" }, true) { }

        [ActionName("AllWithID")]
        public IEnumerable<vendor_contact> GetAll(int id1, int id2, int id3, int? purchaseOrderId = null, int? purchaseOrderDomainId = null, int? purchaseOrderProjectId = null) // id4 = purchase_order (optional)
        {
            using (var repository = new TableRepository<vendor_contact>())
            {
                var contacts = repository.GetAll(new[] { "vendor_domain_id", "vendor_id", "domain_id" }, GetIds(id1, id2, id3), purchaseOrderId != null ? new[] { "purchase_order" } : null, true);
                if (purchaseOrderId != null)
                {
                    contacts = contacts.Where(c => c.purchase_order.Any(p => p.po_id == purchaseOrderId 
                                                                             && p.domain_id == purchaseOrderDomainId 
                                                                             && p.project_id == purchaseOrderProjectId)).ToList();
                }
                return contacts;
            }
        }

        [ActionName("Item")]
        public vendor_contact Add(vendor_contact item, int id1, int id2, int id3)
        {
            using (var repository = new TableRepository<vendor_contact>())
            {
                var purchaseOrderToInsert = item.purchase_order;

                if (item.purchase_order.Count > 0)
                {
                    item.purchase_order = new List<purchase_order>();
                }
                base.Add(item, id1, id2, id3);

                if (purchaseOrderToInsert != null && purchaseOrderToInsert.Count > 0)
                {
                    using (var db = new audaxwareEntities())
                    {
                        purchaseOrderToInsert.ToList().ForEach(p =>
                        {
                            var po = db.purchase_order.Find(p.po_id, p.project_id, p.domain_id);
                            db.vendor_contact.Attach(item);
                            po.vendor_contact.Add(item);
                        });

                        db.SaveChanges();
                    }
                }

                return item;
            }
        }

        [ActionName("Item")]
        public vendor_contact Get(int id1, int id2, int id3, string id4)
        {
            id4 = id4.ToLower();
            return base.GetAll(id1, id2, id3).FirstOrDefault(mc => mc.name.ToLower().Equals(id4));
        }

        [ActionName("Item")]
        public HttpResponseMessage Delete(int id1, int id2, int id3, string id4)
        {
            using (var repository = new TableRepository<vendor_contact>())
            {
                repository.Delete(Get(id1, id2, id3, id4));
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}
