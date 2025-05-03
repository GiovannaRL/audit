using System;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class DuplicateRepository : IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public DuplicateRepository()
        {
            this._db = new audaxwareEntities();
        }

        public void DuplicateVendorItems(short from_domain_id, int from_vendor_id, short to_domain_id, int to_vendor_id)
        {
            this._db.copy_vendor_items(from_domain_id, from_vendor_id, to_domain_id, to_vendor_id);
        }

        public void DuplicateManufacturerItems(short from_domain_id, int from_manufacturer_id, short to_domain_id, int to_manufacturer_id, string added_by = null)
        {
            this._db.copy_manufacturer_items(from_domain_id, from_manufacturer_id, to_domain_id, to_manufacturer_id, added_by);
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