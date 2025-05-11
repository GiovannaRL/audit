using System;
using System.Collections.Generic;
using System.Linq;
using xPlannerCommon.Models;

namespace xPlannerCommon.Services
{
    public class ProceduresRepository : IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public ProceduresRepository()
        {
            this._db = new audaxwareEntities();
        }

        public IEnumerable<get_possible_associations_Result> GetAvailableAssociations(short domain_id, int project_id, int document_id, string type)
        {
            return this._db.get_possible_associations(domain_id, project_id, document_id, type).ToList();
        }

        public IEnumerable<get_doc_associations_names_Result> GetDocAssociations(short project_domain_id, int project_id, int document_id)
        {
            return this._db.get_doc_associations_names(project_domain_id, project_id, document_id).ToList();
        }

        public bool CleanUserProjects(AspNetUser user)
        {
            try
            {
                if (user?.Id != null)
                {
                    _db.clean_user_projects(user.Id);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateUserProject(AspNetUser newUser, short projectsDomain)
        {
            try
            {
                if (newUser.project_user != null && newUser.project_user.Any())
                {
                    _db.update_user_projects(newUser.Id, projectsDomain, string.Join(";", newUser.project_user.Select(p => p.project_id)), newUser.AspNetUserRoles.First().RoleId);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void UpdateDomainManufacturers(short domain_id, IEnumerable<manufacturer> manufacturers)
        {
            if (manufacturers != null && manufacturers.Any())
            {
                var groupedManufacturers = manufacturers.GroupBy(m => m.domain_id).Select(m => new
                {
                    domain_id = m.Key,
                    manufacturer_ids = string.Join(";", m.Select(m1 => m1.manufacturer_id))
                });

                foreach (var manufacturer in groupedManufacturers)
                {
                    _db.update_domain_manufacturers(domain_id, manufacturer.domain_id, manufacturer.manufacturer_ids);
                }
            }
            else {
                // will delete the existing manufacturers
                _db.update_domain_manufacturers(domain_id, null, null);
            }
        }

        public IEnumerable<get_associated_manufacturers_Result> GetAssociatedManufacturers(short domain_id)
        {
            return _db.get_associated_manufacturers(domain_id);
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
