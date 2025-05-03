using System;
using System.Collections.Generic;
using System.Linq;
using xPlannerAPI.Interfaces;
using System.Data.Entity;
using xPlannerCommon.Models;
using xPlannerCommon.App_Data;
using xPlannerAPI.Models;
using System.Reflection;

namespace xPlannerAPI.Services
{
    public class AssetInventoryConsolidatedRepository : IAssetInventoryConsolidatedRepository, IDisposable
    {

        private audaxwareEntities _db;
        private bool _disposed = false;

        public AssetInventoryConsolidatedRepository()
        {
            this._db = new audaxwareEntities();
        }

        public List<asset_inventory> GetAll(int domainId, int projectId, int? phaseId, int? departmentId, int? roomId, string[] groupBy, bool? FilterPoQty, bool? showOnlyApprovedAssets)
        {
            try
            {
                var queryGenarator = new AssetInventoryConsolidatedQueryGeneratorService();
                string consolidatedQuery = queryGenarator.CreateQuery(domainId, projectId, phaseId, departmentId, roomId, groupBy, FilterPoQty, showOnlyApprovedAssets);
                List<asset_inventory> assets = this._db.Database.SqlQuery<asset_inventory>(consolidatedQuery).ToList();
                var columns = new List<string>() { "source_location", "target_location", "asset_description", "unit_budget_total", "total_unit_budget", "unit_budget_adjusted", "unit_escalation", "unit_escalation_calc", "unit_freight", "unit_freight_net", "unit_freight_markup", "unit_install", "unit_install_net", "unit_install_markup", "unit_markup", "unit_markup_calc", "unit_tax", "unit_tax_calc" };

                assets.ForEach(delegate (asset_inventory item)
                {
                    //this was made here, because then try to use count distinct inside the consolidated query we lose performance
                    item.phase_description = item.phases_qty.Split(',').Distinct().Count() > 1 ? "Multiple" : item.phase_description;
                    item.department_description = item.departments_qty.Split(',').Distinct().Count() > 1 ? "Multiple" : item.department_description;
                    item.room_name = item.room_number = item.rooms_qty.Split(',').Distinct().Count() > 1 ? "Multiple" : item.room_name;
                    SetColumnsToMultiple(item, columns);


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
            catch (Exception ex)
            {
                Helper.RecordLog("AssetInventoryConsolidatedRepository", "GetAll", ex);
                throw new ApplicationException(ex.Message);
            }

        }

        private void SetColumnsToMultiple(object item,  List<string> propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                PropertyInfo property = item.GetType().GetProperty(propertyName);

                if (property != null && property.PropertyType == typeof(string))
                {
                    string value = (string)property.GetValue(item)??"";
                    string newValue = value.Split(';').Distinct().Count() > 1 ? "Multiple" : value.Split(';').First();

                    property.SetValue(item, newValue);
                }
            }
        }

        public void Add(string cost_field, project_room_inventory assetData)
        {

            try
            {
                //CHECK COST FIELD
                string cost_col = cost_field;
                string cost_col_linked = cost_field;

                var room = _db.project_room.Find(assetData.project_id, assetData.department_id, assetData.room_id, assetData.phase_id, assetData.domain_id);
                var linked_rooms = new List<project_room>();
                decimal? unit_budget_linked = null;

                if (room.is_template == true)
                    linked_rooms = _db.project_room.Where(a => a.linked_template == true && a.applied_id_template == room.id).ToList();

                var asset = _db.assets.Find(assetData.asset_id, assetData.asset_domain_id);

                if (cost_col == "default")
                {
                    cost_col = _db.projects.Find(assetData.project_id, assetData.domain_id).default_cost_field;
                    if (linked_rooms.Count > 0)
                    {
                        cost_col_linked = _db.projects.Find(linked_rooms.First().project_id, linked_rooms.First().domain_id).default_cost_field;
                        unit_budget_linked = GetUnitBudget(cost_col_linked, asset);
                    }
                }

                if (assetData.resp == null)
                    assetData.resp = asset.default_resp;

                if (assetData.current_location == null)
                    assetData.current_location = "Plan";

                decimal? unit_budget = GetUnitBudget(cost_col, asset);


                //CHECK IF THE EQUIPMENT EXISTS ALREADY
                var exist_asset = _db.project_room_inventory.Where(x => x.project_id == assetData.project_id && x.domain_id == assetData.domain_id && x.phase_id == assetData.phase_id && x.department_id == assetData.department_id && x.room_id == assetData.room_id)
                                        .Where(x => x.asset_id == assetData.asset_id && x.asset_domain_id == assetData.asset_domain_id).FirstOrDefault();


                if (exist_asset == null || assetData.comment == "1")
                {
                    var data = AddInventory(assetData, unit_budget);
                    _db.project_room_inventory.Add(data);
                    _db.SaveChanges();

                    foreach (var item in linked_rooms)
                    {
                        assetData.project_id = item.project_id;
                        assetData.phase_id = item.phase_id;
                        assetData.department_id = item.department_id;
                        assetData.room_id = item.room_id;
                        assetData.domain_id = item.domain_id;
                        assetData.linked_id_template = item.applied_id_template;
                        var data2 = AddInventory(assetData, unit_budget_linked);
                        _db.project_room_inventory.Add(data2);
                        _db.SaveChanges();
                    }

                }
                else
                {
                    exist_asset.budget_qty = exist_asset.budget_qty + assetData.budget_qty;
                    //exist_asset.relocate_qty = exist_asset.relocate_qty + assetData.relocate_qty;
                    exist_asset.lease_qty = exist_asset.lease_qty + assetData.lease_qty;
                    if (assetData.unit_budget == null || assetData.unit_budget == 0)
                        exist_asset.unit_budget = unit_budget;

                    _db.Entry(exist_asset).State = EntityState.Modified;
                    _db.SaveChanges();

                    foreach (var item in linked_rooms)
                    {
                        var linked_asset = _db.project_room_inventory.Where(x => x.project_id == item.project_id && x.domain_id == item.domain_id && x.phase_id == item.phase_id && x.department_id == item.department_id && x.room_id == item.room_id)
                                        .Where(x => x.asset_id == assetData.asset_id && x.asset_domain_id == assetData.asset_domain_id).FirstOrDefault();

                        linked_asset.budget_qty = linked_asset.budget_qty + assetData.budget_qty;
                        linked_asset.lease_qty = linked_asset.lease_qty + assetData.lease_qty;
                        if (assetData.unit_budget == null || assetData.unit_budget == 0)
                            linked_asset.unit_budget = unit_budget_linked;

                        _db.Entry(linked_asset).State = EntityState.Modified;
                        _db.SaveChanges();
                    }

                }

            }
            catch (Exception ex)
            {
                Helper.RecordLog("AssetInventoryConsolidatedRepository", "Add", ex);
                throw new ApplicationException(ex.Message);
            }
        }

        private decimal? GetUnitBudget(string cost_col, asset asset)
        {
            decimal? unit_budget = 0;

            switch (cost_col)
            {
                case "min_cost":
                    unit_budget = asset.min_cost;
                    break;
                case "max_cost":
                    unit_budget = asset.max_cost;
                    break;
                case "avg_cost":
                    unit_budget = asset.avg_cost;
                    break;
                case "last_cost":
                    unit_budget = asset.last_cost;
                    break;
            }

            return unit_budget;
        }

        private project_room_inventory AddInventory(project_room_inventory assetData, decimal? unit_budget)
        {
            var cost_center = _db.cost_center.Where(c => c.project_id == assetData.project_id && c.is_default == true).FirstOrDefault();

            assetData.date_added = DateTime.Now;
            if (cost_center != null)
                assetData.cost_center_id = cost_center.id;
            if (assetData.unit_budget == null || assetData.unit_budget == 0)
                assetData.unit_budget = unit_budget;


            if (assetData.comment == "1")
                assetData.comment = null;

            //set de budget default values from project to new asset
            var project = _db.projects.Where(x => x.domain_id == assetData.domain_id && x.project_id == assetData.project_id).FirstOrDefault();
            assetData.unit_markup = project.markup;
            assetData.unit_escalation = project.escalation;
            assetData.unit_tax = project.tax;
            assetData.unit_freight_markup = project.freight_markup;
            assetData.unit_install_markup = project.install_markup;

            return assetData;
        }
        
        public bool Delete(string inventory_ids, int domain_id, int project_id, int phase_id, int department_id, int room_id)
        {

            bool allDeleted = true;
            var ids = inventory_ids.Split(',');

            //check first if it's template
            if (project_id == 1)
            {
                foreach (var inventory_id in ids)
                {
                    var template = _db.project_room_inventory.Include("project_room").SingleOrDefault(x => x.inventory_id.ToString() == inventory_id);
                    var template_id = template.project_room.id;

                    var pos = _db.inventory_purchase_order.Include("project_room_inventory")
                    .Where(x => (x.project_room_inventory.linked_id_template == template_id && x.project_room_inventory.asset_id == template.asset_id && x.project_room_inventory.asset_domain_id == template.asset_domain_id) || x.project_room_inventory.inventory_id.ToString() == inventory_id).ToList();

                    foreach (var item in pos)
                    {
                        if (item.po_qty == 0)
                        {
                            _db.inventory_purchase_order.Remove(item);
                            _db.SaveChanges();
                        }
                    }

                    if (pos.Count() > 0)
                    {
                        allDeleted = false;
                    }
                    else
                    {
                        var inventories = _db.project_room_inventory
                            .Where(x => (x.linked_id_template == template_id && x.asset_id == template.asset_id && x.asset_domain_id == template.asset_domain_id) || x.inventory_id.ToString() == inventory_id).ToList();

                        foreach (var item in inventories)
                        {
                            var inventory = _db.project_room_inventory.Where(x => x.inventory_target_id == item.inventory_id && x.domain_id == domain_id).ToList();
                            foreach (var i in inventory)
                            {
                                i.inventory_target_id = null;
                                _db.Entry(i).State = EntityState.Modified;
                                _db.SaveChanges();
                            }

                            _db.project_room_inventory.Remove(item);
                            _db.SaveChanges();

                            //AUDIT
                            using (var repository = new AuditRepository())
                            {
                                repository.CompareAndSaveAuditedData(item, null, "DELETE", new project_room_inventory());
                            }
                        }
                        
                    }
                }
            }
            else
            {


                var pos = _db.inventory_purchase_order.Include("project_room_inventory")
            .Where(x => x.project_room_inventory.domain_id == domain_id && x.project_id == project_id)
            .Where(item => ids.Contains(item.inventory_id.ToString()));

                if (phase_id != -1)
                    pos = pos.Where(x => x.project_room_inventory.phase_id == phase_id);
                if (department_id != -1)
                    pos = pos.Where(x => x.project_room_inventory.department_id == department_id);
                if (room_id != -1)
                    pos = pos.Where(x => x.project_room_inventory.room_id == room_id);

                var assets = _db.project_room_inventory.Where(x => x.domain_id == domain_id && x.project_id == project_id)
                    .Where(item => ids.Contains(item.inventory_id.ToString()));
                if (phase_id != -1)
                    assets = assets.Where(x => x.phase_id == phase_id);
                if (department_id != -1)
                    assets = assets.Where(x => x.department_id == department_id);
                if (room_id != -1)
                    assets = assets.Where(x => x.room_id == room_id);

                foreach (var item in assets.ToList())
                {
                    // Delete Assset IT Connectivities
                    var connectivities = _db.asset_it_connectivity.Where(x => x.inventory_id_in == item.inventory_id || x.inventory_id_out == item.inventory_id);

                    foreach (var connectivity in connectivities)
                    {
                        _db.asset_it_connectivity.Remove(connectivity);                        
                    }

                    if (Array.Exists(pos.ToArray(), po => po.inventory_id == item.inventory_id && po.po_qty == 0))
                    {
                        var po_del = pos.Where(x => x.po_qty == 0 && x.inventory_id == item.inventory_id).FirstOrDefault();

                        _db.inventory_purchase_order.Remove(po_del);
                        _db.SaveChanges();
                    }

                    if (!Array.Exists(pos.ToArray(), po => po.inventory_id == item.inventory_id))
                    {
                        var inventory = _db.project_room_inventory.Where(x => x.inventory_target_id == item.inventory_id && x.domain_id == domain_id).ToList();
                        foreach (var i in inventory)
                        {
                            i.inventory_target_id = null;
                            _db.Entry(i).State = EntityState.Modified;
                            _db.SaveChanges();
                        }
                        _db.project_room_inventory.Remove(item);
                        _db.SaveChanges();

                        //AUDIT
                        using (var repository = new AuditRepository())
                        {
                            repository.CompareAndSaveAuditedData(item, null, "DELETE", new project_room_inventory());
                        }
                    }
                    else
                    {
                        allDeleted = false;
                    }
                }
            }
            return allDeleted;
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