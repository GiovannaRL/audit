using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using xPlannerAPI.Services;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using xPlannerCommon.Services;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using xPlannerAPI.Security.Services;
using System.Web;
using xPlannerAPI.Security.Attributes;
using xPlannerCommon.Enumerators;
using xPlannerAPI.Models;
using HelperAPI = xPlannerAPI.App_Data.Helper;
using xPlannerCommon.App_Data;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.AssetsCatalogAssets)]
    public class AssetsController : TableGenericController<asset>
    {
        public AssetsController() : base(new[] { "domain_id", "asset_id" }, new[] { "domain_id" }, new[] { "manufacturer", "assets_subcategory.assets_category", "jsn" }, true) { }

        [ActionName("Summarized")]
        public IEnumerable<asset_summarized> GetAll(int id1)
        {
            if (!SetDomain(id1))
            {
                Trace.TraceError("Error to set domain to {0}", id1);
                return null;
            }
            using (var repository = new TableRepository<asset_summarized>())
            {
                IEnumerable<asset_summarized> data = repository.GetAll(new[] { "domain_id" }, new[] { id1 }, null, true);
                if (id1 != Helper.AudaxwareDomainId)
                {
                    if (isLoggedAsManufacturer())
                    {
                        return data.Where(a => HasManufacturerAccess(a.manufacturer_domain_id, a.manufacturer_id)
                            && !a.approval_pending.GetValueOrDefault());
                    }

                    return data.Where(a => !a.approval_pending.GetValueOrDefault());
                }

                return data;
            }
        }

        [ActionName("SummarizedSingle")] // id2 = asset_id
        public asset_summarized GetSingle(int id1, int id2)
        {
            using (var repository = new TableRepository<asset_summarized>())
            {
                asset_summarized asset = repository.Get(new[] { "domain_id", "asset_id" }, new[] { id1, id2 }, null);

                if (!isLoggedAsManufacturer() || HasManufacturerAccess(asset.manufacturer_domain_id, asset.manufacturer_id))
                {
                    return asset;
                }

                return null;
            }
        }

        protected override bool UpdateReferences(asset item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (item.asset_id == 0)
            {
                using (IAssetRepository repository = new AssetRepository())
                {
                    item.asset_code = repository.GetNextCode(id1, item.asset_code);
                }
            }
            else
            {
                if (item.discontinued != true)
                {
                    item.alternate_asset = null;
                }

                item.avg_cost = (item.max_cost + item.min_cost) / 2;

                asset current = base.GetItem(item.domain_id, item.asset_id);
                if (current.min_cost != item.min_cost || current.max_cost != item.max_cost)
                {
                    item.last_budget_update = DateTime.Now;
                }
            }

            if (item.default_resp == null)
                item.default_resp = "OFOI";

            if (item.eq_measurement_id == null)
                item.eq_measurement_id = 1;

            string category;
            using (var repository = new TableRepository<assets_category>())
                category = repository.Get(new[] { "domain_id", "category_id" }, new[] { item.assets_subcategory.category_domain_id, item.assets_subcategory.category_id }, null).description;

            if (category != item.assets_subcategory.description)
                category = category + ", " + item.assets_subcategory.description;

            item.asset_description = category + (string.IsNullOrEmpty(item.asset_suffix) ? "" : ", " + item.asset_suffix);
            item.manufacturer_id = item.manufacturer.manufacturer_id;
            item.manufacturer_domain_id = item.manufacturer.domain_id;
            item.manufacturer = null;
            item.subcategory_id = item.assets_subcategory.subcategory_id;
            item.subcategory_domain_id = item.assets_subcategory.domain_id;
            item.assets_subcategory = null;
            item.updated_at = DateTime.Today;

            return true;
        }

        [ActionName("Jacks")]
        public HttpResponseMessage Get(int id1)
        {
            using (IAssetRepository repository = new AssetRepository())
            {
                var jacks = repository.GetJacks(id1);

                return Request.CreateResponse(HttpStatusCode.OK, jacks.Select(j => new
                {
                    j.asset_id,
                    j.domain_id,
                    j.asset_code,
                    j.serial_number,
                    j.serial_name,
                    j.asset_description,
                }));
            }
        }

        public override asset Add([FromBody] asset item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var db = new audaxwareEntities();
            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    var manufacturer = item.manufacturer;
                    var subcategory = item.assets_subcategory;
                    item.asset_description = "-";

                    var returnAsset = base.Add(item, id1, id2, id3, id4, id5);

                    using (var aVendorRepository = new TableRepository<assets_vendor>())
                    {
                        var vendors = aVendorRepository.GetAll(null, new int[] { }, null);

                        using (var assetRepository = new TableRepository<asset>())
                        {
                            var assetIds = assetRepository.GetAll(new[] { "domain_id", "manufacturer_id", "manufacturer_domain_id" }, new[] { item.domain_id, item.manufacturer_id, item.manufacturer_domain_id }, null).Select(a => a.asset_id);

                            foreach (var vendor in vendors.Where(v => assetIds.Contains(v.asset_id)).Select(v => new { v.vendor_id, v.vendor_domain_id }).GroupBy(v => new { v.vendor_id, v.vendor_domain_id }))
                            {
                                var itemAdd = new assets_vendor
                                {
                                    asset_id = returnAsset.asset_id,
                                    vendor_id = vendor.FirstOrDefault()?.vendor_id ?? 0
                                };
                                itemAdd.min_cost = itemAdd.max_cost = itemAdd.avg_cost = 0;
                                itemAdd.date_added = DateTime.Now;
                                itemAdd.added_by = returnAsset.added_by;
                                itemAdd.comment = "Auto added";
                                itemAdd.asset_domain_id = returnAsset.domain_id;
                                itemAdd.vendor_domain_id = vendor.FirstOrDefault()?.vendor_domain_id ?? 0;

                                aVendorRepository.Add(itemAdd);
                            }
                        }
                    }

                    dbTransaction.Commit();

                    returnAsset.manufacturer = manufacturer;
                    returnAsset.assets_subcategory = subcategory;

                    try
                    {
                        using (var repository = new CutSheetRepository(returnAsset.domain_id))
                        {
                            repository.BuildFullFromZero(returnAsset, BlobContainersName.FullCutsheet(returnAsset.domain_id), returnAsset.asset_id.ToString() + returnAsset.domain_id.ToString() + ".pdf");
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"Error to create cutsheet for asset {returnAsset.asset_code}", ex.Message);
                    }

                    return returnAsset;
                }
                catch (Exception e)
                {
                    dbTransaction.Rollback();
                    Trace.TraceError($"Error in AssetsController:Add. ErrorMessage: {e.Message}. InnerException: {e.InnerException}");
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);

                }
            }
        }

        [ActionName("Item")]
        public override asset Put(asset item, int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            if (item.domain_id == 1 && id1 != 1)
            {
                using (IAssetRepository repository = new AssetRepository())
                {
                    var oldAsset = item.asset_code;
                    var duplicatedAsset = repository.DuplicateAsset(item.asset_id, id1, true, UserName);
                    repository.DuplicateFiles(item, duplicatedAsset);
                    item.asset_id = duplicatedAsset.asset_id;
                    item.domain_id = duplicatedAsset.domain_id;
                    item.asset_code = duplicatedAsset.asset_code;
                    item.added_by = UserName;
                    item.cut_sheet = duplicatedAsset.cut_sheet;

                    var domain = new DomainsController();
                    var url = HttpContext.Current.Request.Url.Host;
                    var sendEmail = new EmailService();
                    var message = new Microsoft.AspNet.Identity.IdentityMessage
                    {
                        Destination = System.Web.Configuration.WebConfigurationManager.AppSettings["support_email"],
                        Subject = "[AudaxWare - Asset duplicated] "
                    };
                    message.Body += "<p>The Audaxware asset " + oldAsset + " was duplicated.</p>";
                    message.Body += "<p><strong>Request made by:</strong> " + UserName + "</p>";
                    message.Body += "<p><strong>Enterprise:</strong> " + domain.GetItem(id1).name + "</p>";
                    message.Body += "<p><strong>Site:</strong> " + url + "</p>";
                    message.Body += "<p><strong>New Asset Code:</strong> " + item.asset_code + "</p>";

                    sendEmail.SendAsync(message);
                }
            }
            else {
                using (IAssetRepository repository = new AssetRepository())
                    repository.ResetControlColumnToRegenerateCoverSheet(item);

                if (item.asset_code.Length == 3)
                {
                    using (IAssetRepository repository = new AssetRepository())
                        item.asset_code = repository.GetNextCode(id1, item.asset_code);
                }
            }

            item = base.Put(item, id1, id2, id3, id4, id5);

            using (var repository = new CutSheetRepository(item.domain_id))
            {
                repository.BuildFullFromZero(item, BlobContainersName.FullCutsheet(item.domain_id), item.asset_id.ToString() + item.domain_id + ".pdf");
            }

            return item;
        }

        [ActionName("Duplicate")]
        public asset PutDuplicate(int id1, int id2, int id3, bool? id4 = false)
        {
            var item = base.GetItem(id1, id2);
            if (item == null)
                return null;

            using (IAssetRepository repository = new AssetRepository())
            {
                var duplicatedAsset = repository.DuplicateAsset(item, id3, false, UserName, id4 ?? false);
                using (var cutRep = new CutSheetRepository(duplicatedAsset.domain_id))
                {
                    cutRep.BuildFullFromZero(duplicatedAsset, BlobContainersName.FullCutsheet(duplicatedAsset.domain_id), duplicatedAsset.asset_id.ToString() + duplicatedAsset.domain_id.ToString() + ".pdf");
                }

                return duplicatedAsset;
            }
        }

        /**
         * id1 = domain of the asset to be duplicated
         * id2 = id of the asset to be duplicated
         */
        [ActionName("ApproveRequest")]
        public HttpResponseMessage PutDuplicateToAW(int id1, int id2, bool? id3 = false)
        {
            asset item = base.GetItem(id1, id2);
            if (item == null)
                return null;

            using (IAssetRepository repository = new AssetRepository())
            {
                short oldDomain = item.approval_pending_domain.GetValueOrDefault();
                get_related_assets_Result related_asset = new get_related_assets_Result();

                if (id3 == false) { 
                    item.approval_pending_domain = null;
                    repository.UpdateAssetSimple(item);

                    related_asset.asset_code = item.asset_code;
                    related_asset.asset_id = item.asset_id;
                    related_asset.domain_id = item.domain_id;
                }
                else
                {
                    related_asset = repository.UpdateAudaxWareAsset(item);
                    Delete(id1, id2);
                }

                //get all users from the old asset domain
                using (TableRepository<AspNetUser> userRepository = new TableRepository<AspNetUser>())
                {
                    var emails = userRepository.GetAll(new string[] { }, GetIds(null), new string[] { }).Where(x => x.domain_id == oldDomain).Select(x => x.Email).ToList();
                    emails.Add("customerservice@audaxware.com");

                    foreach (var email in emails)
                    {
                        var sendEmail = new EmailService();
                        var message = new Microsoft.AspNet.Identity.IdentityMessage
                        {
                            Destination = email,
                            Subject = "[AudaxWare - Request to add asset to Audaxware] "
                        };
                        message.Body += "<p>The request for " + (id3 == true ? "updating" : "creating") + " asset " + item.created_from + " has been completed.</p>";
                        if (id3 == true)
                        {
                            message.Body += "<p>The AudaxWare asset " + related_asset.asset_code + " has been update with " + item.created_from + " information.</p>";
                        }
                        else
                        {
                            message.Body += "<p>The asset has been added to AudaxWare Database as " + item.asset_code + ".</p>";
                        }
                        
                        message.Body += "<p>Thank you for choosing xPlanner.</p>";
                        message.Body += "<p></p><p>AudaxWare Team</p>";

                        sendEmail.SendAsync(message);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, related_asset);
            }
        }

        [ActionName("DenyRequest")]
        public HttpResponseMessage PutDuplicateToAW(int id1, int id2, bool id3, string id4)
        {
            asset item = base.GetItem(id1, id2);
            if (item == null)
                return null;

            using (IAssetRepository repository = new AssetRepository())
            {
                short oldDomain = item.approval_pending_domain.GetValueOrDefault();
                if(!repository.DeleteRelatedAsset(item))
                    return Request.CreateResponse(HttpStatusCode.Conflict);

                //get all users from the old asset domain
                using (TableRepository<AspNetUser> userRepository = new TableRepository<AspNetUser>())
                {
                    var emails = userRepository.GetAll(new string[] { }, GetIds(null), new string[] { }).Where(x => x.domain_id == oldDomain).Select(x => x.Email).ToList();
                    emails.Add("juliana.barros@audaxware.com");

                    foreach (var email in emails)
                    {
                        var sendEmail = new EmailService();
                        var message = new Microsoft.AspNet.Identity.IdentityMessage
                        {
                            Destination = email,
                            Subject = "[AudaxWare - Request to add asset to Audaxware] "
                        };
                        message.Body += "<p>The request for " + (id3 == true ? "updating" : "creating") + " asset " + item.created_from + " was denied.</p>";
                        if (id3 == true)
                        {
                            message.Body += "<p>The AudaxWare asset was not updated with " + item.created_from + " information.</p>";
                        }
                        else
                        {
                            message.Body += "<p>The asset was not added to AudaxWare Database as " + item.asset_code + ".</p>";
                        }

                        message.Body += "<p>Denying comment: " + id4 + ".</p>";

                        message.Body += "<p>Thank you for choosing xPlanner.</p>";
                        message.Body += "<p></p><p>AudaxWare Team</p>";

                        sendEmail.SendAsync(message);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [ActionName("Item")]
        public override asset GetItem(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<asset>())
            {
                var returnAsset = repository.Get(new[] { "domain_id", "asset_id" }, GetIds(id1, id2), new[] { "manufacturer", "assets_subcategory.assets_category", "assets1", "jsn" });
                if (returnAsset == null || 
                    (isLoggedAsManufacturer() && !HasManufacturerAccess(returnAsset.manufacturer_domain_id, returnAsset.manufacturer_id)))
                {
                    return null;
                }

                returnAsset.assets1 = returnAsset.assets1.Where(ra => HasDomainAccess(ra.domain_id)).ToList();
                if (returnAsset.jsn != null)
                {
                    returnAsset.jsn.utility1 = returnAsset.jsn_utility1_ow == true ? returnAsset.jsn_utility1 : returnAsset.jsn.utility1;
                    returnAsset.jsn.utility2 = returnAsset.jsn_utility2_ow == true ? returnAsset.jsn_utility2 : returnAsset.jsn.utility2;
                    returnAsset.jsn.utility3 = returnAsset.jsn_utility3_ow == true ? returnAsset.jsn_utility3 : returnAsset.jsn.utility3;
                    returnAsset.jsn.utility4 = returnAsset.jsn_utility4_ow == true ? returnAsset.jsn_utility4 : returnAsset.jsn.utility4;
                    returnAsset.jsn.utility5 = returnAsset.jsn_utility5_ow == true ? returnAsset.jsn_utility5 : returnAsset.jsn.utility5;
                    returnAsset.jsn.utility6 = returnAsset.jsn_utility6_ow == true ? returnAsset.jsn_utility6 : returnAsset.jsn.utility6;
                }
                return returnAsset;
            }
        }

        [ActionName("Item")]
        public override HttpResponseMessage Delete(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (var repository = new TableRepository<project_room_inventory>())
            {
                var asset = base.GetItem(id1, id2, id3, id4, id5);
                if (asset == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                if (repository.GetAll(new[] { "asset_domain_id", "asset_id" }, GetIds(id1, id2), null).Any())
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "This asset is assigned to inventory and cannot be deleted");
                }

                base.Delete(id1, id2, id3, id4, id5);

                return Request.CreateResponse(HttpStatusCode.OK);

            }
        }

        [ActionName("Import")]
        public async Task<HttpResponseMessage> PostImport(short id1)
        {
            try
            {
                var timestamp = DateTime.Now;
                var directoryPath = Path.Combine(Domain.GetRoot(), "ImportFiles", "Assets");
                using (var fileRep = new FileStreamRepository())
                {
                    fileRep.CreateLocalDirectory(directoryPath);
                    var filePath = Path.Combine(directoryPath, string.Format("{0}_{1}_{2}.xlsx", id1, timestamp.ToShortDateString().Replace("/", ""), timestamp.ToShortTimeString().Replace(":", "")));
                    using (var fileStream = File.Create(filePath))
                    {
                        var requestStreamXX = await Request.Content.ReadAsMultipartAsync();
                        var requestStream = requestStreamXX.Contents[0].ReadAsStreamAsync().Result;
                        requestStream.CopyTo(fileStream);
                    }
                    using (var assetRep = new AssetRepository())
                    {
                        await assetRep.ImportData(id1, filePath, UserId);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (IOException e)
            {
                if (e.InnerException != null && e.InnerException is HttpException && (e.InnerException as HttpException).WebEventCode == 3004)
                        {
                       var message = string.Format(StringMessages.ImportFileSizeExceeded, HelperAPI.GetMaxRequestLength());
                    return Request.CreateResponse(HttpStatusCode.RequestEntityTooLarge, new { ErrorMessage = message });
                }

                throw e;
            }
        }

        [ActionName("AddAssetRequest")]
        public HttpResponseMessage PutAssetRequest(RequestData requestData, int id1, int id2)
        {
            try
            {
                using (IAssetRepository repository = new AssetRepository())
                {
                    asset data = base.GetItem(id1, id2);

                    var newAsset = repository.DuplicateAssetWithAWApproval(data, 1, false, UserName, true, (requestData.requestType == 1 ? true : false));
                    using (var cutRep = new CutSheetRepository(newAsset.domain_id))
                    {
                        cutRep.BuildFullFromZero(newAsset, BlobContainersName.FullCutsheet(newAsset.domain_id), newAsset.asset_id.ToString() + newAsset.domain_id + ".pdf");
                    }

                    using (TableRepository<domain> domainRepository = new TableRepository<domain>())
                    {
                        var sendEmail = new EmailService();
                        var message = new Microsoft.AspNet.Identity.IdentityMessage
                        {
                            Destination = System.Web.Configuration.WebConfigurationManager.AppSettings["support_email"],
                            Subject = "[AudaxWare - Request to add asset to Audaxware] "
                        };
                        message.Body += "<p>There is a request to add the following asset to Audaxware enterprise. After the analysis please return to the customer.</p>";
                        message.Body += "<p><strong>Request made by:</strong> " + UserName + "</p>";
                        message.Body += "<p><strong>Enterprise:</strong> " + domainRepository.Get(new string[] { "domain_id" }, new int[] { id1 }, null)?.name + "</p>";
                        message.Body += "<p><strong>Link to approved:</strong> " + HelperAPI.GetAssetCatalogURL(newAsset) + "</p>";
                        message.Body += "<p><strong>Source Asset Code:</strong> " + data.asset_code + "</p>";
                        message.Body += "<p><strong>Comment:</strong> " + requestData.comment + "</p>";

                        sendEmail.SendAsync(message);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [ActionName("CustomizedDifferences")]
        public List<AssetsDifferencesStructure> GetCustomizedDifferences(int id1, int id2)
        {
            return AssetRepository.GetCustomizedDifferences(id1, id2);
        }

        [ActionName("Project")]
        public IEnumerable<ProjectQty> GetProjects(int id1, int id2, int id3)
        {
            using (IAssetRepository repository = new AssetRepository())
            {
                return repository.GetProjects(id1, id2, id3);
            }
        }
    }
}