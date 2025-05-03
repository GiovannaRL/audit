using System;
using System.Collections.Generic;
using System.Linq;
using xPlannerAPI.Models;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using System.Data.Entity;

namespace xPlannerAPI.Services
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        // Constructor
        public PurchaseOrderRepository()
        {
            _db = new audaxwareEntities();
        }

        public List<get_purchase_orders_Result> GetAll(int domainId, int projectId, int? phaseId = null, int? departmentId = null, int? roomId = null)
        {
            return _db.get_purchase_orders((short)domainId, projectId, phaseId, departmentId, roomId).ToList();
        }

        public IEnumerable<Get_PO_assigned_assets_Result> GetAssignedAssets(short domainId, int projectId, int poId)
        {
            return _db.Get_PO_assigned_assets(domainId, projectId, poId).ToList();
        }

        public IEnumerable<get_expirated_pos_Result> GetExpirated(short domainId, string userId) {

            return _db.get_expirated_pos(userId, domainId);
        }


        public IEnumerable<get_available_assets_to_PO_Result> GetAssetsToAssign(int domainId, int projectId, int? phaseId = null, int? departmentId = null, int? roomId = null, bool? showUnapproved = false)
        {
            return _db.get_available_assets_to_PO(domainId, projectId, phaseId, departmentId, roomId, showUnapproved).ToList();
        }

        public void UpdateAssetCost(short poDomainId, int poId, int projectId, short assetDomainId, int assetId, int inventoryId, decimal? poUnitAmt)
        {
            _db.update_cost(poDomainId, projectId, assetDomainId, assetId, poId, inventoryId, poUnitAmt);

        }

        public bool AddAssets(int projectDomainId, int projectId, int poId, List<asset_inventory> ipo, string addedBy)
        {
            try
            {
                foreach (var item in ipo)
                {
                    _db.ins_po_asset((short)projectDomainId, projectId, item.asset_domain_id, item.asset_id, poId,
                        item.inventory_id > 0 ? item.inventory_id.ToString() : item.inventory_ids, item.budget_qty, 0, addedBy);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateInventoryData(int domainId, int inventoryId, string assetStatus, DateTime? deliveredDate, DateTime? receivedDate)
        {
            var inventory = _db.project_room_inventory.Where(x => x.inventory_id == inventoryId && x.domain_id == domainId).FirstOrDefault();
            if (inventory == null)
                return true;

            inventory.delivered_date = deliveredDate;
            inventory.received_date = receivedDate;
            inventory.current_location = assetStatus;
            
            _db.Entry(inventory).State = EntityState.Modified;
            return _db.SaveChanges() > 0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}