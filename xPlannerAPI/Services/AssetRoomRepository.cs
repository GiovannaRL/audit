using System;
using System.Collections.Generic;
using System.Linq;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class AssetRoomRepository : IAssetRoomRepository
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public AssetRoomRepository()
        {
            _db = new audaxwareEntities();
        }


        public List<get_asset_rooms_Result> Get(int domain_id, int projectId, int phaseId, int departmentId, int roomId, string assetIds)
        {
            return _db.get_asset_rooms(projectId, domain_id, assetIds, phaseId, departmentId, roomId).ToList();
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