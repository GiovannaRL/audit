using System;
using System.Linq;
using xPlannerAPI.Models;
using xPlannerAPI.Interfaces;
using System.Data.Entity;
using xPlannerCommon.Models;
using xPlannerCommon.App_Data;
using System.Collections.Generic;
using xPlannerCommon.Services;
using System.Diagnostics;
using xPlannerCommon.Models.Enums;
using System.Data.Entity.Core;
using Newtonsoft.Json;
using System.Text;

namespace xPlannerAPI.Services
{
    public class AssetInventoryRepository : IAssetInventoryRepository, IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public AssetInventoryRepository()
        {
            this._db = new audaxwareEntities();
        }

        public void UpdateOptions(int domain_id, int project_id, InventoriesOptions options, string addedBy)
        {
            if (options.options != null && options.inventories_id != null && options.inventories_id.Any())
            {
                var allItems = string.Join(";", options.inventories_id.Concat(this._db.get_linked_inventories(string.Join(";", options.inventories_id))
                    .Select(i => i.inventory_id).ToList()));

                string inventories_id = string.Join(";",
                    this._db.filter_can_edit_options_item(allItems).Where(i => i != null).ToList()
                 );

                if (!string.IsNullOrEmpty(inventories_id))
                {
                    var oldInventoryOptions = _db.inventory_options.Where(x => inventories_id.Contains(x.inventory_id.ToString()) && x.domain_id == domain_id).ToList();

                    using (IOptionRepository optionRepo = new OptionRepository())
                    {
                        foreach (GenericOption op in options.options)
                        {
                            // If there is no option_id (option_id <= 0) means it's a new option and thus a custom option
                            if (op.option_id <= 0)
                            {
                                // Add new option
                                op.project_domain_id = (short)domain_id;
                                op.project_id = project_id;
                                var addedOption = optionRepo.AddCustomOption(op, addedBy);

                                if (addedOption != null)
                                {
                                    op.option_id = addedOption.asset_option_id;
                                    op.domain_id = addedOption.domain_id;
                                }
                            } else if (op.scope == AssetOptionScopeEnum.Custom)
                            {
                                using (TableRepository<assets_options> assetOptionRepo = new TableRepository<assets_options>())
                                {
                                    var option = assetOptionRepo.Get(new[] { "domain_id", "asset_option_id" }, new[] { op.domain_id, op.option_id }, null);
                                    option = op.MergeInfo(option);
                                    
                                    assetOptionRepo.Update(option);
                                    
                                    //AUDIT
                                    using (var auditRep = new AuditRepository())
                                    {
                                        auditRep.CompareAndSaveAuditedData(op, option, "UPDATE", null);
                                    }
                                }
                            }

                            this._db.update_inventories_option(op.domain_id, inventories_id, op.option_id, op.quantity, op.unit_price);
                            
                            

                            optionRepo.AddPicture(new GenericOption
                            {
                                domain_id = (short)domain_id,
                                option_id = op.option_id,
                                inventories_id = inventories_id
                            }, op.picture);
                        }
                    }

                    var newInventoryOptions = _db.inventory_options.Where(x => inventories_id.Contains(x.inventory_id.ToString()) && x.domain_id == domain_id).ToList();

                    //AUDIT
                    using (var auditRep = new AuditRepository())
                    {
                        foreach (GenericOption op in options.options)
                        {
                            var oldData = oldInventoryOptions.Where(x => x.option_id == op.option_id).FirstOrDefault();
                            var newData = newInventoryOptions.Where(x => x.option_id == op.option_id).FirstOrDefault();
                            if (oldData != null)
                            {
                                auditRep.CompareAndSaveAuditedData(oldData, newData, "UPDATE", new inventory_options());
                            }

                        }

                    }
                }

                this._db.delete_assets_options(allItems, string.Join(";", options.options.Select(io => io.option_id)));
            }
        }

        public List<asset_inventory> GetDocLink(short domain_id, int project_id, int document_id)
        {
            return _db.asset_inventory.Where(x => x.domain_id == domain_id && x.project_id == project_id).Where(y => y.linked_document == null || y.linked_document == document_id).ToList();
            //return this._db.get_inventory_doc_link(domain_id, project_id, document_id).ToList();
        }

        private bool ValidateDNPQtyForConsolidated(EditMultipleData data, List<asset_inventory> inventories) {
            
            if (data.edited_data.budget_qty > 0 || data.edited_data.dnp_qty > 0 || data.edited_data.lease_qty > 0)
            {
                if (inventories.Where(x => ((data.edited_data.budget_qty??x.budget_qty)??0) - ((data.edited_data.lease_qty??x.lease_qty)??0) < ((data.edited_data.dnp_qty??x.dnp_qty)??0)).Count() > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidatePlannedQtyForAssetsWithPOValueForConsolidated(EditMultipleData data, List<asset_inventory> inventories)
        {

            if (data.edited_data.budget_qty > 0)
            {
                if (inventories.Where(x => x.po_qty > 0 && data.edited_data.budget_qty != x.po_qty).Count() > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public string EditMultiple(EditMultipleData data)
        {
            try
            {
                // Necessary because we should not overwrite jsn data when edition is locked for edit mult
                if (!data.edited_data.jsn_ow.GetValueOrDefault())
                    data.edited_data.jsn_ow = null;

                var oldInventories = _db.asset_inventory.Where(x => data.inventories.Contains(x.inventory_id)).ToList();
                //VALIDATE DNP QTY FOR CONSOLIDATED
                if (!ValidateDNPQtyForConsolidated(data, oldInventories)) {
                    return "The quantity values provided are not valid. DNP Qty cannot be greater than planned qty - po qty";
                }

                if (!ValidatePlannedQtyForAssetsWithPOValueForConsolidated(data, oldInventories))
                    return "This asset is linked to a PO. The Planned Qty column should match the PO Qty";

                var inventories = data.inventories;

                while (data.inventories != null && data.inventories.Count() > 0)
                {
                    var inventories_range = data.inventories.GetRange(0, data.inventories.Count() > 500 ? 500 : data.inventories.Count());
                    if (data.inventories.Count() > 500)
                        data.inventories = data.inventories.GetRange(500, data.inventories.Count() - 500);
                    else
                        data.inventories = null;

                    string inventories_id = String.Join(";", inventories_range);

                    var newInventories = this._db.edit_multi_asset(inventories_id, data.edited_data.resp, data.edited_data.current_location,
                    data.edited_data.cost_center_id, data.edited_data.estimated_delivery_date, data.edited_data.comment,
                    data.edited_data.tag, data.edited_data.cad_id, data.edited_data.unit_budget, data.edited_data.none_option,
                    data.edited_data.detailed_budget, data.edited_data.lead_time, data.edited_data.clin, data.edited_data.unit_markup,
                    data.edited_data.unit_freight_net, data.edited_data.unit_freight_markup, data.edited_data.unit_escalation,
                    data.edited_data.unit_install_net, data.edited_data.unit_install_markup, data.edited_data.unit_tax,
                    data.edited_data.ECN, data.edited_data.placement, data.edited_data.placement_ow,
                    data.edited_data.temporary_location, data.edited_data.biomed_check_required,
                    data.edited_data.asset_description, data.edited_data.asset_description_ow, data.edited_data.budget_qty,
                    data.edited_data.lease_qty, data.edited_data.dnp_qty,
                    data.edited_data.jsn_ow, data.edited_data.manufacturer_description, data.edited_data.manufacturer_description_ow,
                    data.edited_data.serial_number, data.edited_data.serial_number_ow,
                    data.edited_data.serial_name, data.edited_data.serial_name_ow, data.edited_data.jsn_code,
                    data.edited_data.jsn_utility1, data.edited_data.jsn_utility2, data.edited_data.jsn_utility3,
                    data.edited_data.jsn_utility4, data.edited_data.jsn_utility5, data.edited_data.jsn_utility6,
                    data.edited_data.jsn_utility7, data.edited_data.@class, data.edited_data.class_ow, data.edited_data.final_disposition, data.edited_data.delivered_date, data.edited_data.received_date);


                    //AUDIT
                    using (var repository = new AuditRepository())
                    {
                        var newInventoriesConverted = JsonConvert.DeserializeObject<List<project_room_inventory>>(JsonConvert.SerializeObject(newInventories));
                        foreach (var inventory in oldInventories)
                        {
                            var newInventory = newInventoriesConverted.Where(x => x.inventory_id == inventory.inventory_id).FirstOrDefault();
                            repository.CompareAndSaveAuditedData(inventory, newInventory, "UPDATE", new project_room_inventory(), "EDIT MULTIPLE");
                        }

                    }
                }

                

                return "";
            }
            catch (Exception ex)
            {
                Helper.RecordLog("AssetInventoryRepository", "EditMultiple", ex);
                throw new ApplicationException(ex.Message);
            }
        }

        public bool EditSingle(project_room_inventory item)
        {
            try
            {
                //GET OLD VALUES
                var newInventory = _db.project_room_inventory.Where(x => x.inventory_id == item.inventory_id && x.domain_id == item.domain_id).FirstOrDefault();
                var oldInventory = Helper.Clone<project_room_inventory>(newInventory); 

                var return_save = SaveEditSingle(item);
                //check if it's template
                if (item.project_id == 1)
                {
                    var template = _db.project_room_inventory.Include("project_room").SingleOrDefault(x => x.inventory_id == item.inventory_id);
                    var template_id = template.project_room.id;

                    var inventories = _db.project_room_inventory.Include("asset")
                        .Where(x => x.linked_id_template == template_id && x.asset_id == template.asset_id && x.asset_domain_id == template.asset_domain_id).ToList();

                    foreach (var inventory in inventories)
                    {
                        SaveEditSingle(item, inventory);

                        //AUDIT
                        using (var repository = new AuditRepository())
                        {
                            repository.CompareAndSaveAuditedData(template, inventory, "UPDATE", null, "Updated automatically because is linked to a template (original was updated)");
                        }
                    }

                }

                //AUDIT
                using (var repository = new AuditRepository())
                {
                    repository.CompareAndSaveAuditedData(oldInventory, newInventory, "UPDATE");
                }
                
                return return_save;
            }
            catch (Exception ex)
            {
                Helper.RecordLog("AssetInventoryRepository", "EditSingle", ex);
                throw new ApplicationException(ex.Message);
            }
        }


        public bool Synchronize(project_room_inventory item)
        {
            try
            {
                _db.update_inventory_by_project_jsn(item.domain_id, item.project_id, item.inventory_id, item.jsn_code, item.resp);
                return true;
            }
            catch (Exception ex)
            {
                Helper.RecordLog("AssetInventoryRepository", "UpdateInventoryByJsnAndProject", ex);
                throw new ApplicationException(ex.Message);
            }
        }



        private void UpdateGovernmentFields(project_room_inventory item, project_room_inventory itemDB)
        {
            if (item.jsn_ow.GetValueOrDefault())
            {
                itemDB.jsn_code = item.jsn_code;
                itemDB.jsn_utility1 = item.jsn_utility1;
                itemDB.jsn_utility2 = item.jsn_utility2;
                itemDB.jsn_utility3 = item.jsn_utility3;
                itemDB.jsn_utility4 = item.jsn_utility4;
                itemDB.jsn_utility5 = item.jsn_utility5;
                itemDB.jsn_utility6 = item.jsn_utility6;
                itemDB.jsn_utility7 = item.jsn_utility7;
            }
            else
            {
                if (itemDB.asset.jsn_id != null)
                {
                    var jsn = _db.jsns.FirstOrDefault(x => x.Id == itemDB.asset.jsn_id && x.domain_id == itemDB.asset.jsn_domain_id);
                    if (jsn != null)
                    {
                        itemDB.jsn_code = jsn.jsn_code + (string.IsNullOrEmpty(itemDB.asset.jsn_suffix) ? "" : "." + itemDB.asset.jsn_suffix);
                    }
                }
                itemDB.jsn_utility1 = itemDB.asset.jsn_utility1;
                itemDB.jsn_utility2 = itemDB.asset.jsn_utility2;
                itemDB.jsn_utility3 = itemDB.asset.jsn_utility3;
                itemDB.jsn_utility4 = itemDB.asset.jsn_utility4;
                itemDB.jsn_utility5 = itemDB.asset.jsn_utility5;
                itemDB.jsn_utility6 = itemDB.asset.jsn_utility6;
            }
        }

        private bool SaveEditSingle(project_room_inventory item, project_room_inventory itemDB = null)
        {
            if (itemDB == null)
            {
                itemDB = this._db.project_room_inventory.Include("asset").FirstOrDefault(pri => pri.inventory_id == item.inventory_id);
            }

            // Sets the overwritable first
            itemDB.jsn_ow = item.jsn_ow;
            itemDB.asset_description_ow = item.asset_description_ow;
            itemDB.manufacturer_description_ow = item.manufacturer_description_ow;
            itemDB.serial_number_ow = item.serial_number_ow;
            itemDB.serial_name_ow = item.serial_name_ow;
            itemDB.depth_ow = item.depth_ow;
            itemDB.height_ow = item.height_ow;
            itemDB.mounting_height_ow = item.mounting_height_ow;
            itemDB.width_ow = item.width_ow;
            itemDB.connection_type_ow = item.connection_type_ow;
            itemDB.plug_type_ow = item.plug_type_ow;
            itemDB.lan_ow = item.lan_ow;
            itemDB.network_option_ow = item.network_option_ow;
            itemDB.network_type_ow = item.network_type_ow;
            itemDB.bluetooth_ow = item.bluetooth_ow;
            itemDB.cat6_ow = item.cat6_ow;
            itemDB.displayport_ow = item.displayport_ow;
            itemDB.dvi_ow = item.dvi_ow;
            itemDB.hdmi_ow = item.hdmi_ow;
            itemDB.wireless_ow = item.wireless_ow;
            itemDB.ports_ow = item.ports_ow;
            itemDB.volts_ow = item.volts_ow;
            itemDB.amps_ow = item.amps_ow;


            
            // These fields do not yet have lock, so we need to detect the change. We should modify these 
            // field to also have the little lock and make them behave like the ones above
            itemDB.placement_ow = !(string.IsNullOrEmpty(item.placement) && string.IsNullOrEmpty(itemDB.asset.placement)) && 
                (string.IsNullOrEmpty(item.placement) || !item.placement.Equals(itemDB.asset.placement));
            itemDB.class_ow = !(item.@class == itemDB.asset.@class) && (item.@class == null || !item.@class.Equals(itemDB.asset.@class));

            // After sets the overwritables, check if some of the properties that the overwritable refers had changed
            if (OverwritableFieldsHadChanged(itemDB, item))
            {
                // Anula o cutsheet para saber que ele precisa ser regerado
                itemDB.cut_sheet_filename = null;
            }

            itemDB.estimated_delivery_date = item.estimated_delivery_date;
            itemDB.delivered_date = item.delivered_date;
            itemDB.received_date = item.received_date;
            itemDB.unit_budget = item.unit_budget;
            itemDB.cad_id = item.cad_id != null ? item.cad_id : itemDB.cad_id;
            itemDB.budget_qty = item.budget_qty;
            itemDB.lease_qty = item.lease_qty;
            itemDB.dnp_qty = item.dnp_qty;
            itemDB.comment = item.comment;
            itemDB.tag = item.tag;
            itemDB.resp = item.resp;
            itemDB.cost_center_id = item.cost_center_id;
            itemDB.cost_center = null;
            itemDB.none_option = item.none_option;
            itemDB.detailed_budget = item.detailed_budget;
            itemDB.lead_time = item.lead_time;
            itemDB.clin = item.clin;
            itemDB.unit_markup = item.unit_markup;
            itemDB.unit_escalation = item.unit_escalation;
            itemDB.unit_freight_net = item.unit_freight_net;
            itemDB.unit_freight_markup = item.unit_freight_markup;
            itemDB.unit_install_net = item.unit_install_net;
            itemDB.unit_install_markup = item.unit_install_markup;
            itemDB.unit_tax = item.unit_tax;
            itemDB.ECN = item.ECN;
            itemDB.placement = item.placement;
            itemDB.biomed_check_required = item.biomed_check_required;
            itemDB.@class = item.@class;
            itemDB.asset_description = item.asset_description;
           
            UpdateGovernmentFields(item, itemDB);

            if (item.current_location.Equals("Approved"))
            {
                if (item.resp == null || (!item.resp.Equals("EXOI") && !item.resp.Equals("EXCI") && !item.resp.Equals("EXVI") && !item.resp.Equals("EXEX")))
                {
                    itemDB.current_location = item.current_location;
                }
            }
            else
            {
                itemDB.current_location = item.current_location;
            }

            itemDB.manufacturer_description = item.manufacturer_description;
            itemDB.serial_number = item.serial_number;
            itemDB.serial_name = item.serial_name;
            itemDB.temporary_location = item.temporary_location;
            itemDB.final_disposition = item.final_disposition;
            itemDB.depth = item.depth;
            itemDB.height = item.height;
            itemDB.mounting_height = item.mounting_height;
            itemDB.width = item.width;
            itemDB.connection_type = item.connection_type;
            itemDB.plug_type = item.plug_type;
            itemDB.lan = item.lan;
            itemDB.network_option = item.network_option;
            itemDB.network_type = item.network_type;
            itemDB.bluetooth = item.bluetooth;
            itemDB.cat6 = item.cat6;
            itemDB.displayport = item.displayport;
            itemDB.dvi = item.dvi;
            itemDB.hdmi = item.hdmi;
            itemDB.wireless = item.wireless;
            itemDB.ports = item.ports;
            itemDB.volts = item.volts;
            itemDB.amps = item.amps;

            _db.Entry(itemDB).State = EntityState.Modified;
            return _db.SaveChanges() > 0;

            /*if (itemDB.none_option != true)
            {
                UpdateOptions(itemDB.inventory_id.ToString(), item.inventory_options);
            }

            return returnValue;*/
        }

        private bool OverwritableFieldsHadChanged(project_room_inventory itemDB, project_room_inventory item) {
            return itemDB.placement != item.placement 
                || itemDB.cad_id != item.cad_id
                || itemDB.@class != item.@class
                || (item.asset_description_ow.GetValueOrDefault() && !itemDB.asset_description.Equals(item.asset_description))
                || (item.manufacturer_description_ow.GetValueOrDefault() && !itemDB.manufacturer_description.Equals(item.manufacturer_description))
                || (item.serial_number_ow.GetValueOrDefault() && !(itemDB.serial_number ?? "").Equals(item.serial_number ?? ""))
                || (item.serial_name_ow.GetValueOrDefault() && !(itemDB.serial_name ?? "").Equals(item.serial_name ?? ""))
                || (item.jsn_ow.GetValueOrDefault() && jsnChanged(itemDB, item));
        }

        private bool jsnChanged(project_room_inventory itemDB, project_room_inventory item) {

            return itemDB.jsn_code != item.jsn_code
                || itemDB.jsn_utility1 != item.jsn_utility1
                || itemDB.jsn_utility2 != item.jsn_utility2
                || itemDB.jsn_utility3 != item.jsn_utility3
                || itemDB.jsn_utility4 != item.jsn_utility4
                || itemDB.jsn_utility5 != item.jsn_utility5
                || itemDB.jsn_utility6 != item.jsn_utility6
                || itemDB.jsn_utility7 != item.jsn_utility7;
        }

        public bool LockCost(int domain_id, int project_id, int? phase_id, int? department_id, int? room_id)
        {
            using (TableRepository<project_room_inventory> repository = new TableRepository<project_room_inventory>())
            {
                var inventories = repository.GetAll(new string[] { "domain_id", "project_id", "phase_id", "department_id", "room_id" }, new int?[] { domain_id, project_id, phase_id, department_id, room_id }, new string[] { "project_room" }).ToList();
                inventories.ForEach(i => { i.locked_room_quantity = i.project_room.room_quantity; i.locked_unit_budget = i.unit_budget; i.locked_dnp_qty = i.dnp_qty; i.locked_budget_qty = i.budget_qty; _db.Entry(i).State = EntityState.Modified; });
            }
            return _db.SaveChanges() > 0;
        }

        private copy_single_item_inventory_Result CopyInventory(asset_inventory copyFrom, int project_id, int phase_id, int department_id, int room_id, int action, int quantity, string addedBy, bool withBudget = false)
        {
            int? inventory_source_id = null;
            if (action == 2) //relocated asset
                inventory_source_id = Convert.ToInt32(copyFrom.inventory_id);
            var result = new copy_single_item_inventory_Result();

            result = _db.copy_single_item_inventory(copyFrom.domain_id, copyFrom.project_id, copyFrom.phase_id, copyFrom.department_id,
                copyFrom.room_id, Convert.ToInt32(copyFrom.inventory_id), project_id, phase_id, department_id, room_id, quantity, true, addedBy, inventory_source_id, Convert.ToDecimal(copyFrom.total_unit_budget), withBudget).FirstOrDefault();

            return result;
        }


        public List<copy_single_item_inventory_Result> CopyMultipleInventory(List<asset_inventory> copyFrom, int domain_id, int project_id, int phase_id, int department_id, int room_id, int action, string addedBy, bool withBudget = false)
        {
            List<copy_single_item_inventory_Result> result = new List<copy_single_item_inventory_Result>();
            foreach (var item in copyFrom)
            {
                if (item.budget_qty.GetValueOrDefault() > 0)
                {
                    result.Add(CopyInventory(item, project_id, phase_id, department_id, room_id, action, item.budget_qty.GetValueOrDefault(), addedBy, withBudget));
                }
            }

            return result;            
        }

        public bool LinkInventory(asset_inventory sourceInventory, int domainId, int projectId, int targetInventoryId)
        {
            // We should never change this order as updating the source first makes the source data be copied to the target
            // on the initial link
            var inventory = _db.project_room_inventory.Where(x => x.inventory_id == sourceInventory.inventory_id && x.domain_id == sourceInventory.domain_id).FirstOrDefault();
            inventory.inventory_target_id = targetInventoryId;
            _db.Entry(inventory).State = EntityState.Modified;
            if (_db.SaveChanges() != 1)
            {
                Trace.TraceError("Error to establish link, source update failed");
                return false;
            }

            _db.update_link_inventory(inventory.domain_id, inventory.inventory_id);


            // Please notice that the asset inventory links (properties, picture, etc) happen with triggers. Please see the update_link_inventory
            // stored procedure, which is called on triggers on project_room_inventory.
            var targetInventory = _db.project_room_inventory.Where(x => x.inventory_id == targetInventoryId && x.domain_id == domainId).FirstOrDefault();
            targetInventory.inventory_source_id = Convert.ToInt32(sourceInventory.inventory_id);
            _db.Entry(targetInventory).State = EntityState.Modified;
            if (_db.SaveChanges() != 1)
            {
                Trace.TraceError("Error to establish link, target update failed");
                return false;
            }
            var userData = new AspNetUsersRepository();
            _db.update_link_insert_inventory_pictures(inventory.domain_id, inventory.inventory_id, userData.GetLoggedUserEmail(), targetInventory.inventory_id, null);
            return true;
        }

        public bool DeleteLinkInventory(int domainId, int projectId, int[] inventoryIds)
        {
            var userData = new AspNetUsersRepository();
            var userId = userData.GetLoggedUserId();

            foreach (var inventoryId in inventoryIds)
            {
                _db.unlink_inventory((short)domainId, inventoryId, userId);
            }
            return true;
        }

        public get_project_profiles_Result GetProfile(int inventory_id)
        {
            project_room_inventory asset = this._db.project_room_inventory.Find(inventory_id);

            return this._db.get_project_profiles(asset.domain_id, asset.project_id, asset.asset_domain_id, asset.asset_id)
                .Where(pr => pr.detailed_budget == asset.detailed_budget && pr.profile_text == asset.asset_profile
                && (!asset.detailed_budget || pr.profile_budget == asset.asset_profile_budget)).FirstOrDefault();
        }

        public int GetSynchronizedCount(int domainId, int projectId, int inventoryId, string jsnCode, string resp)
        {
            var total = _db.project_room_inventory.Where(x => x.project_id == projectId && x.domain_id == domainId && x.jsn_code == jsnCode && x.inventory_id != inventoryId && x.resp.Trim() == resp.Trim()).ToList();

            return total.Count();
        }


        public bool DeleteLinkedToDoc(int domain_id, int inventory_id)
        {
            var inventory = this._db.project_room_inventory.Where(p => p.inventory_id == inventory_id && p.domain_id == domain_id).FirstOrDefault();

            inventory.linked_document = null;
            _db.Entry(inventory).State = EntityState.Modified;
            bool returnValue = _db.SaveChanges() > 0;

            return returnValue;
        }


        public CalculateAssetBudget CalculateBudget(CalculateAssetBudget inventory)
        {
            inventory.unit_markup_calc = CalcUnitMarkup(inventory);
            inventory.unit_escalation_calc = CalcEscalation(inventory);
            inventory.unit_budget_adjusted = CalcUnitBudgetAdjusted(inventory);
            inventory.unit_tax_calc = CalcUnitTax(inventory);
            inventory.unit_install = CalcUnitInstall(inventory);
            inventory.unit_freight = CalcUnitFreight(inventory);
            inventory.unit_budget_total = CalcUnitBudgetTotal(inventory);

            if (inventory.type_resp == "NEW")
            {
                inventory.total_install_net = CalcTotalInstallNet(inventory);
                inventory.total_budget_adjusted = CalcTotalBudgetAdjusted(inventory);
                inventory.total_tax = CalcTotalTax(inventory);
                inventory.total_install = CalcTotalInstall(inventory);
                inventory.total_freight_net = CalcTotalFreightNet(inventory);
                inventory.total_freight = CalcTotalFreight(inventory);
                inventory.total_budget = CalcTotalBudget(inventory);
            }
            else
            {
                inventory.total_install_net = 0;
                inventory.total_budget_adjusted = 0;
                inventory.total_tax = 0;
                inventory.total_install = 0;
                inventory.total_freight_net = 0;
                inventory.total_freight = 0;
                inventory.total_budget = 0;
            }

            return inventory;

        }

        private decimal CalcUnitMarkup(CalculateAssetBudget inventory)
        {
            var calc = ((inventory.unit_markup ?? 0) / 100) * (inventory.total_unit_budget ?? 0);
            return calc;
        }

        private decimal CalcEscalation(CalculateAssetBudget inventory)
        {
            var calc = ((inventory.unit_escalation ?? 0) / 100) * (inventory.total_unit_budget ?? 0);
            return calc;
        }

        private decimal? CalcUnitBudgetAdjusted(CalculateAssetBudget inventory)
        {
            var calc = (inventory.total_unit_budget ?? 0) + inventory.unit_markup_calc + inventory.unit_escalation_calc;
            return calc;
        }

        private decimal? CalcUnitTax(CalculateAssetBudget inventory)
        {
            var calc = inventory.unit_budget_adjusted * ((inventory.unit_tax ?? 0) / 100); ;
            return calc;
        }

        private decimal? CalcUnitInstall(CalculateAssetBudget inventory)
        {
            var calc = (inventory.unit_install_net ?? 0) * (1 + ((inventory.unit_install_markup ?? 0) / 100));
            return calc;
        }

        private decimal? CalcUnitFreight(CalculateAssetBudget inventory)
        {
            var calc = (inventory.unit_freight_net ?? 0) * (1 + ((inventory.unit_freight_markup ?? 0) / 100)); ;
            return calc;
        }

        private decimal? CalcUnitBudgetTotal(CalculateAssetBudget inventory)
        {
            var calc = inventory.unit_budget_adjusted + inventory.unit_freight + inventory.unit_install + inventory.unit_tax_calc;
            return calc;
        }

        private decimal? CalcTotalBudgetAdjusted(CalculateAssetBudget inventory)
        {
            var calc = inventory.unit_budget_adjusted * inventory.net_new;
            return calc;
        }

        private decimal? CalcTotalTax(CalculateAssetBudget inventory)
        {
            var calc = inventory.unit_tax_calc * inventory.net_new;
            return calc;
        }

        private decimal? CalcTotalInstallNet(CalculateAssetBudget inventory)
        {
            var calc = (inventory.unit_install_net ?? 0) * inventory.net_new;
            return calc;
        }

        private decimal? CalcTotalInstall(CalculateAssetBudget inventory)
        {
            var calc = inventory.unit_install * inventory.net_new;
            return calc;
        }

        private decimal? CalcTotalFreight(CalculateAssetBudget inventory)
        {
            var calc = inventory.unit_freight * inventory.net_new;
            return calc;
        }

        private decimal? CalcTotalFreightNet(CalculateAssetBudget inventory)
        {
            var calc = inventory.unit_freight_net * inventory.net_new;
            return calc;
        }

        private decimal? CalcTotalBudget(CalculateAssetBudget inventory)
        {
            var calc = inventory.total_freight + inventory.total_tax + inventory.total_budget_adjusted + inventory.total_install;
            return calc;
        }

        public List<string> GetPictures(int domain_id, int project_id, int inventory_id)
        {
            return _db.documents_associations.Include("project_documents.document_types.documents_display_levels").Where(da => da.project_domain_id == domain_id
                    && da.project_id == project_id && da.inventory_id == inventory_id
                    && da.project_documents.document_types.documents_display_levels.description.Equals(DocumentDisplayLevelEnum.Inventory))
                .Select(da => da.project_documents.blob_file_name).ToList();
        }

        private void InsertInventoryPicutureAssociation(project_room_inventory item, FileData picture)
        {
            if (item != null && picture != null)
            {
                var userData = new AspNetUsersRepository();
                this._db.add_inventory_picture(item.domain_id, item.project_id, picture.fileName,
                    $"{picture.fileName}.{picture.fileExtension}", picture.fileExtension, item.inventory_id, picture.fileType, picture.label, userData.GetLoggedUserEmail());
            }
        }

        public void UploadInventoryPictures(int domain_id, int project_id, int inventory_id, List<FileData> pictures)
        {

            if (pictures == null || !pictures.Any()) return;

            project_room_inventory item = this._db.project_room_inventory.Where(x => x.domain_id == domain_id && x.project_id == project_id && x.inventory_id == inventory_id).FirstOrDefault();

            if (item == null || item.domain_id != domain_id || item.project_id != project_id)
            {
                Trace.TraceError($"No inventory with id {inventory_id} was found.");
                throw new ObjectNotFoundException($"No inventory with id {inventory_id} was found");
            }

            using (var repositoryBlob = new FileStreamRepository())
            {
                string container = $"photo{item.domain_id}";
                foreach (FileData picture in pictures)
                {
                    try
                    {
                        picture.fileName = repositoryBlob.UploadBase64Hashed(container, $"inventory_pic_{item.domain_id}_{item.project_id}",
                            picture.base64File, picture.fileExtension);
                        InsertInventoryPicutureAssociation(item, picture);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError($"Error to save inventory's picture " + picture.fileName, e);
                        throw e;
                    }
                }
            }
        }
        

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}