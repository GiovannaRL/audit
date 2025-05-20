using System;
using System.Linq;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Claims;
using xPlannerAPI.Models;
using xPlannerAPI.Security.Extensions;
using xPlannerAPI.App_Data;

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

        public IEnumerable<DepartmentTreeView> GetPhaseAsTable(int domainId, ClaimsIdentity identity)
        {
            try
            {
                var departmentList = new List<DepartmentTreeView>();
                var departments = _db.project_department.Include("project_phase.project").Where(d => d.domain_id == domainId && d.project_id > 0);
                var phaseEmpty = _db.project_phase.Include("project_department").Where(p => p.domain_id == domainId && p.project_department.Count == 0);

                foreach (var dept in departments)
                {
                    DepartmentTreeView tree = new DepartmentTreeView();
                    tree.domain_id = dept.domain_id;
                    tree.project_id = dept.project_id;
                    tree.phase_id = dept.phase_id;
                    tree.phase_desc = dept.project_phase.description;
                    tree.department_id = dept.department_id;
                    tree.department_desc = dept.description;

                    departmentList.Add(tree);
                };
                foreach (var phase in phaseEmpty)
                {
                    DepartmentTreeView aux = new DepartmentTreeView();
                    aux.domain_id = phase.domain_id;
                    aux.project_id = phase.project_id;
                    aux.phase_id = phase.phase_id;
                    aux.phase_desc = phase.description;
                    aux.department_id = -1;
                    aux.department_desc = "Create New Department";

                    departmentList.Add(aux);
                };
                return departmentList.Where(p => identity.CheckProjectAccess(p.domain_id, p.project_id));
            }
            catch (Exception ex)
            {
                Helper.RecordLog("TreeViewRepository", "GetPhases", ex);
                throw new ApplicationException(ex.Message);

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