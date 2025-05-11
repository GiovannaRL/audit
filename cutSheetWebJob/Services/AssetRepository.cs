using System;
using xPlannerCommon.Models;

namespace cutSheetWebJob.Services
{
    public class AssetRepository : IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;
        
        public AssetRepository()
        {
            this._db = new audaxwareEntities();
        }

        public AssetRepository(short domain_id)
        {
            this._db = new audaxwareEntities();
            using (var cmd = this._db.Database.Connection.CreateCommand())
            {
                this._db.Database.Connection.Open();
                cmd.CommandText = "EXEC sp_set_session_context 'domain_id', '" + domain_id + "';";
                cmd.ExecuteNonQuery();
            }
        }

        public bool UpdateCutSheetValue(asset item, string value)
        {
            if (item != null)
            {
                item.cut_sheet = value;
                this._db.Entry(item).State = System.Data.Entity.EntityState.Modified;

                return this._db.SaveChanges() > 0;
            }

            return false;
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
