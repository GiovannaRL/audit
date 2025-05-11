using System;
using xPlannerAPI.Models;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class ReplaceInventoryRepository : IReplaceInventoryRepository
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public ReplaceInventoryRepository()
        {
            _db = new audaxwareEntities();
        }

        public bool Put(int domainId, int projectId, ReplaceInventory data)
        {
            var ret = true;

            foreach (var inventoryId in data.inventories_id)
            {
                var output = new System.Data.Entity.Core.Objects.ObjectParameter("statusLog", 9);
                _db.replace_inventory(domainId, projectId, inventoryId, data.new_asset_domain_id, data.new_asset_id, data.cost_col, data.budget, data.resp, output);
                if (output.Value.ToString() == "0")
                {
                    ret = false;
                }
            }

            return ret;
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