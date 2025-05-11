using System;
using System.Linq;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using System.Diagnostics;

namespace xPlannerAPI.Services
{
    public class PhaseRepository : IPhaseRepository
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public PhaseRepository()
        {
            _db = new audaxwareEntities();
        }

        public bool Delete(int domain_id, int? project_id, int? phase_id)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var rooms = _db.project_room.Where(pr => pr.domain_id == domain_id && pr.project_id == project_id && pr.phase_id == phase_id);
                    _db.project_room.RemoveRange(rooms);
                    _db.SaveChanges();

                    var phase = _db.project_phase.FirstOrDefault(ph => ph.domain_id == domain_id && ph.project_id == project_id && ph.phase_id == phase_id);
                    _db.project_phase.Remove(phase);
                    _db.SaveChanges();

                    transaction.Commit();

                    //AUDIT
                    using (var auditRep = new AuditRepository())
                    {
                        foreach (var room in rooms)
                        {
                            auditRep.CompareAndSaveAuditedData(room, null, "DELETE", new project_room(), "Deleted automatically when phase was deleted");
                        }
                        auditRep.CompareAndSaveAuditedData(phase, null, "DELETE", new project_phase());

                    }

                    return true;
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Error deleting phase. Exception {e.Message}");
                    transaction.Rollback();
                    return false;
                }
            }
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