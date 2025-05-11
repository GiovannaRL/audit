using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsPurchaseOrders)]
    public class PurchaseOrdersController : TableGenericController<purchase_order>
    {

        public PurchaseOrdersController() : base(new [] { "domain_id", "project_id", "po_id" }, new [] { "domain_id", "project_id" }, new [] { "project_addresses", "vendor.vendor_contact", "vendor_contact" }) { }

        protected override bool UpdateReferences(purchase_order item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (id2 == null && item.po_id <= 0)
            {
                item.copy_link = Guid.NewGuid();
            }

            if (item.vendor == null)
                return false;

            item.vendor_id = item.vendor.vendor_id;
            item.vendor_domain_id = item.vendor.domain_id;
            item.vendor = null;
            item.vendor_contact = null;

            //I have no idea why this compare
            //if (id3 == null)
            //    item.status = "Open";
            if (item.status == null)
                item.status = "Open";

            if (item.freight == null)
                item.freight = 0;
            if (item.warehouse == null)
                item.warehouse = 0;
            if (item.tax == null)
                item.tax = 0;
            if (item.warranty == null)
                item.warranty = 0;
            if (item.misc == null)
                item.misc = 0;
            if (item.install == null)
                item.install = 0;

            item.upd_asset_value = id1 == 3;

            return true;
        }


        [ActionName("AllGrouped")]
        public List<get_purchase_orders_Result> GetAll(int id1, int id2, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (IPurchaseOrderRepository repository = new PurchaseOrderRepository())
            {
                return repository.GetAll(id1, id2, id3, id4, id5);

            }
        }

        [ActionName("RequestPO")]
        public purchase_order PutRequestPO(purchase_order item, int id1, int id2, int id3)
        {
            var po = base.GetItem(id1, id2, id3);

            po.status = "PO Requested";
            po.po_requested_date = item.po_requested_date;
            po.po_requested_number = item.po_requested_number;
            po.po_number = null;
            po.po_received_date = null;

            return base.Put(po, id1, id2, id3);
        }

        [ActionName("RequestQuote")]
        public purchase_order PutRequestQuote(purchase_order item, int id1, int id2, int id3)
        {
            var po = base.GetItem(id1, id2, id3);

            po.status = "Quote Requested";
            po.quote_requested_date = item.quote_requested_date;
            po.quote_expiration_date = item.quote_expiration_date;
            po.po_number = null;
            po.po_requested_number = null;
            po.quote_number = null;
            po.quote_received_date = null;
            po.po_requested_date = null;
            po.po_received_date = null;

            return base.Put(po, id1, id2, id3);
        }

        [ActionName("ReceiveQuote")]
        public purchase_order PutReceiveQuote(purchase_order item, int id1, int id2, int id3)
        {
            var po = base.GetItem(id1, id2, id3);

            po.status = "Quote Received";
            po.quote_received_date = item.quote_received_date;
            po.quote_number = item.quote_number;
            po.quote_amount = item.quote_amount;
            po.quote_expiration_date = item.quote_expiration_date;
            po.po_requested_number = null;
            po.po_requested_date = null;
            po.po_number = null;
            po.po_received_date = null;

            return base.Put(po, id1, id2, id3);
        }

        [ActionName("ReceivePO")]
        public purchase_order PutReceivePO(purchase_order item, int id1, int id2, int id3)
        {
            var po = base.GetItem(id1, id2, id3);

            po.status = "PO Issued";
            po.po_received_date = item.po_received_date;
            po.po_number = item.po_number;

            return base.Put(po, id1, id2, id3);
        }

        public override purchase_order Put(purchase_order item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            
            var po = new TableRepository<purchase_order>();
            var oldPurchaserOrder = po.GetFirst(new[] { "domain_id", "project_id", "po_id" }, GetIds(id1, id2, id3), new [] {"vendor_contact"});
            var oldVendor = oldPurchaserOrder.vendor_contact.ToList();

            /* remove PO contacts*/
            if (oldPurchaserOrder.vendor_id != item.vendor.vendor_id)
            {
                RemoveVendorContacts(oldVendor, id1, id2??0, id3??0);
            }       
            
            using (var repository = new TableRepository<inventory_purchase_order>())
            {
                var assets = repository.GetAll(new [] { "po_domain_id", "project_id", "po_id" }, new [] { item.domain_id, item.po_id, item.project_id }, null);
                var totalPOValues = assets.Sum(a => a.po_qty * a.po_unit_amt) + item.warehouse + item.warranty + item.freight + item.tax + item.misc;

                if (totalPOValues != item.quote_amount || !item.upd_asset_value)
                    return base.Put(item, id1, id2, id3, id4, id5);

                foreach (var asset in assets)
                {
                    if (asset.po_qty * asset.po_unit_amt > 0)
                    {
                        using (IPurchaseOrderRepository repo = new PurchaseOrderRepository())
                        {
                            repo.UpdateAssetCost(item.domain_id, item.po_id, item.project_id, (short)asset.asset_domain_id, asset.asset_id, asset.inventory_id, asset.po_unit_amt);
                        }
                    }
                }

                return base.Put(item, id1, id2, id3, id4, id5);
            }
        }

        [ActionName("AddVendorContacts")]
        public void AddVendorContacts(IList<vendor_contact> vendor_contacts, int id1, int id2, int id3) 
        {
            using (var repository = new TableRepository<purchase_order>())
            {
                var p = repository.Get(new[] { "domain_id", "project_id", "po_id" }, new[] { id1, id2, id3 }, null);
                vendor_contacts.ToList().ForEach(v =>
                {
                    repository.AttachItem(v);
                    p.vendor_contact.Add(v);
                });
                repository.Update(p);
            }
        }

        [ActionName("RemoveVendorContacts")]
        public void RemoveVendorContacts(IList<vendor_contact> vendor_contacts, int id1, int id2, int id3)
        {
            using (var repository = new TableRepository<purchase_order>())
            {
                var p = repository.Get(new[] { "domain_id", "project_id", "po_id" }, new[] { id1, id2, id3 }, new[] { "vendor_contact" });

                var vendorContactsToRemove = p.vendor_contact.Where(vc => vendor_contacts.Any(v => v.domain_id == vc.domain_id && v.vendor_contact_id == vc.vendor_contact_id));
                vendorContactsToRemove.ToList().ForEach(v =>
                {
                    var removed = p.vendor_contact.Remove(v);
                    if (!removed)
                    {
                        throw new Exception("Could not remove vendor contact");
                    }
                });
                repository.Update(p);
            }
        }

        [ActionName("DownloadPOCover")]
        public HttpResponseMessage GetPOCover(int id1, int id2, int id3)
        {
            try
            {
                using (var poRep = new PORepository(1))
                {
                    var path = poRep.CreatePODetailsPage(id1, id2, id3);
                    var fileStream = File.OpenRead(path);
                    var data = new MemoryStream();

                    fileStream.CopyTo(data);
                    data.Seek(0, SeekOrigin.Begin);

                    // Create response message with stream as its content
                    var message = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(data)
                    };

                    // Set content headers
                    message.Content.Headers.ContentLength = fileStream.Length;
                    message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    message.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "POCover" + id3.ToString() + ".pdf",
                        Size = fileStream.Length
                    };

                    return message;

                }
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }
    }
}