using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using xPlannerCommon.App_Data;
using xPlannerCommon.Models;

namespace xPlannerAPI.Services
{
    public class CategorySubcategoryRepository : IDisposable
    {
        private audaxwareEntities _db;
        private bool _disposed = false;

        public CategorySubcategoryRepository()
        {
            this._db = new audaxwareEntities();
        }

        public IEnumerable<joined_category_subcategory> GetAll(short categoryDomainId, short subCategoryDomainId)
        {
            var arrCat = new int[] { (Helper.ShowAudaxWareInfo(categoryDomainId) ? 1 : categoryDomainId), categoryDomainId };
            var arrSub = new int[] { (Helper.ShowAudaxWareInfo(categoryDomainId) ? 1 : subCategoryDomainId), subCategoryDomainId };
            var categories = _db.joined_category_subcategory.Where(x => arrCat.Contains(x.category_domain_id) && arrSub.Contains(x.subcategory_domain_id)).ToList();

            return categories;
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