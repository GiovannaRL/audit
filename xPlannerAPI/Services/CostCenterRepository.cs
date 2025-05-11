using System;
using System.Data.Entity;
using System.Linq;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using xPlannerCommon.App_Data;

namespace xPlannerAPI.Services
{
    public class CostCenterRepository : ICostCenterRepository, IDisposable
    {
        private audaxwareEntities db;
        private bool _disposed = false;

        public CostCenterRepository()
        {
            this.db = new audaxwareEntities();
        }

        public bool updateAllAssets(int domain_id, int project_id, int cost_center_id)
        {
            try
            {
                var assets = this.db.project_room_inventory.Where(pri => pri.domain_id == domain_id && pri.project_id == project_id);

                foreach (project_room_inventory a in assets)
                {
                    a.cost_center_id = cost_center_id;
                    this.db.Entry(a).State = EntityState.Modified;
                }

                this.db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Helper.RecordLog("CostCenterRepository", "updateAllAssets", ex);
                throw new ApplicationException(ex.Message);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this.db.Dispose();
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