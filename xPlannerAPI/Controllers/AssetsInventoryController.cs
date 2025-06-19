using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using xPlannerAPI.Models;
using xPlannerAPI.Services;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using xPlannerAPI.Security.Attributes;
using System.Net.Http;
using xPlannerCommon.Enumerators;
using System.Net;
using System.Diagnostics;
using System;
using xPlannerAPI.App_Data;

namespace xPlannerAPI.Controllers
{
    [AudaxAuthorize(AreaModule = AreaModule.ProjectsAssets)]
    public class AssetsInventoryController : TableGenericController<asset_inventory>
    {

        [ActionName("AllInventories")]
        public IEnumerable<asset_inventory> GetAllInventories(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            return GetInventories(id1, id2, id3, id4, id5);
        }


        [ActionName("AllNetWorkInventoriesWithPortsAndITConnections")]
        public IEnumerable<asset_inventory> GetAllNetWorkInventoriesWithPortsAndITConnections(int id1, int? id2 = null)

        {
            return GetInventories(id1, id2).Where(x => x.it_connections < x.ports);
        }

        [ActionName("AllNetWorkInventories")]
        public IEnumerable<asset_inventory> GetAllNetworkInventories(int id1, int? id2 = null, int? id3 = null)
        {
            return GetInventories(id1, id2).Where(x => x.network_option == 1 && x.inventory_id != id3);
        }

        [ActionName("AllInventoriesAvailableForPO")]
        public IEnumerable<asset_inventory> GetAllInventoriesAvailableForPO(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null, [FromUri] bool? showOnlyApprovedAssets = true)
        {
            return GetInventories(id1, id2, id3, id4, id5).Where(x => x.po_qty == 0 && (showOnlyApprovedAssets == false || showOnlyApprovedAssets == true && x.current_location == "Approved"));
        }


        [ActionName("AllFilterRelocate")]
        public IEnumerable<asset_inventory> GetAllFilterRelocate(int id1, int? id2 = null, int? id3 = null)
        {
            return GetInventories(id1, id2).Where(x => x.target_location == null && (id3 == null || x.budget_qty == id3));
        }

        [ActionName("InventoryBudget")]
        public CalculateAssetBudget PutInventoryBudget(int id1, int id2, [FromBody] CalculateAssetBudget inventory)
        {
            using (IAssetInventoryRepository rep = new AssetInventoryRepository())
                return rep.CalculateBudget(inventory);
        }

        [ActionName("DocToLink")]
        public IEnumerable<asset_inventory> GetDocLink(short id1, int id2, int id3)
        {
            using (var repository = new AssetInventoryRepository())
            {
                var assets = repository.GetDocLink(id1, id2, id3);

                assets.ForEach(delegate (asset_inventory item)
                {
                    if (item.none_option != true && item.total_assets_options != 0 && item.option_descriptions == null)
                    {
                        item.option_descriptions = "Pending";
                        item.option_codes = "Pending";
                        item.options_price = "Pending";
                        item.asset_profile = "Options Pending";
                    }
                });
                return assets;
            }
        }

        [ActionName("LinkedToDoc")]
        public IEnumerable<asset_inventory> GetLinkedToDoc(short id1, int id2, int id3)
        {
            return base.GetAll(id1, id2).Where(a => a.linked_document == id3);
        }

        [ActionName("LinkedToDoc")]
        public bool DeleteLinkedToDoc(int id1, int id2)
        {
            using (var repository = new AssetInventoryRepository())
            {
                return repository.DeleteLinkedToDoc(id1, id2);
            }
        }

        /*
         * id1 = domain_id
         * id2 = project_id
         */
        [ActionName("All")]
        public HttpResponseMessage Put(int id1, int id2, [FromBody] EditMultipleData data)
        {
            using (IAssetInventoryRepository repository = new AssetInventoryRepository())
            {
                var msg = repository.EditMultiple(data);
                return Request.CreateResponse(msg == "" ? HttpStatusCode.OK : HttpStatusCode.BadRequest, msg);
            }
        }

        [ActionName("EditSingle")]
        public bool Put(int id1, int id2, [FromBody] project_room_inventory item)
        {
            using (IAssetInventoryRepository repository = new AssetInventoryRepository())
            {
                return repository.EditSingle(item);
            }
        }

        [ActionName("Synchronize")]
        public bool PutSynchronize(int id1, int id2, [FromBody] project_room_inventory item)
        {
            using (IAssetInventoryRepository repository = new AssetInventoryRepository())
            {
                return repository.Synchronize(item);
            }
        }

        [ActionName("GetSynchronizedCount")]
        public UniqueString GetSynchronizedCount(int id1, int id2, int id3, string id4, string id5)
        {
            using (IAssetInventoryRepository repository = new AssetInventoryRepository())
            {
                return new UniqueString() { text = repository.GetSynchronizedCount(id1, id2, id3, id4, id5).ToString() };
            }
        }

        [ActionName("LockCost")]
        public bool Put(int id1, int id2, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            using (IAssetInventoryRepository repository = new AssetInventoryRepository())
            {
                return repository.LockCost(id1, id2, id3, id4, id5);
            }
        }

        [ActionName("FromProjectWithBudgets")]
        public List<copy_single_item_inventory_Result> PostWithBudgets(short id1, int id2, int id3, int id4, int id5, int id6, [FromBody] List<asset_inventory> items)
        {
            if (items == null || !items.Any(i => i.budget_qty > 0))
                return null;

            using (var rep = new AssetInventoryRepository())
            {
                return rep.CopyMultipleInventory(items, id1, id2, id3, id4, id5, id6, UserName, true);
            }
        }

        [ActionName("FromProject")]
        public List<copy_single_item_inventory_Result> Post(short id1, int id2, int id3, int id4, int id5, int id6, [FromBody] List<asset_inventory> items)
        {
            if (items == null || !items.Any(i => i.budget_qty > 0))
                return null;

            using (var rep = new AssetInventoryRepository())
            {
                return rep.CopyMultipleInventory(items, id1, id2, id3, id4, id5, id6, UserName);
            }
        }

        /*
         * id1 = domain_id
         * id2 = project_id
         * id3 = action
         */
        [ActionName("FromProjectToMultipleLocations")]
        public List<copy_single_item_inventory_Result> PostMultipleLocations(short id1, int id2, int id3, [FromBody] CopyAssetsInventoryToMultipleLocationsRequest request, bool withBudgets = false)
        {
            try
            {
                if (request.items == null || !request.items.Any(i => i.budget_qty > 0))
                    return null;

                
                    var retLocations = new List<copy_single_item_inventory_Result>();
                    foreach (var location in request.locations)
                    {
                        using (var repository = new AssetInventoryRepository())
                        {
                            var result = repository.CopyMultipleInventory(request.items, id1, location.project_id, location.phase_id, location.department_id, location.room_id, id3, UserName, withBudgets);
                            retLocations.AddRange(result);
                        }
                    }

                    return retLocations;

                
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error to add asset(s) from a project. Domain = {id1}, Project = {id2}. FullError: {e}. InnerError: {e.InnerException}");
                throw;
            }      
            
        }

        [ActionName("LinkInventory")]
        public bool PostLinkInventory(short id1, int id2, int id3, [FromBody] asset_inventory item)
        {
            if (item == null)
                return false;

            using (var rep = new AssetInventoryRepository())
            {
                return rep.LinkInventory(item, id1, id2, id3);
            }
        }

        [HttpPost]
        [ActionName("LinkInventory")]
        public bool DeleteLinkInventory(short id1, int id2, [FromBody] int[] ids)
        {
            using (var rep = new AssetInventoryRepository())
            {
                return rep.DeleteLinkInventory(id1, id2, ids);
            }
        }

        /*
         * id1 = domain_id,
         * id3 = project_id
         * id3 = inventory_id
         */
        [ActionName("Profile")]
        public get_project_profiles_Result GetProfile(int id1, int id2, int id3)
        {
            using (var repository = new AssetInventoryRepository())
            {
                return repository.GetProfile(id3);
            }
        }

        [HttpPost]
        [ActionName("Multiple")]
        public HttpResponseMessage Delete([FromBody] List<AssetInventoryItemIds> items, int id1, int id2)
        {
            using (IAssetInventoryConsolidatedRepository repository = new AssetInventoryConsolidatedRepository())
            {
                bool allDeleted = true;
                foreach (AssetInventoryItemIds item in items)
                {
                    if (!repository.Delete(item.inventory_ids, id1, id2, item.phase_id, item.department_id, item.room_id)) {
                        allDeleted = false;
                    }                    
                }

                var status = HttpStatusCode.OK;
                if (!allDeleted) status = HttpStatusCode.Conflict;

                return Request.CreateResponse(status, allDeleted);
            }
        }


        private IEnumerable<asset_inventory> GetInventories(int id1, int? id2 = null, int? id3 = null, int? id4 = null, int? id5 = null)
        {
            var assets = base.GetAll(id1, id2, id3, id4, id5).ToList();

            assets.ForEach(delegate (asset_inventory item)
            {
                if (!item.none_option.GetValueOrDefault() && item.total_assets_options != 0 && item.option_descriptions == null)
                {
                    item.option_descriptions = "Pending";
                    item.option_codes = "Pending";
                    item.options_price = "Pending";
                    item.asset_profile = "Options Pending";
                }

            });
            return assets;
        }

        /**
         * id1 = domain_id]
         * id2 = project_id
         * id3 = inventory_id
         */
        [ActionName("Pictures")]
        public List<InventoryPictureInfo> GetPictures(int id1, int id2, int id3)
        {
            using (IDocumentRepository repository = new DocumentRepository())
            {
                return repository.GetInventoryPictures(id1, id2, id3);
            }
        }

        /**
         * id1 = domain_id
         * id2 = project_id
         * id3 = inventory_id 
         */
        [ActionName("Pictures")]
        public HttpResponseMessage PostImportPictures([FromBody] List<FileData> pictures, int id1, int id2, int id3)
        {

            if (pictures == null || !pictures.Any())
                return Request.CreateResponse(HttpStatusCode.BadRequest, Helper.GetMaxRequestLengthMessage());

            using (IAssetInventoryRepository inventoryRepository = new AssetInventoryRepository())
            {
                inventoryRepository.UploadInventoryPictures(id1, id2, id3, pictures);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "");
        }

        /**
         * id1 = domain_id
         * id2 = project_id
         */
        [HttpPut]
        [ActionName("Options")]
        public void PutOptions([FromBody] InventoriesOptions options, int id1, int id2)
        {
            using (IAssetInventoryRepository inventoryRepository = new AssetInventoryRepository())
            {
                inventoryRepository.UpdateOptions(id1, id2, options, User.Identity.Name);
            }
        }

        // id1 = domain_id
        // id2 = project_id
        // id3 = inventory_id
        // id4 = picture_id
        [HttpDelete]
        [ActionName("Picture")]
        public HttpResponseMessage DeletePicture(int id1, int id2, int id3, int id4)
        {
            try
            {
                using (IDocumentRepository docRepository = new DocumentRepository())
                {
                    docRepository.DeleteInventoryPicture(id1, id2, id3, id4);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error in AssetsInventoryController:DeletePicture. Domain = {id1}, Project = {id2}, Inventory = {id3}, Picture = {id4}. ErrorMessage: {e.Message}. InnerException: {e.InnerException}");
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        public AssetsInventoryController() : base(new[] { "inventory_id" },
            new[] { "domain_id", "project_id", "phase_id", "department_id", "room_id" })
        { }



        


    }
}