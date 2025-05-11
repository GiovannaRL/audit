using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class GridViewRepository : IDisposable
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public GridViewRepository()
        {
            _db = new audaxwareEntities();
        }

        public User_gridView Get(string userName, string type, string name, int domainId)
        {
            var view = _db.User_gridView.Where(
                x => x.added_by == userName && x.type == type && x.name == name && x.domain_id == domainId ||
                x.type == type && x.name == name && x.domain_id == 1
                ).First();

            return view;
        }

        public List<User_gridView> GetAll(string userName, string type, int domainId )
        {
            var views = _db.User_gridView.Where(
                x => x.type == type && x.domain_id == 1 ||
                x.added_by == userName && x.type == type && x.domain_id == domainId ||
                x.type == type && x.domain_id == domainId && x.is_private != true
                ).OrderBy(x => x.domain_id).ThenBy(x => x.name).ToList();

            foreach (var view in views)
            {
                if (view.is_private != true && view.added_by != userName)
                    view.name = view.name + " (shared)";
            }

            return views;
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