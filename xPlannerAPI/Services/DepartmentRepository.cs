using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using xPlannerAPI.Interfaces;
using xPlannerAPI.Security.Extensions;
using xPlannerCommon.App_Data;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class DepartmentRepository : IDepartmentRepository, IDisposable
    {
        private audaxwareEntities db;
        private bool disposed = false;

        public DepartmentRepository()
        {
            this.db = new audaxwareEntities();
        }

        public List<department_inventory_po_Result> GetWithFinancials(int domain_id, int project_id, int phase_id)
        {
            return this.db.department_inventory_po(domain_id, project_id, phase_id).ToList();
        }

        public IEnumerable<project_department> GetDepartmentAsTable(int domainId, ClaimsIdentity identity)
        {
            try
            {
                var domainProjects = db.project_department.Include("project_phase.project")
                    .Where(pd => pd.domain_id== domainId && pd.project_id > 1).ToList();
                return domainProjects.Where(p => identity.CheckProjectAccess(p.domain_id, p.project_id));
            }
            catch (Exception ex)
            {
                Helper.RecordLog("TreeViewRepository", "GetDepartments", ex);
                throw new ApplicationException(ex.Message);
            }
        }

        public bool Delete(int domain_id, int? project_id, int? phase_id, int? department_id)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var rooms = db.project_room.Where(pr => pr.domain_id == domain_id
                                                            && pr.project_id == project_id
                                                            && pr.phase_id == phase_id
                                                            && pr.department_id == department_id);
                    db.project_room.RemoveRange(rooms);
                    db.SaveChanges();

                    var department = db.project_department.FirstOrDefault(pd => pd.domain_id == domain_id
                                                                                && pd.project_id == project_id
                                                                                && pd.phase_id == phase_id
                                                                                && pd.department_id == department_id);
                    db.project_department.Remove(department);
                    db.SaveChanges();
                    transaction.Commit();

                    //AUDIT
                    using (var auditRep = new AuditRepository())
                    {
                        foreach (var room in rooms)
                        {
                            auditRep.CompareAndSaveAuditedData(room, null, "DELETE", new project_room(), "Deleted automatically when department was deleted");
                        }
                        auditRep.CompareAndSaveAuditedData(department, null, "DELETE", new project_department());

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
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}