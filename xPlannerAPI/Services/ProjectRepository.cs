using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using System.Data.Entity;
using System.Security.Claims;
using xPlannerAPI.Security.Extensions;
using xPlannerAPI.App_Data;

namespace xPlannerAPI.Services
{
    public class ProjectRepository : IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public ProjectRepository()
        {
            this._db = new audaxwareEntities();
        }


        public string DeleteTemplates(int domain_id, int? project_id = null)
        {
            try
            {
                var templates = _db.project_room.Where(x => x.project_id_template == project_id && x.project_domain_id_template == domain_id).ToList();
                foreach (var item in templates) { 
                    _db.project_room.Remove(item);
                    _db.SaveChanges();
                }
                return "";
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error trying to delete related templates. Exception {0}", ex);
                return "Error trying to delete related templates.";
            }
        }

        public bool AddProjectToUser(project item, int id1, string userName) {
            var user = _db.AspNetUsers.Include("AspNetUserRoles").Where(u => u.Email == userName).FirstOrDefault();
            var role = user.AspNetUserRoles.Where(x => x.domain_id == id1).FirstOrDefault();

            if (role.RoleId == "2")
            {
                var projectUser = new project_user();
                projectUser.user_pid = user.Id;
                projectUser.project_id = item.project_id;
                projectUser.project_domain_id = item.domain_id;
                _db.project_user.Add(projectUser);
                return _db.SaveChanges() > 0;

            }
            return true;
        }

        public bool UpdateAssetsBudget(project item, int domainId, int projectId) {

            try
            {
                _db.update_asset_budget((short)domainId, projectId, item.markup, item.escalation, item.tax, item.freight_markup, item.install_markup);

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error trying to update assets budget. Exception {0}", ex);
                return false;
            }

          
            
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

        internal IEnumerable<project> GetAll(int domainId, ClaimsIdentity identity)
        {
            try
            {
                var domainProjects = _db.projects.Include("user_project_mine")
                    .Where(p => domainId == p.domain_id && p.project_id != 1 && p.status != "R").ToList();
                var userProjects = from p in domainProjects where identity.CheckProjectAccess(domainId, p.project_id) select p;
                return userProjects;
            }
            catch (Exception ex)
            {
                Helper.RecordLog("ProjectRepository", "GetAll", ex);
                throw new ApplicationException(ex.Message);
            }
        }
    }
}