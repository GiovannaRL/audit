using System;
using System.Collections.Generic;
using System.Linq;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class InventoryOptionRepository : IInventoryOptionRepository
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public InventoryOptionRepository()
        {
            _db = new audaxwareEntities();
        }

        public IEnumerable<assets_options> UpdateAddMulti(int inventoriesDomainId, List<int> inventoriesIds)
        {
            var returnData = new List<assets_options>();

            var optionsIds = _db.inventory_options.Where(ivo => ivo.inventory_domain_id == inventoriesDomainId &&
                 inventoriesIds.Contains(ivo.inventory_id))
                .GroupBy(ivo => new { ivo.option_id, ivo.domain_id }).Select(ivo => new { values = ivo.Key, count = ivo.Count() });

            var db1 = new audaxwareEntities();

            foreach (var op in optionsIds)
            {
                if (op.count == inventoriesIds.Count)
                    returnData.Add(db1.assets_options.FirstOrDefault(ao => ao.asset_option_id == op.values.option_id && ao.domain_id == op.values.domain_id));
            }

            return returnData;
        }

        public bool DeleteFromMulti(int inventoriesDomainId, List<int> inventories, int optionDomainId, int optionId)
        {
            _db.inventory_options.RemoveRange(_db.inventory_options.Where(ivo => ivo.inventory_domain_id == 
                inventoriesDomainId && inventories.Contains(ivo.inventory_id)
                && ivo.domain_id == optionDomainId && ivo.option_id == optionId));

            return _db.SaveChanges() > 0;
        }

        public List<assets_options> GetAllToAdd(int domainId, int assetId, int? inventoryId, List<int> inventoriesIds)
        {
            if (inventoryId != null)
                return _db.assets_options.Where(ao => ao.domain_id == domainId && ao.asset_id == assetId &&
                    !ao.inventory_options.Any(ivo => ivo.inventory_id == inventoryId)).ToList();

            return _db.assets_options.Where(ao => ao.domain_id == domainId && ao.asset_id == assetId &&
                !ao.inventory_options.Any(ivo => inventoriesIds.Contains(ivo.inventory_id))).ToList();
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