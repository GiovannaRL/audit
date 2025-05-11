using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using xPlannerAPI.Interfaces;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class ProjectMineRepository : IProjectMineRepository
    {

        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public ProjectMineRepository()
        {
            _db = new audaxwareEntities();
        }
        public bool Remove(int domainId, int projectId, string userId)
        {
            var proj = _db.user_project_mine.Find(userId, projectId, domainId);

            if (proj == null)
                return false;

            _db.user_project_mine.Remove(proj);
            _db.Entry(proj).State = EntityState.Deleted;
            _db.SaveChanges();
            return true;

        }

        public IEnumerable<user_project_mine> GetMyProjects(int domainId,  string userId)
        {
            return _db.user_project_mine.Where(x => x.domain_id == domainId && x.userId == userId).ToList();
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