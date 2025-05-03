using System;
using System.Linq;
using System.Diagnostics;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;
using xPlannerCommon.App_Data;

namespace xPlannerAPI.Services
{
    public class CopyFromRepository : ICopyFromRepository, IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public CopyFromRepository()
        {
            this._db = new audaxwareEntities();
        }

        public string Copy(short domain_id, int project_id, int phase_id, int department_id, int room_id, short cp_domain_id, int cp_project_id,
             int cp_phase_id, int cp_department_id, int cp_room_id, string addedBy, bool cp_opt_col)
        {
            try
            {
                var return_var = new ObjectParameter("return_var", "");
                this._db.copy_from_project(domain_id, project_id, phase_id, department_id, room_id, cp_domain_id,
                cp_project_id, cp_phase_id, cp_department_id, cp_room_id, addedBy, cp_opt_col, return_var);
                return return_var.Value.ToString();
            }
            catch (Exception ex)
            {
                Helper.RecordLog("CopyFromRepository", "Copy", ex);
                throw new ApplicationException(ex.Message);
            }
        }

        public string CopyProject(short domainId, int projectId, string projectDescription, bool copyUser, string addedBy)
        {
            try
            {
                _db.Database.CommandTimeout = 600000;
                var copy = _db.copy_project(domainId, projectId, projectDescription, copyUser, addedBy);
                return "";
            }
            catch (Exception ex)
            {
                Helper.RecordLog("CopyFromRepository", "CopyProject", ex);
                return ex.Message;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this._db.Dispose();
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