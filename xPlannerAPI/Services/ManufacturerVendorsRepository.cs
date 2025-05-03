using System;
using System.Collections.Generic;
using System.Linq;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class ManufacturerVendorsRepository : IDisposable
    {

        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public ManufacturerVendorsRepository()
        {
            _db = new audaxwareEntities();
        }

        public List<vendor> GetAllVendors(short manufacturerDomainId, int manufacturerId)
        {
            return _db.vendors.Where(v => v.assets_vendor.Any(av => av.asset.manufacturer_domain_id == manufacturerDomainId && av.asset.manufacturer_id == manufacturerId)).ToList();
        }

        public List<manufacturer> GetAllManufacturers(short vendorDomainId, int vendorId)
        {
            return _db.manufacturers.Where(m => m.assets.Any(a => a.assets_vendor.Any(av => av.vendor_domain_id == vendorDomainId && av.vendor_id == vendorId))).ToList();
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