using System;
using System.Collections.Generic;
using System.Linq;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class SubcategoryRepository : IDisposable
    {
        private readonly audaxwareEntities _db;
        private bool _disposed = false;

        public SubcategoryRepository()
        {
            _db = new audaxwareEntities();
        }

        public IEnumerable<get_all_category_subcategories_Result> GetFromCategories(int domainId, int categoryDomainId, IEnumerable<int> categoriesId)
        {
            return _db.get_all_category_subcategories(domainId, (short)categoryDomainId, string.Join(";", categoriesId)).ToList();
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