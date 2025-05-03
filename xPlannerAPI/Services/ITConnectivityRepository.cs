using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using xPlannerAPI.Models;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class ITConnectivityRepository : IITConnectivityRepository, IDisposable
    {

        private audaxwareEntities _db;
        private bool _disposed = false;

        public ITConnectivityRepository()
        {
            this._db = new audaxwareEntities();
        }

        public List<AssetITConnectivity> GetAllConnections(int domainId, int projectId)
        {
            //var connectivity = _db.asset_it_connectivity.Where(x => x.project_id == projectId && x.domain_id == domainId).ToList();

            var connectivity =
               (from conn in _db.asset_it_connectivity
                join assetIn in _db.project_room_inventory on conn.inventory_id_in equals assetIn.inventory_id
                join assetOut in _db.project_room_inventory on conn.inventory_id_out equals assetOut.inventory_id
                join roomIn in _db.project_room on assetIn.room_id equals roomIn.room_id
                join roomOut in _db.project_room on assetOut.room_id equals roomOut.room_id
                where conn.domain_id == domainId && conn.project_id == projectId
                select new AssetITConnectivity {
                    asset_in_description = assetIn.asset_description,
                    room_name_in = roomIn.final_room_name,
                    room_number_in = roomIn.final_room_number,
                    asset_out_description = assetOut.asset_description,
                    room_name_out = roomOut.final_room_name,
                    room_number_out = roomOut.final_room_number,

                    conn = conn,
                    inventoryIn = assetIn,
                    inventoryOut = assetOut
                }).ToList();

            return connectivity;
        }

        public List<asset_inventory> GetAssetsOut(int domainId, int projectId, int ITConnectivityId, int inventoryIdIn)
        {
            var connectivity = _db.asset_inventory.Where(x => x.project_id == projectId && x.domain_id == domainId && x.inventory_id != inventoryIdIn && x.network_option == 1).ToList();

            return connectivity;
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